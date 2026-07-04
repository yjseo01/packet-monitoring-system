using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;

using SharpPcap;
using SharpPcap.LibPcap;

using Microsoft.Extensions.Hosting;
using PacketDotNet;
using Microsoft.VisualBasic;

namespace PacketCapture
{
    public sealed class MainHostedService : IHostedService
    {
        CaptureFileReaderDevice? _deviceFromFile;
        MqttPublisher _mqttPublisher;
        string _fileName;

        private static int packetIndex = 0;

        public MainHostedService(MqttPublisher mqttPublisher, CaptureFileReaderDevice? deviceFromFile = null)
        {
            _fileName = "C:\\Users\\seoyujin\\Desktop\\C#\\project1\\PacketCapture\\ModbusTCP.pcap";
            _mqttPublisher = mqttPublisher;
            _deviceFromFile = deviceFromFile;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _mqttPublisher.Start();
            try
            {
                // get an offline device
                _deviceFromFile = new CaptureFileReaderDevice(_fileName);
                // open the device
                _deviceFromFile.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception when opening file" + e.ToString());
                return Task.CompletedTask;
            }

            _deviceFromFile.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);

            Console.WriteLine
                ("-- Capturing from '{0}', hit 'Ctrl-C' to exit...",
                _fileName);


            _deviceFromFile.Capture();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _deviceFromFile.Close();

            return Task.CompletedTask;
        }


        private void Device_OnPacketArrival(object sender, SharpPcap.PacketCapture e)
        {
            packetIndex++;

            var rawPacket = e.GetPacket();
            var packet = PacketDotNet.Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            // ethernet packet 추출
            var ethernetPacket = packet.Extract<EthernetPacket>();
            if (ethernetPacket != null)
            {
                Console.WriteLine("Packet {0} captured at {1} {2}",
                packetIndex,
                e.Header.Timeval.Date.ToString(),
                e.Header.Timeval.Date.Millisecond);

                // ip packet 추출
                var ipPacket = ethernetPacket.Extract<IPPacket>();
                if (ipPacket != null)
                {
                    // tcp packet 추출
                    var tcpPacket = packet.Extract<TcpPacket>();
                    if (tcpPacket != null)
                    {
                        _mqttPublisher.PublishMqttMessage(tcpPacket);
                    }
                }

            }
        }

    }
}