using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace PCapture
{
    public class MqttPublisher
    {
        MqttFactory? _mqttFactory;
        IMqttClient? _mqttClient;
        MqttClientOptions? _mqttClientOptions;


        public async void Start()
        {
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();
            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost")
                .Build();

            await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
        }

        public void Stop()
        {
            _mqttFactory = null;
            _mqttClient = null;
            _mqttClientOptions = null;
        }

        public async void PublishMqttMessage()
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("network/packets")
                .Build();

            _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            Console.WriteLine("[MQTT] Mqtt Message published!");
        }
    }

}