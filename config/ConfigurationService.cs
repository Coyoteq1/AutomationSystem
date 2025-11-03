using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AutomationSystem.config
{
    /// <summary>
    /// Implementation of configuration service functionality
    /// Based on ConfigService from VRising arena system
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private readonly AutomationSystemOptions _options;
        
        private object? _config;
        private readonly string _configPath;

        /// <summary>
        /// Constructor for ConfigurationService
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="options">The automation system options</param>
        public ConfigurationService(ILogger<ConfigurationService> logger, IOptions<AutomationSystemOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            
            // Determine config path from data directory
            _configPath = Path.Combine(_options.DataDirectory, "config.json");
            
            _logger.LogInformation("ConfigurationService initialized with config path: {ConfigPath}", _configPath);
        }

        /// <summary>
        /// Gets the configuration file path
        /// </summary>
        public string ConfigPath => _configPath;

        /// <summary>
        /// Checks if configuration file exists
        /// </summary>
        public bool ConfigExists => File.Exists(_configPath);

        /// <summary>
        /// Gets the current configuration
        /// </summary>
        /// <typeparam name="T">The type of configuration</typeparam>
        /// <returns>The configuration instance</returns>
        public T? GetConfig<T>() where T : class
        {
            if (_config is T config)
            {
                return config;
            }

            LoadConfig();
            return _config as T;
        }

        /// <summary>
        /// Saves a configuration object
        /// </summary>
        /// <typeparam name="T">The type of configuration</typeparam>
        /// <param name="config">The configuration to save</param>
        public void SaveConfig<T>(T config) where T : class
        {
            try
            {
                if (config == null)
                {
                    _logger.LogWarning("Attempted to save null configuration");
                    return;
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serialize to JSON
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configPath, json);

                _config = config;
                _logger.LogInformation("Configuration saved successfully to {ConfigPath}", _configPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration to {ConfigPath}", _configPath);
            }
        }

        /// <summary>
        /// Reloads the configuration from disk
        /// </summary>
        public void Reload()
        {
            _config = null;
            LoadConfig();
            _logger.LogInformation("Configuration reloaded from {ConfigPath}", _configPath);
        }

        /// <summary>
        /// Validates the current configuration
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public bool ValidateConfig()
        {
            try
            {
                if (!ConfigExists)
                {
                    _logger.LogWarning("Configuration file does not exist at {ConfigPath}", _configPath);
                    return false;
                }

                var json = File.ReadAllText(_configPath);
                var doc = JsonConvert.DeserializeObject(json);
                
                if (doc != null)
                {
                    _logger.LogInformation("Configuration validation successful for {ConfigPath}", _configPath);
                    return true;
                }
                
                _logger.LogWarning("Configuration file at {ConfigPath} contains invalid JSON", _configPath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration at {ConfigPath}", _configPath);
                return false;
            }
        }

        /// <summary>
        /// Loads the configuration from disk
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                if (ConfigExists)
                {
                    var json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject(json);
                    _logger.LogInformation("Configuration loaded successfully");
                }
                else
                {
                    // Create default configuration
                    _config = CreateDefaultConfig();
                    SaveConfig(_config);
                    _logger.LogInformation("Default configuration created");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration from {ConfigPath}", _configPath);
                _config = CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Creates a default configuration
        /// </summary>
        /// <returns>The default configuration object</returns>
        private object CreateDefaultConfig()
        {
            return new AutomationConfiguration
            {
                MaxEntities = 1000,
                AutoPersistenceEnabled = true,
                PersistenceIntervalSeconds = 60,
                BackupEnabled = true,
                MaxBackups = 5,
                TrackingEnabled = true,
                SnapshotRetentionDays = 30
            };
        }
    }

    /// <summary>
    /// Configuration model for AutomationSystem
    /// </summary>
    public class AutomationConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum number of entities to track
        /// </summary>
        public int MaxEntities { get; set; } = 1000;

        /// <summary>
        /// Gets or sets whether automatic persistence is enabled
        /// </summary>
        public bool AutoPersistenceEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the persistence interval in seconds
        /// </summary>
        public int PersistenceIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets whether backup is enabled
        /// </summary>
        public bool BackupEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of backups to keep
        /// </summary>
        public int MaxBackups { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether tracking is enabled
        /// </summary>
        public bool TrackingEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of days to retain snapshots
        /// </summary>
        public int SnapshotRetentionDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the data directory path
        /// </summary>
        public string DataDirectory { get; set; } = "Data/AutomationSystem";

        /// <summary>
        /// Gets or sets the log level for the system
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// Gets or sets whether debug mode is enabled
        /// </summary>
        public bool DebugMode { get; set; } = false;
    }
}