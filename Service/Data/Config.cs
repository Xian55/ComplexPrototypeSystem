﻿using System;

namespace ComplexPrototypeSystem.Service.Data
{
    public static class ConfigMeta
    {
        public const string FILE_NAME = "config.json";
    }

    public sealed class Config
    {
        public Guid Id { get; set; } = Guid.Empty;

        public int Interval { get; set; } = 5000;
    }
}
