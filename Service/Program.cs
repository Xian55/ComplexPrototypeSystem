using System;

using ComplexPrototypeSystem.Service.Client;
using ComplexPrototypeSystem.Service.Controllers;
using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Service.Data;
using ComplexPrototypeSystem.Service.Worker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

namespace ComplexPrototypeSystem.Service
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "CPU Sensor Service";
                })
                .ConfigureServices((hostContext, services) =>
                {
                    LoggerProviderOptions
                        .RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);

                    services.AddSingleton<MessageQueue>();
                    services.AddSingleton<ConfigDAO>();

                    services.AddHostedService<CPUInfoCollectorWorker>();
                    services.AddHostedService<TCPClient>();

                    services.AddSingleton<IController, Controller>();
                });
    }
}
