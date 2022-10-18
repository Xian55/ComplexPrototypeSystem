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

namespace ComplexPrototypeSystem.Client.Services.SensorService
{
    public sealed class SensorService : ISensorService
    {
        private readonly HttpClient http;
        private readonly NavigationManager navigationManager;
        private readonly JsonSerializerOptions serializerOptions;

        public List<SensorSettings> SensorSettings { get; set; } = new List<SensorSettings>();

        public SensorService(HttpClient http,
            NavigationManager navigationManager)
        {
            this.http = http;
            this.navigationManager = navigationManager;

            serializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
            serializerOptions.Converters.Add(new IPAddressConverter());
        }

        public async Task GetAll()
        {
            var result = await http
                .GetFromJsonAsync<List<SensorSettings>>("api/sensor",
                serializerOptions);

            if (result != null)
                SensorSettings = result;
        }

        public async Task<SensorSettings> Get(Guid guid)
        {
            var result = await http.GetFromJsonAsync<SensorSettings>
                ($"api/sensor/{guid}", serializerOptions);

            if (result == null)
                throw new Exception("Sensor not found!");

            return result;
        }

        public async Task Create(SensorSettings sensorSetting)
        {
            await http.PostAsJsonAsync("api/sensor",
                sensorSetting, serializerOptions);

            navigationManager.NavigateTo("sensors");
        }

        public async Task Update(SensorSettings sensorSetting)
        {
            await http.PutAsJsonAsync($"api/sensor/{sensorSetting.Guid}",
                sensorSetting, serializerOptions);

            navigationManager.NavigateTo("sensors");
        }

        public async Task Delete(Guid guid)
        {
            await http.DeleteAsync($"api/sensor/{guid}");

            navigationManager.NavigateTo("sensors");
        }
    }
}
