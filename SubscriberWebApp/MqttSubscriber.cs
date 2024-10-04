using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Packets;

public class MqttSubscriber
{
    private IMqttClient _mqttClient;
    private MqttFactory _mqttFactory;
    private MqttClientOptions _mqttClientOptions;
    private MqttTopicFilter _mqttTopicFilter;

    public MqttSubscriber()
    {
        _mqttFactory = new MqttFactory();
        _mqttClient = _mqttFactory.CreateMqttClient();
    }

    public async Task Start()
    {
        _mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId("Subscriber")
            .WithTcpServer("localhost")
            .Build();

        _mqttClient.ConnectedAsync += MqttClientConnected;
        _mqttClient.DisconnectedAsync += MqttClientDisconnected;
        _mqttClient.ApplicationMessageReceivedAsync += MqttClientApplicationMsgReceived;

        await _mqttClient.ConnectAsync(_mqttClientOptions);
    }

    public async Task Stop()
    {
        // 종료 시그널을 보냈다면 stop() 호출
        _mqttClient.ConnectedAsync -= MqttClientConnected;
        _mqttClient.DisconnectedAsync -= MqttClientDisconnected;
        _mqttClient.ApplicationMessageReceivedAsync -= MqttClientApplicationMsgReceived;

        await _mqttClient.DisconnectAsync();
    }

    public async Task MqttClientConnected(MqttClientConnectedEventArgs e)
    {
        Console.WriteLine("[MQTT sub] connected to broker");
        _mqttTopicFilter = new MqttTopicFilterBuilder()
            .WithTopic("ModbusTCP/4") // topic
            .Build();
        Console.WriteLine("[MQTT sub] Subscribed to Topic");
        await _mqttClient.SubscribeAsync(_mqttTopicFilter);
    }

    public async Task MqttClientDisconnected(MqttClientDisconnectedEventArgs e)
    {
        Console.WriteLine("[MQTT sub] disconnected to broker");
        Stop();
    }

    public async Task MqttClientApplicationMsgReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        var message = e.ApplicationMessage.PayloadSegment;
        Console.WriteLine($"[MQTT sub] received: " + $"{Encoding.UTF8.GetString(message)}");
        await Task.Run(() =>
        {
            // save data
        });
    }
}
