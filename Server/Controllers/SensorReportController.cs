using System;
using System.Linq;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Server.Data;
using ComplexPrototypeSystem.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplexPrototypeSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class SensorReportController : ControllerBase
    {
        private readonly SensorReportDbContext context;

        public SensorReportController(SensorReportDbContext context)
        {
            this.context = context;
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetReportsBySensor(Guid guid)
        {
            var sensor = await context.SensorReports.Where(x => x.SensorGuid == guid).ToListAsync();
            if (sensor == null)
                return NotFound();

            return Ok(sensor);
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> DeleteReport(Guid guid)
        {
            var dbSensor = await context.SensorReports
                .FirstOrDefaultAsync(x => x.ReportId == guid);
            if (dbSensor == null)
                return NotFound();

            context.SensorReports.Remove(dbSensor);

            int changes = await context.SaveChangesAsync();
            if (changes > 0)
                return Ok();

            return NotFound();
        }
    }
}
