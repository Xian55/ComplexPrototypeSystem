using System;
using System.Text.Json;

using System.IO;

using ComplexPrototypeSystem.Service.Data;

using Microsoft.Extensions.Logging;

namespace ComplexPrototypeSystem.Service.DAO
{
    public sealed class ConfigDAO
    {
        private readonly ILogger<ConfigDAO> logger;

        public Config Config { get; }

        public ConfigDAO(ILogger<ConfigDAO> logger)
        {
            this.logger = logger;

            string fileName = Path.Join(AppDomain.CurrentDomain.BaseDirectory, ConfigMeta.FileName);

            if (!File.Exists(fileName))
            {
                Config = new Config();

                Save();

                logger.LogInformation($"Config created {fileName}");
            }
            else
            {
                string json = File.ReadAllText(fileName);
                Config = JsonSerializer.Deserialize<Config>(json);
            }

            logger.LogInformation($"Config loaded id: {Config.Id} | interval: {Config.Interval}ms");
        }

        public void Save()
        {
            string fileName = Path.Join(AppDomain.CurrentDomain.BaseDirectory, ConfigMeta.FileName);
            string json = JsonSerializer.Serialize(Config);
            File.WriteAllText(fileName, json);
            logger.LogInformation("Config saved");
        }
    }
}
