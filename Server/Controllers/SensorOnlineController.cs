using System;

using Microsoft.AspNetCore.Mvc;
using ComplexPrototypeSystem.Server.Messaging;

namespace ComplexPrototypeSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class SensorOnlineController : ControllerBase
    {
        private readonly SensorOnlineStatus sensorOnlineStatus;

        public SensorOnlineController(SensorOnlineStatus sensorOnlineStatus)
        {
            this.sensorOnlineStatus = sensorOnlineStatus;
        }

        [HttpGet]
        public IActionResult GetOnlineSensors()
        {
            return Ok(sensorOnlineStatus.GuidToIpAddress.Keys);
        }

        [HttpGet("{guid}")]
        public IActionResult GetSensorOnline(Guid guid)
        {
            if (sensorOnlineStatus.GuidToIpAddress.TryGetValue(guid, out _))
                return Ok(true);

            return Ok(false);
        }
    }
}
