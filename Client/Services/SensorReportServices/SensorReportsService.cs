using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Shared;
using ComplexPrototypeSystem.Shared.Converters;

using Microsoft.AspNetCore.Components;

namespace ComplexPrototypeSystem.Client.Services.SensorReportServices
{
    public sealed class SensorReportsService : ISensorReportsService
    {
        private readonly HttpClient http;
        private readonly JsonSerializerOptions serializerOptions;

        public List<SensorReport> SensorReports { get; set; } = new List<SensorReport>();

        public SensorReportsService(HttpClient http)
        {
            this.http = http;

            serializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
            serializerOptions.Converters.Add(new IPAddressConverter());
        }

        public async Task Get(Guid guid)
        {
            var result = await http.GetFromJsonAsync<List<SensorReport>>
                ($"api/sensorreport/{guid}", serializerOptions);

            if (result == null)
                throw new Exception("Sensor not found!");

            SensorReports = result;
        }

        public async Task Delete(Guid guid)
        {
            await http.DeleteAsync($"api/sensorreport/{guid}");
        }
    }
}
