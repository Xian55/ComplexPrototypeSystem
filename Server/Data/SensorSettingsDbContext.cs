using System.Net;
using System;

using ComplexPrototypeSystem.Shared;

using Microsoft.EntityFrameworkCore;

namespace ComplexPrototypeSystem.Server.Data
{
    public sealed class SensorSettingsDbContext : DbContext
    {
        public DbSet<SensorSettings> SensorSettings { get; set; }

        public SensorSettingsDbContext(DbContextOptions<SensorSettingsDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorSettings>();
        }
    }
}
