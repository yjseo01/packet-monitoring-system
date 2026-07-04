using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace SubscriberWebApp
{
    public sealed class MainHostedService : IHostedService
    {
        private readonly MqttSubscriber _mqttSubscriber;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _backgroundTask;

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
            _cancellationTokenSource?.Cancel();
            return _backgroundTask is null ? Task.CompletedTask : _backgroundTask.ContinueWith(_ => _mqttSubscriber.Stop(), cancellationToken);
        }
    }
}
