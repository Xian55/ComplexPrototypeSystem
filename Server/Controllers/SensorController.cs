using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly MessageQueue queue;

        public SensorController(SensorSettingsDbContext context, MessageQueue queue)
        {
            this.context = context;
            this.queue = queue;
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

            // TODO: move this to elsewhere
            if (dbSensor.Interval != sensor.Interval)
            {
                using MemoryStream ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);

                bw.Write((byte)Opcode.SetInterval);
                int size = System.Runtime.InteropServices.Marshal.SizeOf(sensor.Interval);
                bw.Write(size);
                bw.Write(sensor.Interval);

                queue.SendInterval.Add(new KeyValuePair<string, byte[]>(dbSensor.Guid.ToString(), ms.ToArray()));
            }

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
