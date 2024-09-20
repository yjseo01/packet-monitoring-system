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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_device is not null)
            {
                _device.StopCapture();
                _device.OnPacketArrival -= device_OnPacketArrival;

                _mqttPublisher.Stop();
            }
        }
    }

}
