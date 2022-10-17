using System;

namespace ComplexPrototypeSystem.Shared
{
    public class CPUSensorMeasurement
    {
        public Guid SensorGuid { get; set; }

        public DateTime DateTime { get; set; }

        public int Usage { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
