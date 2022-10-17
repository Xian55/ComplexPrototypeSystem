using System;

namespace ComplexPrototypeSystem.Service.Data
{
    public static class ConfigMeta
    {
        public const string FileName = "config.json";
    }

    public class Config
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Interval { get; set; } = 5000;
    }
}
