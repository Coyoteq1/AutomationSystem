using System.IO;
using System.Text.Json;

namespace CrowbaneArena
{
    /// <summary>
    /// Handles loading and accessing configuration settings.
    /// </summary>
    public static class ConfigHandler
    {
        private static readonly string configPath = Path.Combine("config", "crowbanearena", "appsettings.json");
        private static AppConfig? config;

        static ConfigHandler()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                try
                {
                    config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip
                    });
                }
                catch (JsonException)
                {
                    // Keep config null on parse failure; callers will use defaults
                    config = null;
                }
            }
        }

        public static bool cunstomsEnabled() => config?.CrowbaneArena?.EnableGodMode ?? true;

        public static string GetLogLevel()
        {
            return config?.CrowbaneArena?.LogLevel ?? "Info";
        }

        // POCO types for System.Text.Json deserialization
        private class AppConfig
        {
            public CrowbaneConfig? CrowbaneArena { get; set; }
        }

        private class CrowbaneConfig
        {
            public bool? EnableGodMode { get; set; }
            public string? LogLevel { get; set; }
        }
    }
}