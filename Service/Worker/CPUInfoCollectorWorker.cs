using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Service.Data;
using ComplexPrototypeSystem.Shared;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComplexPrototypeSystem.Service.Worker
{
    public class CPUInfoCollectorWorker : BackgroundService
    {
        private readonly ILogger<CPUInfoCollectorWorker> logger;
        private readonly MessageQueue queue;
        private readonly CPUInfoQuery cpuInfo;

        private int interval = 5000;
        public int Interval
        {
            get => interval;
            set
            {
                interval = value;
                logger.LogInformation($"Initialized with Interval: {interval}");
            }
        }

        public CPUInfoCollectorWorker(ILogger<CPUInfoCollectorWorker> logger,
            MessageQueue container)
        {
            this.logger = logger;
            this.queue = container;

            cpuInfo = new CPUInfoQuery(this.logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (cpuInfo.Poll(out int tempF, out int usage))
                {
                    var m = new CPUMeasurement
                    {
                        DateTime = DateTime.UtcNow,
                        TemperatureF = tempF,
                        Usage = usage,
                    };

                    logger.LogInformation($"{m.DateTime} - Temp:{tempF} Usage:{usage}");
                    queue.Send.Add($"Time:{m.DateTime},TempF:{tempF},Usage:{usage}");
                }

                await Task.Delay(Interval, stoppingToken);
            }

            logger.LogInformation($"Stopped");
        }
    }
}
