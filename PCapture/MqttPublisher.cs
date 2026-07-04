using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace PCapture
{
    public class MqttPublisher
    {
        private MqttFactory? _mqttFactory;
        private IMqttClient? _mqttClient;
        private MqttClientOptions? _mqttClientOptions;
        private bool _stopRequested;

        public async Task StartAsync()
        {
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();
            var mqttClient = _mqttClient;
            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId("PCapture")
                .WithTcpServer("localhost")
                .Build();

            _stopRequested = false;

            while (!_stopRequested && (_mqttClient is null || !_mqttClient.IsConnected))
            {
                try
                {
                    await mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MQTT] connect failed: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        }

        public void Stop()
        {
            _stopRequested = true;

            if (_mqttClient is not null && _mqttClient.IsConnected)
            {
                _mqttClient.DisconnectAsync().GetAwaiter().GetResult();
            }

            _mqttFactory = null;
            _mqttClient = null;
            _mqttClientOptions = null;
        }

        public async Task PublishMqttMessageAsync(string topic, string payload)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            if (_mqttClient is null || !_mqttClient.IsConnected)
            {
                Console.WriteLine("[MQTT] client is not connected");
                return;
            }

            await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            Console.WriteLine($"[MQTT] published {topic}: {payload}");
        }
    }

}
