using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Packets;
using SubscriberWebApp.Components.Models;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore;

public class MqttSubscriber
{
    private IMqttClient _mqttClient;
    private MqttFactory _mqttFactory;
    private MqttClientOptions _mqttClientOptions;
    private MqttTopicFilter _mqttTopicFilter;
    private ModbusDbContext _modbusDbContext;

    private readonly IServiceProvider _serviceProvider;

    private int _pktCnt;

    public MqttSubscriber(IServiceProvider serviceProvider)
    {
        _mqttFactory = new MqttFactory();
        _mqttClient = _mqttFactory.CreateMqttClient();

        _serviceProvider = serviceProvider;

        _pktCnt = 0;

        using (var scope = _serviceProvider.CreateScope())
        {
            _modbusDbContext = scope.ServiceProvider.GetRequiredService<ModbusDbContext>();

            _pktCnt = _modbusDbContext.ModbusData.OrderByDescending(m => m.Id).Select(m => m.Id).FirstOrDefault();
        }
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        Console.WriteLine("[Mqtt Subscriber] start ");
        while (!cancellationToken.IsCancellationRequested)
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
    }

    public async Task Stop()
    {
        // 종료 시그널을 보냈다면 stop() 호출하기 (구현 전)
        _mqttClient.ConnectedAsync -= MqttClientConnected;
        _mqttClient.DisconnectedAsync -= MqttClientDisconnected;
        _mqttClient.ApplicationMessageReceivedAsync -= MqttClientApplicationMsgReceived;

        await _mqttClient.DisconnectAsync();
    }

    public async Task MqttClientConnected(MqttClientConnectedEventArgs e)
    {
        Console.WriteLine("[MQTT sub] connected to broker");
        _mqttTopicFilter = new MqttTopicFilterBuilder()
            .WithTopic("ModbusTCP/+") // topic
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

        string topic = e.ApplicationMessage.Topic;
        byte[] payload = e.ApplicationMessage.PayloadSegment.ToArray();
        ModbusData data = new ModbusData();

        switch (topic)
        {
            case "ModbusTCP/1": // Read Coils
                data.FunctionCode = 1;
                break;
            case "ModbusTCP/2": // Read Discrete Inputs
                data.FunctionCode = 2;
                break;
            case "ModbusTCP/3": // Read Multiple Registers
                data.FunctionCode = 3;
                break;
            case "ModbusTCP/4": // Read Input Registers
                data.FunctionCode = 4;
                break;
            case "ModbusTCP/5": // Write Single Coil
                data.FunctionCode = 5;
                break;
            case "ModbusTCP/6": // Write Signle Registers
                data.FunctionCode = 6;
                break;
            case "ModbusTCP/10": // Write Multiple Registers
                data.FunctionCode = 10;
                break;
            case "ModbusTCP/15": // Write Multiple Coils
                data.FunctionCode = 15;
                break;
            default:
                // error
                Console.WriteLine($"Unknown topic: {topic}");
                break;
        }

        data.Id = ++_pktCnt;
        data.PayLoadData = payload;
        data.TimeStamp = DateTime.Now;

        using (var scope = _serviceProvider.CreateScope())
        {
            _modbusDbContext = scope.ServiceProvider.GetRequiredService<ModbusDbContext>();

            // save data
            try
            {
                _modbusDbContext.ModbusData.Add(data);
                await _modbusDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to the database: {ex.Message}");
            }
        }

    }
}
