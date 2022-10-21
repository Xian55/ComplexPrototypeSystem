using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ComplexPrototypeSystem.Server.Data
{
    public sealed class MessageQueue
    {
        public BlockingCollection<KeyValuePair<string, byte[]>> Send { get; } = new BlockingCollection<KeyValuePair<string, byte[]>>();
        public BlockingCollection<KeyValuePair<string, byte[]>> SendInterval { get; } = new BlockingCollection<KeyValuePair<string, byte[]>>();
    }
}
