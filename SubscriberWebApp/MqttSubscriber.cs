using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;
using SubscriberWebApp.Components.Models;
using System.Text;

public class MqttSubscriber
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMqttClient _mqttClient;
    private readonly MqttFactory _mqttFactory;
    private MqttClientOptions? _mqttClientOptions;
    private bool _eventsAttached;
    private int _pktCnt;

    public MqttSubscriber(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _mqttFactory = new MqttFactory();
        _mqttClient = _mqttFactory.CreateMqttClient();

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ModbusDbContext>();
        _pktCnt = dbContext.ModbusData.OrderByDescending(m => m.Id).Select(m => m.Id).FirstOrDefault();
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        Console.WriteLine("[Mqtt Subscriber] start");
        AttachEvents();

        _mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId("Subscriber")
            .WithTcpServer("localhost")
            .Build();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_mqttClient.IsConnected)
            {
                try
                {
                    await _mqttClient.ConnectAsync(_mqttClientOptions, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Mqtt Subscriber] connect failed: {ex.Message}");
                    await Task.Delay(1000, cancellationToken);
                }
            }

            await Task.Delay(500, cancellationToken);
        }
    }

    public async Task Stop()
    {
        DetachEvents();

        if (_mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync();
        }
    }

    private void AttachEvents()
    {
        if (_eventsAttached)
        {
            return;
        }

        _mqttClient.ConnectedAsync += MqttClientConnected;
        _mqttClient.DisconnectedAsync += MqttClientDisconnected;
        _mqttClient.ApplicationMessageReceivedAsync += MqttClientApplicationMsgReceived;
        _eventsAttached = true;
    }

    private void DetachEvents()
    {
        if (!_eventsAttached)
        {
            return;
        }

        _mqttClient.ConnectedAsync -= MqttClientConnected;
        _mqttClient.DisconnectedAsync -= MqttClientDisconnected;
        _mqttClient.ApplicationMessageReceivedAsync -= MqttClientApplicationMsgReceived;
        _eventsAttached = false;
    }

    public async Task MqttClientConnected(MqttClientConnectedEventArgs e)
    {
        Console.WriteLine("[MQTT sub] connected to broker");
        await _mqttClient.SubscribeAsync("ModbusTCP/+");
        Console.WriteLine("[MQTT sub] subscribed to ModbusTCP/+");
    }

    public Task MqttClientDisconnected(MqttClientDisconnectedEventArgs e)
    {
        Console.WriteLine("[MQTT sub] disconnected from broker");
        return Task.CompletedTask;
    }

    public async Task MqttClientApplicationMsgReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        var payloadBytes = e.ApplicationMessage.PayloadSegment.ToArray();
        var payload = Encoding.UTF8.GetString(payloadBytes);
        Console.WriteLine($"[MQTT sub] received: {payload}");

        if (!TryParseFunctionCode(e.ApplicationMessage.Topic, out var functionCode))
        {
            Console.WriteLine($"[MQTT sub] unknown topic: {e.ApplicationMessage.Topic}");
            return;
        }

        var data = new ModbusData
        {
            Id = ++_pktCnt,
            FunctionCode = functionCode,
            PayLoadData = payloadBytes,
            TimeStamp = DateTime.Now
        };

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ModbusDbContext>();

        try
        {
            dbContext.ModbusData.Add(data);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Mqtt Subscriber] save failed: {ex.Message}");
        }
    }

    private static bool TryParseFunctionCode(string topic, out int functionCode)
    {
        functionCode = 0;
        var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        return int.TryParse(parts[1], out functionCode);
    }
}
