using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace SubscriberWebApp
{
    public sealed class MainHostedService : IHostedService
    {
        private MqttSubscriber _mqttSubscriber;
        private Task _backgroundTask;
        private CancellationTokenSource _cancellationTokenSource;

        public MainHostedService(MqttSubscriber mqttSubscriber)
        {
            _mqttSubscriber = mqttSubscriber;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _backgroundTask = Task.Run(() => _mqttSubscriber.Start(_cancellationTokenSource.Token), cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _mqttSubscriber.Stop();
            return Task.CompletedTask;
        }
    }
}