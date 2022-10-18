using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Shared;

using Microsoft.AspNetCore.Mvc;

namespace ComplexPrototypeSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        public static List<SensorSettings> sensors = new List<SensorSettings>()
        {
            new SensorSettings()
            {
                Guid = Guid.NewGuid(),
                Name = "First",
                Interval = 5000,
                IPAddress = IPAddress.Parse("127.0.0.1")
            },
            new SensorSettings()
            {
                Guid = Guid.NewGuid(),
                Name = "Second",
                Interval = 15000,
                IPAddress = IPAddress.Parse("192.168.1.150")
            },
            new SensorSettings()
            {
                Guid = Guid.NewGuid(),
                Name = "Third",
                Interval = 168416,
                IPAddress = IPAddress.Parse("192.168.1.111")
            },
        };

        [HttpGet]
        public async Task<IActionResult> GetSensorSettings()
        {
            return Ok(sensors);
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetSensorSetting(Guid guid)
        {
            var sensor = sensors.FirstOrDefault(x => x.Guid == guid);
            if (sensor == null)
                return NotFound("Sensor Not found!");

            return Ok(sensor);
        }
    }
}
