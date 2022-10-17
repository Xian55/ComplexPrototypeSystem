using System;

namespace ComplexPrototypeSystem.Shared
{
    public class CPUMeasurement
    {
        public Guid SensorGuid { get; set; }

        public DateTime DateTime { get; set; }

        public int Usage { get; set; }

        public int TemperatureF { get; set; }

        public int TemperatureC => TemperatureF - 273;
    }
}
