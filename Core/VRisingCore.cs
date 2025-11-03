using BepInEx.Logging;
using System;
using Unity.Entities;

namespace AutomationSystem.Core
{
    /// <summary>
    /// Core VRising API access point
    /// </summary>
    public static class VRisingCore
    {
        private static IVRisingCore _instance;

        public static EntityManager EntityManager => _instance.EntityManager;
        public static object ServerGameManager { get; internal set; }

        /// <summary>
        /// Initialize VRisingCore with required services
        /// </summary>
        public static void Initialize(ManualLogSource logger)
        {
            _instance = new VRisingCoreImpl(logger);
            _instance.Initialize();
        }

        // For testing purposes
        public static void Initialize(IVRisingCore instance)
        {
            _instance = instance;
        }
    }
}
