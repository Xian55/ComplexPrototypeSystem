using System.Threading.Tasks;
using System.Threading;
using System;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ComplexPrototypeSystem.Server.Hubs;
using Microsoft.Extensions.Hosting;

namespace ComplexPrototypeSystem.Server.Services
{
    public sealed class SensorSignalRServiceWorker : BackgroundService
    {
        private readonly ILogger<SensorSignalRServiceWorker> logger;
        private readonly IHubContext<SensorHub> signalRHub;

        public SensorSignalRServiceWorker(ILogger<SensorSignalRServiceWorker> logger,
            IHubContext<SensorHub> signalRHub)
        {
            this.logger = logger;
            this.signalRHub = signalRHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(30000, stoppingToken);

                int interval = random.Next(5000, 15000);
                logger.LogInformation($"{DateTime.Now.ToString("hh:mm:ss.fff")} Sending Interval:{interval}");
                
                await signalRHub.Clients.All.SendAsync("ReceiveMessage", "Server", $"Interval:{interval}");
            }
        }
    }
}
