using System.IO;
using Newtonsoft.Json;

namespace CrowbaneArena
{
    /// <summary>
    /// Handles loading and accessing configuration settings.
    /// </summary>
    public static class ConfigHandler
    {
        private static readonly string configPath = Path.Combine("config", "crowbanearena", "appsettings.json");
        private static dynamic config;

        static ConfigHandler()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject(json);
            }
        }

        public static bool GetEnableGodMode()
        {
            return config?.CrowbaneArena?.EnableGodMode ?? true;
        }

        public static string GetLogLevel()
        {
            return config?.CrowbaneArena?.LogLevel ?? "Info";
        }
    }
}
