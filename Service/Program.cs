using System;

using ComplexPrototypeSystem.Service.Data;
using ComplexPrototypeSystem.Service.Worker;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ComplexPrototypeSystem.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .UseWindowsService()
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<MessageQueue>();

                    services.AddSingleton<string>(x => Guid.NewGuid().ToString());

                    services.AddSingleton<CPUInfoCollectorWorker>();
                    services.AddSingleton<IHostedService>(p => p.GetService<CPUInfoCollectorWorker>());
                });
    }
}
