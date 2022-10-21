using System;
using System.Collections.Generic;

using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Shared;

using Microsoft.Extensions.Logging;

namespace ComplexPrototypeSystem.Service.Controllers
{
    public sealed class Controller : IController
    {
        private readonly ILogger<Controller> logger;
        private readonly ConfigDAO configDAO;

        private readonly Dictionary<Opcode, Action<int, ArraySegment<byte>>> handlers;

        public Controller(ILogger<Controller> logger, ConfigDAO configDAO)
        {
            this.logger = logger;
            this.configDAO = configDAO;

            handlers =
            new Dictionary<Opcode, Action<int, ArraySegment<byte>>>()
            {
                { Opcode.Identify, Identify },
                { Opcode.SetInterval, SetInterval }
            };

        }

        public void HandleOpcode(Opcode opcode, int size, ArraySegment<byte> payload)
        {
            if (handlers.TryGetValue(opcode, out var handler))
                handler(size, payload);
            else
                logger.LogWarning($"No handler found for {opcode.ToStringF()}");
        }

        public void Identify(int size, ArraySegment<byte> payload)
        {
            try
            {
                Guid id = new Guid(payload);
                configDAO.SetId(id);
                logger.LogInformation($"Registered as {id}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public void SetInterval(int size, ArraySegment<byte> payload)
        {
            try
            {
                int newInterval = BitConverter.ToInt32(payload);
                int oldInterval = configDAO.Config.Interval;

                if (oldInterval != newInterval)
                {
                    logger.LogInformation($"Interval updated to {newInterval}ms from {oldInterval}ms");
                    configDAO.SetInterval(newInterval);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}
