using System.Collections.Concurrent;

namespace ComplexPrototypeSystem.Service.Data
{
    public sealed class MessageQueue
    {
        public BlockingCollection<byte[]> Send { get; } = new BlockingCollection<byte[]>();
        public BlockingCollection<byte[]> PrioritySend { get; } = new BlockingCollection<byte[]>();
    }
}
