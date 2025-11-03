using AutomationSystem.Core;
using AutomationSystem.Snapshots;
using AutomationSystem.Automation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AutomationSystem.Services;

namespace AutomationSystem
{
    /// <summary>
    /// Main service collection extensions for the AutomationSystem
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds AutomationSystem services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="configureOptions">Optional configuration action for AutomationSystem options</param>
        /// <returns>The service collection with AutomationSystem services added</returns>
        public static IServiceCollection AddAutomationSystem(this IServiceCollection services, Action<AutomationSystemOptions>? configureOptions = null)
        {
            // Register options with configuration
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                services.Configure<AutomationSystemOptions>(options => { });
            }

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add core services
            services.AddSingleton<ICoreDataManager, CoreDataManager>();
            services.AddSingleton<ISnapshotManager, SnapshotManager>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IAutomationTracker, AutomationTracker>();

            // Add CommandRegistry as singleton for logger access
            services.AddSingleton<Services.CommandRegistry>();

            // Add automation services (optional services with conditional registration)
            services.AddSingleton<Services.QueueService>();
            services.AddSingleton<Services.SessionService>();
            services.AddSingleton<Services.SystemService>();

            // Add utility services
            services.AddSingleton<Services.LoggingService>();
            services.AddSingleton<Services.LocalizationService>();
            services.AddSingleton<Services.VectorService>();

            return services;
        }



        /// <summary>
        /// Adds all AutomationSystem command services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddAutomationSystemCommands(this IServiceCollection services)
        {
            // Command registry is already handled in Plugin.cs
            return services;
        }

        /// <summary>
        /// Adds game-specific services for V Rising integration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddGameServices(this IServiceCollection services)
        {
            // Add arena-specific services if they exist
            // services.AddSingleton<AutomationSystem.Services.ArenaServiceConfig>();
            
            return services;
        }

        /// <summary>
        /// Adds all system services with proper lifecycle management
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddAutomationSystemSystemServices(this IServiceCollection services)
        {
            // Add system context only if Unity types are available
            try
            {
                // services.AddSingleton<AutomationSystem.Core.SystemContext>();
            }
            catch
            {
                // SystemContext has Unity dependencies that might not be available at runtime
                // Skip registration if there are issues
            }

            return services;
        }
    }

    /// <summary>
    /// Configuration options for AutomationSystem
    /// </summary>
    public class AutomationSystemOptions
    {
        /// <summary>
        /// Gets or sets the data directory path for storing automation data
        /// </summary>
        public string DataDirectory { get; set; } = "Data/AutomationSystem";

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
        /// Gets or sets the maximum number of entities to track
        /// </summary>
        public int MaxEntities { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the number of days to retain snapshots
        /// </summary>
        public int SnapshotRetentionDays { get; set; } = 30;

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
