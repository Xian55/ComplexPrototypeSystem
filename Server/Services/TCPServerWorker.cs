using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SuperSimpleTcp;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using ComplexPrototypeSystem.Server.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using ComplexPrototypeSystem.Shared;
using System.IO;
using ComplexPrototypeSystem.Server.Migrations;

namespace ComplexPrototypeSystem.Server.Services
{
    public sealed class TCPServerWorker : BackgroundService
    {
        private readonly ILogger<TCPServerWorker> logger;
        private readonly SimpleTcpServer server;
        private readonly MessageQueue queue;

        private readonly SensorSettingsDbContext settingsContext;
        private readonly SensorReportDbContext reportsContext;

        private readonly string address;
        private readonly int port;

        private readonly Dictionary<string, Guid> connectionToGuid = new Dictionary<string, Guid>();

        private readonly Dictionary<Opcode, Action<string, int, ArraySegment<byte>>> receiveActions;

        public TCPServerWorker(
            ILogger<TCPServerWorker> logger,
            MessageQueue queue,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.queue = queue;

            this.settingsContext = serviceProvider
                .CreateScope().ServiceProvider
                .GetRequiredService<SensorSettingsDbContext>();

            this.reportsContext = serviceProvider
                .CreateScope().ServiceProvider
                .GetRequiredService<SensorReportDbContext>();

            address = configuration["TcpServer:Address"];
            port = Convert.ToInt32(configuration["TcpServer:Port"]);

            server = new SimpleTcpServer(address, port);

            server.Events.ClientConnected += ClientConnected;
            server.Events.ClientDisconnected += ClientDisconnected;
            server.Events.DataReceived += DataReceived;

            receiveActions =
            new Dictionary<Opcode, Action<string, int, ArraySegment<byte>>>()
            {
                { Opcode.Identify, Identify },
                { Opcode.Report, Report }
            };
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

        private void ClientConnected(object sender, ConnectionEventArgs e)
        {
            logger.LogInformation($"Client Connected {e.IpPort}");
        }

        private void ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            logger.LogInformation($"Client Disconnected {e.IpPort}");
            connectionToGuid.Remove(e.IpPort);
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            Opcode opcode = (Opcode)e.Data.Array[0];
            int sizeOffset = (1 + sizeof(int));
            int size = BitConverter.ToInt32(e.Data.Array[1..sizeOffset]);
            int dataOffset = sizeOffset + size;

            if (receiveActions.TryGetValue(opcode, out var action))
            {
                action(e.IpPort, size, e.Data.Array[sizeOffset..dataOffset]);
            }
            else
            {
                logger.LogError("Unknown Opcode!");
            }
        }

        public void Identify(string client, int size, ArraySegment<byte> payload)
        {
            Guid id = new Guid(payload);

            if (id == Guid.Empty)
            {
                logger.LogInformation($"[{client}]: Requested new GUID");

                var ipAddressOnlySpan = client.AsSpan()[..(client.IndexOf(':') + 1)];
                IPAddress newIp = IPAddress.Parse(ipAddressOnlySpan);

                Shared.SensorSettings sensorSettings = new Shared.SensorSettings()
                {
                    IPAddress = newIp,
                };

                var dbSensor = settingsContext.SensorSettings.FirstOrDefault(x => x.Guid == id);
                if (dbSensor == null)
                {
                    settingsContext.SensorSettings.Add(sensorSettings);
                    int change = settingsContext.SaveChanges();
                    if (change > 0)
                    {
                        logger.LogInformation($"New Sensor Added {sensorSettings.Guid} - {sensorSettings.IPAddress}");

                        connectionToGuid[client] = sensorSettings.Guid;

                        using MemoryStream ms = new MemoryStream();
                        using var bw = new BinaryWriter(ms);

                        bw.Write((byte)Opcode.Identify);

                        byte[] data = sensorSettings.Guid.ToByteArray();
                        bw.Write(data.Length);
                        bw.Write(data);

                        queue.Send.Add(new KeyValuePair<string, byte[]>(client, ms.ToArray()));
                    }
                }
            }
            else
            {
                logger.LogInformation($"[{client}]: {id}");

                var dbSensor = settingsContext.SensorSettings.FirstOrDefault(x => x.Guid == id);
                if (dbSensor != null)
                    connectionToGuid[client] = id;
            }
        }

        public void Report(string client, int size, ArraySegment<byte> payload)
        {
            DateTime time = DateTime.FromBinary(BitConverter.ToInt64(payload));
            int tempF = BitConverter.ToInt32(payload.Slice(sizeof(long)));
            int usage = BitConverter.ToInt32(payload.Slice(sizeof(long) + sizeof(int)));

            Guid id = connectionToGuid[client];

            reportsContext.SensorReports.Add(new SensorReport()
            {
                SensorGuid = id,
                DateTime = time,
                TemperatureF = tempF,
                Usage = usage
            });

            int change = reportsContext.SaveChanges();
            if (change > 0)
            {
                logger.LogInformation($"Report added: {id} - {time} - {tempF} - {usage}");
            }
            else
            {
                logger.LogWarning($"Report not added: {id} - {time} - {tempF} - {usage}");
            }
        }

    }
}
