using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Service.Data;

using Microsoft.Extensions.Logging;

namespace ComplexPrototypeSystem.Service.Controllers
{
    public class Controller : IController
    {
        private readonly ILogger<Controller> logger;
        private readonly MessageQueue queue;

        private readonly ConfigDAO configHandler;

        public Controller(ILogger<Controller> logger,
            MessageQueue queue,
            ConfigDAO configHandler)
        {
            this.logger = logger;
            this.queue = queue;
            this.configHandler = configHandler;
        }

        public void ReceiveMessage()
        {
            if (queue.Recv.TryTake(out var message))
            {
                if (message.Contains("Interval:"))
                {
                    if (int.TryParse(message.Split(':')[1], out int interval))
                    {
                        logger.LogInformation($"Interval updated to {interval}ms from {configHandler.Config.Interval}ms");

                        configHandler.Config.Interval = interval;
                        configHandler.Save();
                    }
                }
            }
        }
    }
}
