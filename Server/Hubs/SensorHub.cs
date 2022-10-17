using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace ComplexPrototypeSystem.Server.Hubs
{
    public sealed class SensorHub : Hub
    {
        private readonly ILogger<SensorHub> logger;

        public SensorHub(ILogger<SensorHub> logger)
        {
            this.logger = logger;
        }

        public Task ProcessClientMessage(string user, string message)
        {
            logger.LogInformation($"ProcessClientMessage({user}, {message})");

            return Task.CompletedTask;
        }
    }
}
