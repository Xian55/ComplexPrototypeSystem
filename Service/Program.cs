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

using Serilog;

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
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log.log",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}"))
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
