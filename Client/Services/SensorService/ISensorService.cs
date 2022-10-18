using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Shared;

namespace ComplexPrototypeSystem.Client.Services.SensorService
{
    public interface ISensorService
    {
        List<SensorSettings> SensorSettings { get; set; }

        Task GetAll();

        Task<SensorSettings> Get(Guid guid);

        Task Create(SensorSettings sensorSetting);

        Task Update(SensorSettings sensorSetting);

        Task Delete(Guid guid);

    }
}
