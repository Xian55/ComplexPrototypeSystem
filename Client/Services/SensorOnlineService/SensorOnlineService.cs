using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Shared;

using Microsoft.AspNetCore.Components;

namespace ComplexPrototypeSystem.Client.Services.SensorOnlineService
{
    public sealed class SensorOnlineService : ISensorOnlineService
    {
        private readonly HttpClient http;

        public SensorOnlineService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<bool> SensorOnline(Guid guid)
        {
            return await http.GetFromJsonAsync<bool>($"api/sensoronline/{guid}");
        }

        public async Task<List<Guid>> OnlineSensors()
        {
            return await http.GetFromJsonAsync<List<Guid>>("api/sensoronline");
        }
    }
}
