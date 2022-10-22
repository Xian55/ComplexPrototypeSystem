using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ComplexPrototypeSystem.Service.Controllers;
using ComplexPrototypeSystem.Service.DAO;
using ComplexPrototypeSystem.Service.Data;
using ComplexPrototypeSystem.Shared;

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

        private bool authorized;

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

            controller.OnAuthorized += Controller_OnAuthorized;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                while (!client.IsConnected && !stoppingToken.IsCancellationRequested)
                {
                    if (queue.Send.Count > 0)
                    {
                        logger.LogWarning($"Unsent messages {queue.Send.Count}");
                    }

                    await Connect(stoppingToken);
                }

                if (client.IsConnected)
                {
                    while (queue.PrioritySend.TryTake(out byte[] prioPayload))
                        await client.SendAsync(prioPayload, stoppingToken);

                    while (queue.Send.TryTake(out byte[] normalPayload))
                        await client.SendAsync(normalPayload, stoppingToken);
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

            using MemoryStream ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write((byte)Opcode.Identify);

            byte[] data = configDAO.Config.Id.ToByteArray();
            bw.Write(data.Length);
            bw.Write(data);

            queue.PrioritySend.Add(ms.ToArray());
        }

        private void OnDisconnected(object sender, ConnectionEventArgs e)
        {
            authorized = false;
            logger.LogInformation("Disconnected");
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            Opcode opcode = (Opcode)e.Data.Array[0];

            if (OpCode_Extension.IsDefined(opcode))
            {
                int sizeOffset = (1 + sizeof(int));
                int size = BitConverter.ToInt32(e.Data.Array[1..sizeOffset]);
                int dataOffset = sizeOffset + size;

                controller.HandleOpcode(opcode, size, e.Data.Array[sizeOffset..dataOffset]);
            }
            else
            {
                logger.LogWarning($"Unknown {nameof(Opcode)} {opcode}");
            }
        }

        private async Task Connect(CancellationToken stoppingToken)
        {
            try
            {
                if (!client.IsConnected)
                    client.ConnectWithRetries(10000);
            }
            catch (Exception ex)
            {
                int nextReconnectAttempt = random.Next(30_000, 60_000);
                logger.LogError(ex, ex.Message + $"\nNext reconnect attempt after {nextReconnectAttempt}ms");

                await Task.Delay(nextReconnectAttempt, stoppingToken);
            }
        }

        private void Controller_OnAuthorized()
        {
            authorized = true;
        }
    }
}
