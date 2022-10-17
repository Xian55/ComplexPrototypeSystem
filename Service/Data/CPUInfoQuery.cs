using System;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace ComplexPrototypeSystem.Service.Data
{
    public sealed class CPUInfoQuery : IDisposable
    {
        private readonly ILogger logger;

        private readonly PerformanceCounter perfTempZone;
        private readonly PerformanceCounter perfCpuCount;

        public CPUInfoQuery(ILogger logger)
        {
            this.logger = logger;

            perfTempZone = new PerformanceCounter("Thermal Zone Information", "Temperature", @"\_TZ.TZ00");
            perfCpuCount = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
        }

        public void Dispose()
        {
            perfTempZone.Dispose();
            perfCpuCount.Dispose();
        }

        public bool Poll(out int tempF, out int usage)
        {
            try
            {
                tempF = (int)perfTempZone.NextValue();
                usage = (int)perfCpuCount.NextValue();
                return true;
            }
            catch (Exception e)
            {
                tempF = 0;
                usage = 0;
                logger.LogError("Unable to collect CPU temperature or Usage\n{e}", e.Message);
            }

            return false;
        }
    }
}
