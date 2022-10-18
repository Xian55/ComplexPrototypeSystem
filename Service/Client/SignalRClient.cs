using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using ComplexPrototypeSystem.Service.Data;
using ComplexPrototypeSystem.Service.Controllers;
using ComplexPrototypeSystem.Service.DAO;
using System.Net.Http;

namespace ComplexPrototypeSystem.Service.Client
{
    public sealed class SignalRClient : BackgroundService
    {
        private readonly ILogger<SignalRClient> logger;
        private readonly ConfigDAO configDAO;
        private readonly MessageQueue queue;
        private readonly IController controller;

        private readonly string serverAddress;
        private readonly int serverPort;
        private readonly string serverHub;

        private HubConnection connection;

        public SignalRClient(ILogger<SignalRClient> logger,
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
            serverHub = configuration.GetConnectionString("ServerHub");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Connect(serverAddress, serverPort, stoppingToken);

                if (connection.State == HubConnectionState.Connected &&
                    queue.Send.TryTake(out string message))
                {
                    await Send(configDAO.Config.Id.ToString(), message, stoppingToken);
                }

                await Task.Delay(100, stoppingToken);
            }

            logger.LogInformation("Stopped");
        }


        private async Task Connect(string serverAddress, int port, CancellationToken stoppingToken)
        {
            if (connection == null)
            {
                try
                {
                    Uri uri = new Uri($"{serverAddress}:{port}/{serverHub}");
                    connection = new HubConnectionBuilder()
                        .WithUrl(uri, options =>
                        {
                            options.UseDefaultCredentials = true;

                            // TODO: workaround for 'The remote certificate is invalid according to the validation procedure.'
                            options.HttpMessageHandlerFactory = (msg) =>
                            {
                                if (msg is HttpClientHandler clientHandler)
                                {
                                    // bypass SSL certificate
                                    clientHandler.ServerCertificateCustomValidationCallback +=
                                        (sender, certificate, chain, sslPolicyErrors) => { return true; };
                                }

                                return msg;
                            };
                        })
                        .WithAutomaticReconnect()
                        .Build();
                }
                catch (Exception ex)
                {
                    logger.LogError($"{ex.Message}; base Exception: {ex.GetBaseException().Message}");
                    return;
                }

                connection.Closed += async (error) =>
                {
                    await Task.Delay(new Random().Next(30, 60) * 1000, stoppingToken);
                    await connection.StartAsync(stoppingToken);
                };

                connection.On<string, string>("ReceiveMessage", ReceiveMessage);
                void ReceiveMessage(string user, string message)
                {
                    string fullMessage = $"{user}: {message}";
                    logger.LogInformation(fullMessage);

                    queue.Recv.Add(message);
                    controller.ReceiveMessage();
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

                await Task.Delay(new Random().Next(30, 60) * 1000, stoppingToken);
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
