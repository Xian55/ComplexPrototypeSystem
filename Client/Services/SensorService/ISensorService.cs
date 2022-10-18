using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Shared;

namespace ComplexPrototypeSystem.Client.Services.SensorService
{
    public interface ISensorService
    {
        List<SensorSettings> SensorSettings { get; set; }

        Task GetSensorSettings();

        Task<SensorSettings> GetSingleSensorSetting(Guid guid);
    }
}
