using System.Collections.Concurrent;

namespace ComplexPrototypeSystem.Service.Data
{
    public class MessageQueue
    {
        public BlockingCollection<string> Send { get; } = new BlockingCollection<string>();
        public BlockingCollection<string> Recv { get; } = new BlockingCollection<string>();
    }
}
