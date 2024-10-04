using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;
using PacketDotNet;
using System.Threading;
using MQTTnet.Formatter;

namespace PacketCapture
{
    public class MqttPublisher
    {
        MqttFactory? _mqttFactory;
        IMqttClient? _mqttClient;
        MqttClientOptions? _mqttClientOptions;

        public async Task Start()
        {
            try
            {
                _mqttFactory = new MqttFactory();
                _mqttClient = _mqttFactory.CreateMqttClient();
                _mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("localhost")
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .Build();

                await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to MQTT broker: {ex.Message}");
            }
        }

        public async Task Stop()
        {
            await _mqttClient.DisconnectAsync();
            _mqttFactory = null;
            _mqttClient = null;
            _mqttClientOptions = null;
        }

        public async Task PublishMqttMessage(TcpPacket tcpPacket)
        {
            if (_mqttClient == null)
            {
                Console.WriteLine("MQTT Client is not initialized. Make sure to call StartAsync first.");
                return;
            }

            if (tcpPacket.SourcePort == 502 || tcpPacket.DestinationPort == 502)
            {
                if (tcpPacket.PayloadData.Length >= 7)
                {
                    byte[] payload = tcpPacket.PayloadData;
                    Console.WriteLine($"PayloadData length: {tcpPacket.PayloadData.Length}");
                    byte functionCode = payload[7];

                    string topic = $"ModbusTCP/{functionCode.ToString()}";
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(payload) // payload2
                        .WithContentType("application/octet-stream") // ContentType을 지정
                        .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)1)
                        .WithRetainFlag()
                        .Build();

                    //await _mqttClient.PublishAsync(message);
                    Console.WriteLine($"MQTT Client Connected: {_mqttClient.IsConnected}");
                    try
                    {
                        await _mqttClient.PublishAsync(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception during PublishAsync: {ex.Message}");
                    }
                    Console.WriteLine($"[Mqtt] mqtt message published : {topic}");
                }
            }

            Thread.Sleep(1000);
        }
    }
}