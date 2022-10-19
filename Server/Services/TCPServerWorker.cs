using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SuperSimpleTcp;
using System.Text;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace ComplexPrototypeSystem.Server.Services
{
    public sealed class TCPServerWorker : BackgroundService
    {
        private readonly ILogger<TCPServerWorker> logger;
        private readonly SimpleTcpServer server;
        private readonly Random random = new Random();

        private readonly string address;
        private readonly int port;

        public TCPServerWorker(
            ILogger<TCPServerWorker> logger,
            IConfiguration configuration)
        {
            this.logger = logger;

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
                await Task.Delay(30000, stoppingToken);
                await BroadcastRandomInterval(stoppingToken);
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
            logger.LogInformation($"[{e.IpPort}]: {Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count)}");
        }
    }
}
