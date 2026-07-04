using System.Text.Json;
using Microsoft.Extensions.Hosting;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace PCapture
{
    public sealed class MainHostedService : IHostedService
    {
        private LibPcapLiveDevice? _device;
        private readonly int _devIdx;
        private readonly MqttPublisher _mqttPublisher;
        private Task? _executingTask;
        private CancellationTokenSource? _serviceCts;

        public MainHostedService(int devIdx, MqttPublisher mqttPublisher)
        {
            _devIdx = devIdx;
            _mqttPublisher = mqttPublisher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _device = LibPcapLiveDeviceList.Instance[_devIdx];
            _device.Open();
            _device.OnPacketArrival += device_OnPacketArrival;
            _device.StartCapture();

            await _mqttPublisher.StartAsync();

            _serviceCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = WatchLoopAsync(_serviceCts.Token);
        }

        private void device_OnPacketArrival(object sender, PacketCapture e)
        {
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            var ipPacket = packet.Extract<IPPacket>();

            var packetInfo = new PacketInfo();
            packetInfo.GetPacketInfo(packet, ipPacket);

            var payload = JsonSerializer.Serialize(new
            {
                packetInfo.srcIP,
                packetInfo.dstIP,
                packetInfo.srcPort,
                packetInfo.dstPort,
                packetInfo.Protocol,
                CapturedAt = DateTimeOffset.Now
            });

            _ = _mqttPublisher.PublishMqttMessageAsync("ModbusTCP/0", payload);
        }

        private async Task WatchLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[PCapture] watch loop stopped.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_serviceCts is not null)
            {
                _serviceCts.Cancel();
            }

            if (_executingTask is not null)
            {
                await _executingTask;
            }

            if (_device is not null)
            {
                try
                {
                    _device.StopCapture();
                    _device.OnPacketArrival -= device_OnPacketArrival;
                    _device.Close();
                    _mqttPublisher.Stop();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PCapture] cleanup error: {ex.Message}");
                }

                Console.WriteLine("[PCapture] stopped.");
            }
        }
    }
}
