using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;

using SharpPcap;
using SharpPcap.LibPcap;

using Microsoft.Extensions.Hosting;
using PacketDotNet;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Configuration;

namespace PacketCapture
{
    public sealed class MainHostedService : IHostedService
    {
        ICaptureDevice _device;
        CaptureFileReaderDevice _deviceFromFile;
        MqttPublisher? _mqttPublisher;
        string? _fileName;

        private readonly IConfiguration _config;
        private readonly string[] _args;

        private static int packetIndex = 0;

        public MainHostedService(MqttPublisher mqttPublisher, IConfiguration configuration, string[] args)
        {
            // _fileName = "C:\\Users\\seoyujin\\Desktop\\C#\\project1\\PacketCapture\\ModbusTCP.pcap"; /////
            _mqttPublisher = mqttPublisher;
            _config = configuration;
            _args = args;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 명령줄 인수 가져오기
            string mode = _config["mode"]; // 명령줄 인수에 --mode <value>로 전달
            _fileName = _args[1] == "2" ? _args[2] : "default"; // mode 2일 경우

            _mqttPublisher.Start();
            try
            {
                if (_args[1] == "1")
                {
                    int readTimeoutMilliseconds = 1000;
                    SelectDevice();
                    _device.Open(DeviceModes.Promiscuous, readTimeoutMilliseconds);
                    _device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
                    Console.WriteLine("-- Capturing from '{0}', hit 'Ctrl-C' to exit...", _fileName);
                    _device.Capture();

                }
                else if (_args[1] == "2")
                {
                    _deviceFromFile = new CaptureFileReaderDevice(_fileName);
                    _deviceFromFile.Open();
                    _deviceFromFile.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
                    Console.WriteLine("-- Capturing from '{0}', hit 'Ctrl-C' to exit...", _fileName);
                    _deviceFromFile.Capture();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception when opening file" + e.ToString());
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_args[1] == "1")
            {
                _device.Close();
            }
            else
            {
                _deviceFromFile.Close();
            }

            return Task.CompletedTask;
        }

        public void SelectDevice()
        {
            var devices = LibPcapLiveDeviceList.Instance;

            Console.WriteLine();
            Console.WriteLine("The following devices are available on this machine.");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine();

            int i = 0;

            // print out the devices
            foreach (var dev in devices)
            {
                Console.WriteLine("{0}) {1} {2}", i++, dev.Name, dev.Description);
            }

            Console.WriteLine();
            Console.Write("--- Please choose a device to capture on: ");
            i = int.Parse(Console.ReadLine());
            _device = devices[i];

            // Console.Write("-- Please enter the output file name: ");
            // string capFile = Console.ReadLine();
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