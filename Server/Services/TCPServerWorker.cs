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
using System.Runtime.InteropServices;

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

        private readonly Dictionary<string, Guid> IpAddressToGuid = new Dictionary<string, Guid>();
        private readonly Dictionary<Guid, string> GuidToIpAddress = new Dictionary<Guid, string>();

        private readonly Dictionary<Opcode, Action<string, int, ArraySegment<byte>>> receiveHandlers;

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

            receiveHandlers =
            new Dictionary<Opcode, Action<string, int, ArraySegment<byte>>>()
            {
                { Opcode.Identify, HandleIdentify },
                { Opcode.Report, HandleReport }
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

                while (queue.SendInterval.TryTake(out var data) &&
                    !stoppingToken.IsCancellationRequested)
                {
                    Guid guid = Guid.Parse(data.Key);
                    if (GuidToIpAddress.TryGetValue(guid, out var ipAddressPort))
                    {
                        server.Send(ipAddressPort, data.Value);
                    }
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
                logger.LogInformation($"[{client}]: Disconnect by Server");
            }
        }

        private void ClientConnected(object sender, ConnectionEventArgs e)
        {
            logger.LogInformation($"[{e.IpPort}]: Connected");
        }

        private void ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            logger.LogInformation($"[{e.IpPort}]: Disconnected");

            if (IpAddressToGuid.TryGetValue(e.IpPort, out Guid guid))
                GuidToIpAddress.Remove(guid);

            IpAddressToGuid.Remove(e.IpPort);
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            Opcode opcode = (Opcode)e.Data.Array[0];

            if (OpCode_Extension.IsDefined(opcode))
            {
                int sizeOffset = (1 + sizeof(int));
                int size = BitConverter.ToInt32(e.Data.Array[1..sizeOffset]);
                int dataOffset = sizeOffset + size;

                if (receiveHandlers.TryGetValue(opcode, out var handler))
                {
                    handler(e.IpPort, size, e.Data.Array[sizeOffset..dataOffset]);
                }
                else
                {
                    logger.LogWarning($"No handler for {nameof(Opcode)} {opcode}");
                }
            }
            else
            {
                logger.LogError($"Unknown {nameof(Opcode)} {e.Data.Array[0]}");
            }
        }

        public void HandleIdentify(string client, int size, ArraySegment<byte> payload)
        {
            Guid id;
            try
            {
                id = new Guid(payload);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return;
            }

            if (id == Guid.Empty)
            {
                logger.LogInformation($"[{client}]: Requested new GUID");

                var ipAddressSpan = client.AsSpan()[..(client.IndexOf(':') + 1)];
                IPAddress newIp = IPAddress.Parse(ipAddressSpan);

                Shared.SensorSettings sensorSettings = new Shared.SensorSettings()
                {
                    IPAddress = newIp,
                };

                if (!settingsContext.SensorSettings.Any(x => x.Guid == id))
                {
                    settingsContext.SensorSettings.Add(sensorSettings);
                    int change = settingsContext.SaveChanges();
                    if (change > 0)
                    {
                        logger.LogInformation($"[{client}]: New Sensor Added {sensorSettings.Guid} - {sensorSettings.IPAddress}");

                        IpAddressToGuid[client] = sensorSettings.Guid;
                        GuidToIpAddress[sensorSettings.Guid] = client;

                        using var ms = new MemoryStream();
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
                logger.LogInformation($"[{client}]: Identified as {id} - Sending Interval");

                var dbSensor = settingsContext.SensorSettings.FirstOrDefault(x => x.Guid == id);
                if (dbSensor != null)
                {
                    IpAddressToGuid[client] = id;
                    GuidToIpAddress[id] = client;

                    using var ms = new MemoryStream();
                    using var bw = new BinaryWriter(ms);

                    bw.Write((byte)Opcode.SetInterval);

                    int sizeInterval = Marshal.SizeOf(dbSensor.Interval);
                    bw.Write(sizeInterval);
                    bw.Write(dbSensor.Interval);

                    queue.Send.Add(new KeyValuePair<string, byte[]>(client, ms.ToArray()));
                }
            }
        }

        public void HandleReport(string client, int size, ArraySegment<byte> payload)
        {
            DateTime time;
            int tempF;
            int usage;
            try
            {
                time = DateTime.FromBinary(BitConverter.ToInt64(payload));
                tempF = BitConverter.ToInt32(payload.Slice(sizeof(long)));
                usage = BitConverter.ToInt32(payload.Slice(sizeof(long) + sizeof(int)));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return;
            }

            if (IpAddressToGuid.TryGetValue(client, out Guid id))
            {
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
                    logger.LogInformation($"[{client}]: Report added: {id} - {time} - {tempF} - {usage}");
                }
                else
                {
                    logger.LogWarning($"[{client}]: Report not added: {id} - {time} - {tempF} - {usage}");
                }
            }
            else
            {
                logger.LogWarning($"[{client}]: Unable to match {client} -> {nameof(SensorReport.SensorGuid)}");
            }
        }
    }
}
