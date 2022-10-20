using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ComplexPrototypeSystem.Server.Data
{
    public sealed class MessageQueue
    {
        public BlockingCollection<KeyValuePair<string, string>> Send { get; } = new BlockingCollection<KeyValuePair<string, string>>();
        public BlockingCollection<KeyValuePair<string, string>> Recv { get; } = new BlockingCollection<KeyValuePair<string, string>>();
    }
}
