using System;
using System.Threading;
using System.Threading.Tasks;
using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Service.Data;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComplexPrototypeSystem.Service.Worker
{
    public sealed class CPUInfoCollectorWorker : BackgroundService
    {
        private readonly ILogger<CPUInfoCollectorWorker> logger;
        private readonly MessageQueue queue;
        private readonly ConfigDAO configDAO;
        private readonly CPUInfoQuery cpuInfo;

        public CPUInfoCollectorWorker(ILogger<CPUInfoCollectorWorker> logger,
            ConfigDAO configDAO,
            MessageQueue container)
        {
            this.logger = logger;
            this.configDAO = configDAO;
            this.queue = container;

            cpuInfo = new CPUInfoQuery(this.logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (cpuInfo.Poll(out int tempF, out int usage))
                {
                    var now = DateTime.UtcNow;

                    logger.LogInformation($"{now} - Temp:{tempF} Usage:{usage}");
                    queue.Send.Add($"Id:{configDAO.Config.Id},Time:{now},TempF:{tempF},Usage:{usage}");
                }

                await Task.Delay(configDAO.Config.Interval, stoppingToken);
            }

            cpuInfo.Dispose();

            logger.LogInformation("Stopped");
        }
    }
}
