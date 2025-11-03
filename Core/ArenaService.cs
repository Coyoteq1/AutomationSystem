using System;
using BepInEx.Logging;

// Simplified arena service for basic functionality
namespace AutomationSystem.Core
{
    /// <summary>
    /// Basic arena service for core functionality
    /// </summary>
    public class ArenaService
    {
        private static ManualLogSource _logger;

        public ArenaService()
        {
            _logger = Plugin.LogSource;
        }

        /// <summary>
        /// Initialize the arena service
        /// </summary>
        public void Initialize()
        {
            _logger?.LogInfo("ArenaService initializing...");

            // Initialize core services
            AutomationSystemCore.Initialize();

            _logger?.LogInfo("ArenaService initialized successfully");
        }

        /// <summary>
        /// Get service status for monitoring
        /// </summary>
        public string GetServiceStatus()
        {
            return $"ArenaService - Initialized: {AutomationSystemCore.HasInitialized}";
        }
    }
}
