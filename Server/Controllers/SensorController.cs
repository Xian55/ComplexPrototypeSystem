using System;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Server.Data;
using ComplexPrototypeSystem.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplexPrototypeSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class SensorController : ControllerBase
    {
        private readonly SensorSettingsDbContext context;

        public SensorController(SensorSettingsDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSensorSettings()
        {
            return Ok(await context.SensorSettings.ToListAsync());
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetSensorSetting(Guid guid)
        {
            var sensor = await context.SensorSettings
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (sensor == null)
                return NotFound();

            return Ok(sensor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSensorSetting(
            [FromBody] SensorSettings sensor)
        {
            context.SensorSettings.Add(sensor);

            int changes = await context.SaveChangesAsync();
            if (changes > 0)
                return Ok();

            return Conflict();
        }

        [HttpPut("{guid}")]
        public async Task<IActionResult> UpdateSensorSetting(
            [FromBody] SensorSettings sensor, [FromRoute] Guid guid)
        {
            var dbSensor = await context.SensorSettings
                .FirstOrDefaultAsync(x => x.Guid == guid);

            if (dbSensor == null)
                return NotFound();

            dbSensor.Name = sensor.Name;
            dbSensor.Interval = sensor.Interval;
            dbSensor.IPAddress = sensor.IPAddress;

            int changes = await context.SaveChangesAsync();
            if (changes > 0)
                return Ok();

            return BadRequest();
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> DeleteSensorSetting(Guid guid)
        {
            var dbSensor = await context.SensorSettings
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (dbSensor == null)
                return NotFound();

            context.SensorSettings.Remove(dbSensor);

            int changes = await context.SaveChangesAsync();
            if (changes > 0)
                return Ok();

            return NotFound();
        }
    }
}
