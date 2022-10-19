using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Service.Controllers;
using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Service.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SuperSimpleTcp;

namespace ComplexPrototypeSystem.Service.Client
{
    public sealed class TCPClient : BackgroundService
    {
        private readonly ILogger<TCPClient> logger;
        private readonly ConfigDAO configDAO;
        private readonly MessageQueue queue;
        private readonly IController controller;

        private readonly string serverAddress;
        private readonly int serverPort;

        private readonly SimpleTcpClient client;
        private readonly Random random = new Random();

        public TCPClient(ILogger<TCPClient> logger,
            ConfigDAO configDAO,
            MessageQueue queue,
            IController controller,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.queue = queue;
            this.controller = controller;
            this.configDAO = configDAO;

            serverAddress = configuration.GetConnectionString("ServerAddress");
            serverPort = Convert.ToInt32(configuration.GetConnectionString("ServerPort"));

            client = new SimpleTcpClient($"{serverAddress}:{serverPort}");
            client.Events.Connected += OnConnected;
            client.Events.Disconnected += OnDisconnected;
            client.Events.DataReceived += OnDataReceived;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                while (!client.IsConnected)
                {
                    await Connect(stoppingToken);
                }

                if (client.IsConnected && queue.Send.TryTake(out string message))
                {
                    await Send(message, stoppingToken);
                }

                await Task.Delay(100, stoppingToken);
            }

            if (client.IsConnected)
                await client.DisconnectAsync();

            logger.LogInformation("Stopped");
        }

        private void OnConnected(object sender, ConnectionEventArgs e)
        {
            logger.LogInformation($"Connected {e.IpPort}");
            queue.Send.Add($"Id:{configDAO.Config.Id},Interval:{configDAO.Config.Interval}");
        }

        private void OnDisconnected(object sender, ConnectionEventArgs e)
        {
            logger.LogInformation("Disconnected");
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count);
            logger.LogInformation($"[{e.IpPort}] {message}");

            queue.Recv.Add(message);
            controller.ReceiveMessage();
        }

        private async Task Connect(CancellationToken stoppingToken)
        {
            try
            {
                if (!client.IsConnected)
                    client.ConnectWithRetries(10000);
            }
            catch (Exception e)
            {
                int nextReconnectAttempt = random.Next(30_000, 60_000);
                logger.LogError(e, e.Message + $"\nNext reconnect attempt after {nextReconnectAttempt}s");

                await Task.Delay(nextReconnectAttempt, stoppingToken);
                await Connect(stoppingToken);
            }
        }

        private async Task Send(string message, CancellationToken stoppingToken)
        {
            try
            {
                if (client.IsConnected)
                {
                    await client.SendAsync(message, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}
