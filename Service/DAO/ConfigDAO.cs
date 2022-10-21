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

        private string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

        public ConfigDAO(ILogger<ConfigDAO> logger)
        {
            this.logger = logger;

            string fileName = Path.Join(BaseDir, ConfigMeta.FILE_NAME);

            try
            {
                string json = File.ReadAllText(fileName);
                Config = JsonSerializer.Deserialize<Config>(json);
                logger.LogInformation($"Config loaded id: {Config.Id} | interval: {Config.Interval}ms");
            }
            catch (FileNotFoundException)
            {
                Config = new Config();
                Save();
                logger.LogInformation($"Config created {fileName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public void Save()
        {
            try
            {
                string fileName = Path.Join(BaseDir, ConfigMeta.FILE_NAME);
                string json = JsonSerializer.Serialize(Config);
                File.WriteAllText(fileName, json);
                logger.LogInformation("Config saved");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public void SetInterval(int interval)
        {
            Config.Interval = interval;
            Save();
        }

        public void SetId(Guid id)
        {
            Config.Id = id;
            Save();
        }
    }
}
