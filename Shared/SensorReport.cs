using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ComplexPrototypeSystem.Shared
{
    public class SensorReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ReportId { get; set; } = Guid.NewGuid();

        public Guid SensorGuid { get; set; }

        public DateTime DateTime { get; set; }

        public int Usage { get; set; }

        public int TemperatureF { get; set; }

        public int TemperatureC => TemperatureF - 273;
    }
}
