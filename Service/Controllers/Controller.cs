using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Service.Data;

using Microsoft.Extensions.Logging;

namespace ComplexPrototypeSystem.Service.Controllers
{
    public sealed class Controller : IController
    {
        private readonly ILogger<Controller> logger;
        private readonly MessageQueue queue;

        private readonly ConfigDAO configDAO;

        public Controller(ILogger<Controller> logger,
            MessageQueue queue,
            ConfigDAO configDAO)
        {
            this.logger = logger;
            this.queue = queue;
            this.configDAO = configDAO;
        }

        public void ReceiveMessage()
        {
            if (queue.Recv.TryTake(out var message))
            {
                if (message.Contains("Interval:"))
                {
                    if (int.TryParse(message.Split(':')[1], out int interval))
                    {
                        logger.LogInformation($"Interval updated to {interval}ms from {configDAO.Config.Interval}ms");
                        configDAO.SetInterval(interval);
                    }
                }
            }
        }
    }
}
