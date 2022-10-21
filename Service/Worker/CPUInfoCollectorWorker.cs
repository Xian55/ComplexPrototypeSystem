using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Service.Data;
using ComplexPrototypeSystem.Shared;

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

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (cpuInfo.Poll(out int tempF, out int usage))
                {
                    var now = DateTime.UtcNow;

                    logger.LogInformation($"{now} - Temp:{tempF} Usage:{usage}");

                    bw.Write((byte)Opcode.Report);
                    bw.Write(sizeof(long) + sizeof(int) + sizeof(int));

                    bw.Write(now.ToBinary());
                    bw.Write(tempF);
                    bw.Write(usage);

                    queue.Send.Add(ms.ToArray());
                    ms.Position = 0;
                    ms.SetLength(0);
                }

                await Task.Delay(configDAO.Config.Interval, stoppingToken);
            }

            cpuInfo.Dispose();

            logger.LogInformation("Stopped");
        }
    }
}
