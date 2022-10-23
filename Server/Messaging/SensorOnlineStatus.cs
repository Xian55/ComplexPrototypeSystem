using System;
using System.Collections.Concurrent;

namespace ComplexPrototypeSystem.Server.Messaging
{
    public sealed class SensorOnlineStatus
    {
        public ConcurrentDictionary<Guid, string> GuidToIpAddress { get; } = new ConcurrentDictionary<Guid, string>();
    }
}
