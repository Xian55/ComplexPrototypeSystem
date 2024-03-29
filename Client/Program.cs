using System;
using System.Net.Http;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Client.Services.SensorOnlineService;
using ComplexPrototypeSystem.Client.Services.SensorReportServices;
using ComplexPrototypeSystem.Client.Services.SensorService;

using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComplexPrototypeSystem.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient("ComplexPrototypeSystem.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ComplexPrototypeSystem.ServerAPI"));
            builder.Services.AddScoped<ISensorService, SensorService>();
            builder.Services.AddScoped<ISensorReportsService, SensorReportsService>();
            builder.Services.AddScoped<ISensorOnlineService, SensorOnlineService>();

            builder.Services.AddApiAuthorization();

            await builder.Build().RunAsync();
        }
    }
}
