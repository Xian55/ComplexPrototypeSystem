using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Shared;
using ComplexPrototypeSystem.Shared.Converters;

namespace ComplexPrototypeSystem.Client.Services.SensorService
{
    public class SensorService : ISensorService
    {
        private readonly HttpClient http;
        private readonly JsonSerializerOptions serializerOptions;

        public List<SensorSettings> SensorSettings { get; set; } = new List<SensorSettings>();

        public SensorService(HttpClient http)
        {
            this.http = http;

            serializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
            serializerOptions.Converters.Add(new IPAddressConverter());
        }

        public async Task GetSensorSettings()
        {
            var result = await http.GetFromJsonAsync<List<SensorSettings>>("api/sensor", serializerOptions);
            if (result != null)
            {
                SensorSettings = result;
            }
        }

        public Task<SensorSettings> GetSingleSensorSetting(Guid guid)
        {
            throw new NotImplementedException();
        }
    }
}
