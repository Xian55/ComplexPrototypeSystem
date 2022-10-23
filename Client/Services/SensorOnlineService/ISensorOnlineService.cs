using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComplexPrototypeSystem.Client.Services.SensorOnlineService
{
    public interface ISensorOnlineService
    {
        Task<bool> SensorOnline(Guid guid);

        Task<List<Guid>> OnlineSensors();
    }
}
