using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using SharpPcap.LibPcap;
using SharpPcap;
using System.Diagnostics;

using Microsoft.Extensions.Hosting;
using PacketDotNet;

namespace PCapture
{
    public sealed class MainHostedService : IHostedService
    {
        // Packet capture
        LibPcapLiveDevice _device;
        private readonly int _devIdx;

        // MQTT publisher
        MqttPublisher _mqttPublisher;

        private Task? _executingTask;
        private CancellationTokenSource? _serviceCts;

        public MainHostedService(int devIdx, MqttPublisher mqttPublisher)
        {
            _devIdx = devIdx;
            _mqttPublisher = mqttPublisher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 패킷 캡처
            _device = LibPcapLiveDeviceList.Instance[_devIdx];
            _device.Open();
            _device.OnPacketArrival += device_OnPacketArrival; // 인스턴스 메서드로 변경
            _device.StartCapture();

            // MQTT 시작
            _mqttPublisher.Start();

            _serviceCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = WatchLoopAsync(_serviceCts.Token);

        }

        private void device_OnPacketArrival(object sender, PacketCapture e)
        {
            Console.WriteLine("[Capture] packet arrived");

            var packet = PacketDotNet.Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            var ipPacket = packet.Extract<PacketDotNet.IPPacket>(); // IP 계층 패킷 추출

            PacketInfo packetInfo = new PacketInfo();
            packetInfo.GetPacketInfo(packet, ipPacket);

            _mqttPublisher.PublishMqttMessage(); // 인스턴스 필드 사용
        }

        private async Task WatchLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 100ms마다 체크하며 대기 (CPU 점유율 폭발 방지)
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 취소 신호에 의해 Task가 종료될 때 발생하는 정상이벤트이므로 무시
                Console.WriteLine("[PCapture] 대기 루프가 안전하게 정지되었습니다.");
            }
        }        

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_device is not null)
            {
                // _device.StopCapture();
                // _device.OnPacketArrival -= device_OnPacketArrival;

                // _mqttPublisher.Stop();

                try
                {
                    _device.StopCapture();
                    _device.OnPacketArrival -= device_OnPacketArrival;
                    _device.Close();

                    _mqttPublisher.Stop();
                }
                catch (Exception ex)
                
                {
                    Console.WriteLine($"장치 정리 중 오류 발생: {ex.Message}");
                }

                Console.WriteLine("[PCapture] 모든 자원이 정리되었습니다.");

            }
        }
    }

}
