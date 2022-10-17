using System;

using ComplexPrototypeSystem.Service.Client;
using ComplexPrototypeSystem.Service.Controllers;
using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Service.Data;
using ComplexPrototypeSystem.Service.Worker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<MessageQueue>();
                    services.AddSingleton<ConfigDAO>();

                    services.AddHostedService<CPUInfoCollectorWorker>();
                    services.AddHostedService<SignalRClient>();

                    services.AddSingleton<IController, Controller>();
                });
    }
}
