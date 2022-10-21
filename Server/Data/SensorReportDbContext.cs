using ComplexPrototypeSystem.Shared;

using Microsoft.EntityFrameworkCore;

namespace ComplexPrototypeSystem.Server.Data
{
    public sealed class SensorReportDbContext : DbContext
    {
        public DbSet<SensorReport> SensorReports { get; set; }

        public SensorReportDbContext(DbContextOptions<SensorReportDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorReport>().HasData(
                new SensorReport()
                {
                    ReportId = System.Guid.NewGuid(),
                    SensorGuid = System.Guid.Empty,
                    DateTime = System.DateTime.MinValue,
                    Usage = 0,
                    TemperatureF = 0
                });
        }
    }
}
