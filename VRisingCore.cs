using Unity.Entities;
using UnityEngine;
using ProjectM;
using ProjectM.Network;
using System;
using BepInEx.Logging;

namespace AutomationSystem
{
    /// <summary>
    /// Direct access to VRising game systems without lifecycle management
    /// </summary>
    public static class VRisingCore
    {
        private static ManualLogSource _logger;

        /// <summary>
        /// Initialize the VRisingCore logger
        /// </summary>
        public static void Initialize(ManualLogSource logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Direct access to the game's EntityManager
        /// </summary>
        public static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        /// <summary>
        /// Direct access to the game's ServerGameManager
        /// Note: This is a placeholder - the actual ServerGameManager type needs to be determined
        /// </summary>
        public static object ServerGameManager => null; // Placeholder until correct type is found

        /// <summary>
        /// Direct access to the game's PrefabCollectionSystem
        /// </summary>
        public static PrefabCollectionSystem PrefabCollectionSystem
        {
            get
            {
                var world = World.DefaultGameObjectInjectionWorld;
                return world.GetExistingSystemManaged<PrefabCollectionSystem>();
            }
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        public static void LogInfo(string message)
        {
            _logger?.LogInfo(message);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void LogError(string message)
        {
            _logger?.LogError(message);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void LogWarning(string message)
        {
            _logger?.LogWarning(message);
        }
    }
}
