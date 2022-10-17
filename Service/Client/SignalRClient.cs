using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using ComplexPrototypeSystem.Service.Data;

namespace ComplexPrototypeSystem.Service.Client
{
    public sealed class SignalRClient : BackgroundService
    {
        private readonly ILogger<SignalRClient> logger;
        private readonly MessageQueue queue;

        private readonly string serverAddress;
        private readonly int serverPort;
        private readonly string serverHub;

        private readonly string guid;

        private HubConnection connection;

        public SignalRClient(ILogger<SignalRClient> logger,
            MessageQueue queue,
            IConfiguration configuration,
            string guid)
        {
            this.logger = logger;
            this.queue = queue;
            this.guid = guid;

            serverAddress = configuration.GetConnectionString("ServerAddress");
            serverPort = Convert.ToInt32(configuration.GetConnectionString("ServerPort"));
            serverHub = configuration.GetConnectionString("ServerHub");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Connect(serverAddress, serverPort, stoppingToken);

                if (queue.Send.TryTake(out string message))
                {
                    await Send(guid, message, stoppingToken);
                }

                await Task.Delay(100, stoppingToken);
            }

            logger.LogInformation("Stopped");
        }


        private async Task Connect(string serverAddress, int port, CancellationToken stoppingToken)
        {
            if (connection == null)
            {
                connection = new HubConnectionBuilder()
                    .WithUrl($"{serverAddress}:{port}/{serverHub}")
                    .Build();

                connection.Closed += async (error) =>
                {
                    await Task.Delay(new Random().Next(1, 5) * 1000, stoppingToken);
                    await connection.StartAsync(stoppingToken);
                };

                connection.On<string, string>("ReceiveMessage", ReceiveMessage);
                void ReceiveMessage(string user, string message)
                {
                    string fullMessage = $"{user}: {message}";
                    logger.LogInformation(fullMessage);

                    queue.Recv.Add(message);
                }
            }

            try
            {
                if (connection.State == HubConnectionState.Disconnected)
                    await connection.StartAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message}; base Exception: {ex.GetBaseException().Message}");

                await Task.Delay(new Random().Next(1, 5) * 1000, stoppingToken);
                await Connect(serverAddress, port, stoppingToken);
            }
        }

        private async Task Send(string user, string message, CancellationToken stoppingToken)
        {
            try
            {
                await connection.InvokeAsync("ProcessClientMessage", user, message, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message}; base Exception: {ex.GetBaseException().Message}");
            }
        }
    }
}
