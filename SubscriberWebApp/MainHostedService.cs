using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace SubscriberWebApp
{
    public sealed class MainHostedService : IHostedService
    {
        MqttSubscriber _mqttSubscriber;
        public MainHostedService(MqttSubscriber mqttSubscriber)
        {
            _mqttSubscriber = mqttSubscriber;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("[Mqtt Subscriber] start ");
            _mqttSubscriber.Start();
            while (true)
            {
                // doing nothing
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _mqttSubscriber.Stop();
            return Task.CompletedTask;
        }
    }
}