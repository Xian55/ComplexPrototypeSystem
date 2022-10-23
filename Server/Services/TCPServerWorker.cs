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
using ComplexPrototypeSystem.Server.Messaging;

namespace ComplexPrototypeSystem.Server.Services
{
    public sealed class TCPServerWorker : BackgroundService
    {
        private readonly ILogger<TCPServerWorker> logger;
        private readonly SimpleTcpServer server;
        private readonly MessageQueue queue;
        private readonly SensorOnlineStatus sensorOnlineStatus;

        private readonly SensorSettingsDbContext settingsContext;
        private readonly SensorReportDbContext reportsContext;

        private readonly string address;
        private readonly int port;

        private readonly Dictionary<string, Guid> IpAddressToGuid = new Dictionary<string, Guid>();

        private readonly Dictionary<Opcode, Action<string, int, ArraySegment<byte>>> receiveHandlers;

        public TCPServerWorker(
            ILogger<TCPServerWorker> logger,
            MessageQueue queue,
            SensorOnlineStatus sensorOnlineStatus,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.queue = queue;
            this.sensorOnlineStatus = sensorOnlineStatus;

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
                    if (sensorOnlineStatus.GuidToIpAddress.TryGetValue(guid, out var ipAddressPort))
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
            foreach (var ipPort in server.GetClients())
            {
                server.DisconnectClient(ipPort);
                logger.LogInformation($"[{ipPort}]: Disconnect by Server");
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
                sensorOnlineStatus.GuidToIpAddress.TryRemove(guid, out _);

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

        public void HandleIdentify(string ipPort, int size, ArraySegment<byte> payload)
        {
            Guid sensorId;
            IPAddress newIp;
            try
            {
                sensorId = new Guid(payload);
                var ipAddressSpan = ipPort.AsSpan()[..(ipPort.LastIndexOf(':'))];
                newIp = IPAddress.Parse(ipAddressSpan);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return;
            }

            if (sensorId == Guid.Empty)
            {
                logger.LogInformation($"[{ipPort}]: Requested new GUID");

                Shared.SensorSettings sensorSettings = new Shared.SensorSettings()
                {
                    IPAddress = newIp,
                };

                if (!settingsContext.SensorSettings.Any(x => x.Guid == sensorId))
                {
                    settingsContext.SensorSettings.Add(sensorSettings);
                    int change = settingsContext.SaveChanges();
                    if (change > 0)
                    {
                        logger.LogInformation($"[{ipPort}]: New Sensor Added {sensorSettings.Guid} - {sensorSettings.IPAddress}");

                        IpAddressToGuid[ipPort] = sensorSettings.Guid;
                        sensorOnlineStatus.GuidToIpAddress.TryAdd(sensorSettings.Guid, ipPort);

                        using var ms = new MemoryStream();
                        using var bw = new BinaryWriter(ms);

                        bw.Write((byte)Opcode.Identify);

                        byte[] data = sensorSettings.Guid.ToByteArray();
                        bw.Write(data.Length);
                        bw.Write(data);

                        queue.Send.Add(new KeyValuePair<string, byte[]>(ipPort, ms.ToArray()));
                    }
                }
            }
            else
            {
                logger.LogInformation($"[{ipPort}]: Identified as {sensorId} - Sending Interval");

                var dbSensor = settingsContext.SensorSettings.FirstOrDefault(x => x.Guid == sensorId);
                if (dbSensor != null)
                {
                    IpAddressToGuid[ipPort] = sensorId;
                    sensorOnlineStatus.GuidToIpAddress.TryAdd(sensorId, ipPort);

                    if (!dbSensor.IPAddress.Equals(newIp))
                    {
                        logger.LogInformation($"[{ipPort}]: Joined from a new IP Address. From {dbSensor.IPAddress} to {newIp}");

                        dbSensor.IPAddress = newIp;

                        settingsContext.SaveChanges();
                    }

                    using var ms = new MemoryStream();
                    using var bw = new BinaryWriter(ms);

                    bw.Write((byte)Opcode.SetInterval);

                    int sizeInterval = Marshal.SizeOf(dbSensor.Interval);
                    bw.Write(sizeInterval);
                    bw.Write(dbSensor.Interval);

                    queue.Send.Add(new KeyValuePair<string, byte[]>(ipPort, ms.ToArray()));
                }
            }
        }

        public void HandleReport(string IpPort, int size, ArraySegment<byte> payload)
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

            if (IpAddressToGuid.TryGetValue(IpPort, out Guid id))
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
                    logger.LogInformation($"[{IpPort}]: Report added: {id} - {time} - {tempF} - {usage}");
                }
                else
                {
                    logger.LogWarning($"[{IpPort}]: Report not added: {id} - {time} - {tempF} - {usage}");
                }
            }
            else
            {
                logger.LogWarning($"[{IpPort}]: Unable to match {IpPort} -> {nameof(SensorReport.SensorGuid)}");
            }
        }
    }
}
