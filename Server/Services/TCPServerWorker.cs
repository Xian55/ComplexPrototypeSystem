using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SuperSimpleTcp;
using System.Text;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using ComplexPrototypeSystem.Server.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;

namespace ComplexPrototypeSystem.Server.Services
{
    public sealed class TCPServerWorker : BackgroundService
    {
        private readonly ILogger<TCPServerWorker> logger;
        private readonly SimpleTcpServer server;
        private readonly MessageQueue queue;
        private readonly Random random = new Random();

        private readonly SensorSettingsDbContext context;

        private readonly string address;
        private readonly int port;

        public TCPServerWorker(
            ILogger<TCPServerWorker> logger,
            MessageQueue queue,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.queue = queue;

            this.context = serviceProvider
                .CreateScope().ServiceProvider
                .GetRequiredService<SensorSettingsDbContext>();

            address = configuration["TcpServer:Address"];
            port = Convert.ToInt32(configuration["TcpServer:Port"]);

            server = new SimpleTcpServer(address, port);

            server.Events.ClientConnected += ClientConnected;
            server.Events.ClientDisconnected += ClientDisconnected;
            server.Events.DataReceived += DataReceived;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            server.Start();
            logger.LogInformation($"Started listening at {address}:{port}");

            while (!stoppingToken.IsCancellationRequested)
            {
                while (queue.Send.TryTake(out var data) &&
                    !stoppingToken.IsCancellationRequested)
                {
                    server.Send(data.Key, data.Value);
                }

                await Task.Delay(100, stoppingToken);
            }

            DisconnectClients();
            server.Stop();

            logger.LogInformation("Stopped");
        }

        private void DisconnectClients()
        {
            foreach (var client in server.GetClients())
            {
                server.DisconnectClient(client);
                logger.LogInformation($"DisconnectedClient({client})");
            }
        }

        private async Task BroadcastRandomInterval(CancellationToken stoppingToken)
        {
            int intervalSec = random.Next(5000, 50_000);

            foreach (var client in server.GetClients())
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                byte[] data = Encoding.UTF8.GetBytes($"Interval:{intervalSec}");
                await server.SendAsync(client, data, stoppingToken);
            }
        }

        private void ClientConnected(object sender, ConnectionEventArgs e)
        {
            logger.LogInformation($"Client Connected {e.IpPort}");
        }

        private void ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            logger.LogInformation($"Client Disconnected {e.IpPort}");
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count);
            logger.LogInformation($"[{e.IpPort}]: {message}");

            string[] array = message.Split(',');
            IPAddress newIp = IPAddress.Parse(e.IpPort.Split(':')[0]);

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].StartsWith("Id:"))
                {
                    Guid id = Guid.Parse(array[i].Split(":")[1]);

                    var dbSensor = context.SensorSettings.FirstOrDefault(x => x.Guid == id);
                    if (dbSensor != null)
                    {
                        logger.LogInformation($"Sensor Exists");

                        if (newIp != dbSensor.IPAddress)
                        {
                            dbSensor.IPAddress = newIp;
                            int change = context.SaveChanges();
                            if (change > 0)
                                logger.LogInformation($"{dbSensor.Guid} connected from a new IP {dbSensor.IPAddress}");
                        }
                    }
                    else
                    {
                        logger.LogInformation($"Sensor New detected!");

                        Shared.SensorSettings sensorSettings = new Shared.SensorSettings()
                        {
                            IPAddress = newIp,
                            Guid = id
                        };

                        context.SensorSettings.Add(sensorSettings);
                        int change = context.SaveChanges();
                        if (change > 0)
                            logger.LogInformation($"New Sensor Added {dbSensor.Guid} - {dbSensor.IPAddress}");
                    }
                    break;
                }
            }
        }
    }
}
