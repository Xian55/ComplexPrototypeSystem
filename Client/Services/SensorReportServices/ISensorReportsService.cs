using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Shared;

namespace ComplexPrototypeSystem.Client.Services.SensorReportServices
{
    public interface ISensorReportsService
    {
        List<SensorReport> SensorReports { get; set; }

        Task Get(Guid guid);

        Task Delete(Guid guid);
    }
}
