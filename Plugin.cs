using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using CrowbaneArena.Services;
using Unity.Entities;
using VampireCommandFramework;

namespace CrowbaneArena
{
    /// <summary>
    /// The main plugin class for Crowbane Arena.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    [BepInProcess("VRisingServer.exe")]
    public class Plugin : BasePlugin
    {
        /// <summary>
        /// The unique GUID for the plugin.
        /// </summary>
        public const string PluginGuid = "com.icb.crowbane.arena";
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public const string PluginName = "Crowbane Arena";
        /// <summary>
        /// The version of the plugin.
        /// </summary>
        public const string PluginVersion = "0.1.0";

        /// <summary>
        /// The logger instance for the plugin.
        /// </summary>
        public static BepInEx.Logging.ManualLogSource Logger { get; private set; } = null!;

        private static bool _initialized = false;
        
        /// <summary>
        /// Called when the plugin is loaded.
        /// </summary>
        public override void Load()
        {
            try
            {
                Logger = Log;

                Logger.LogInfo($"{PluginName} v{PluginVersion} loading...");

                // Initialize core services first
                CrowbaneArenaCore.Initialize();

                // Initialize VRisingCore with proper world detection (ICB.core compatible)
                VRisingCore.Initialize();

                // PlayerService will be initialized by PlayerTracker when the world is ready

                // PlayerTracker will be initialized when the first ServerBootstrapSystem update runs
                // This ensures the world is fully loaded before we try to access EntityManager

                // Register all commands in this assembly with VCF
                CommandRegistry.RegisterAll();

                // Load zone configuration
                ArenaConfigLoader.Initialize();

                // Initialize arena territory grid
                ArenaTerritory.InitializeArenaGrid();

                // Apply Harmony patches with error handling
                try
                {
                    var harmony = new Harmony(PluginGuid);
                    
                    // Get all patch types in the current assembly
                    var patchTypes = AccessTools.GetTypesFromAssembly(typeof(Plugin).Assembly)
                        .Where(t => t != null && t.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0)
                        .ToList();
                    
                    if (patchTypes.Count == 0)
                    {
                        Logger?.LogWarning("No Harmony patches found to apply");
                    }
                    else
                    {
                        Logger?.LogInfo($"Applying {patchTypes.Count} patch types...");
                        
                        // Apply patches one by one with error handling
                        foreach (var type in patchTypes)
                        {
                            if (type == null) continue;
                            
                            try
                            {
                                var patchInfo = HarmonyMethodExtensions.GetFromType(type);
                                if (patchInfo != null)
                                {
                                    var processor = new PatchClassProcessor(harmony, type);
                                    var patchMethods = processor.Patch();
                                    Logger?.LogInfo($"Applied {patchMethods?.Count ?? 0} patches from {type.Name}");
                                }
                            }
                            catch (Exception patchEx)
                            {
                                Logger?.LogError($"Failed to apply patch {type?.Name ?? "[unknown]"}: {patchEx.Message}");
                                Logger?.LogError(patchEx.StackTrace);
                            }
                        }
                    }

                    _initialized = true;
                    PlayerTracker.Initialize();
                    Logger.LogInfo($"{PluginName} v{PluginVersion} loaded successfully!");
                }
                catch (Exception patchAllEx)
                {
                    Logger?.LogError($"Error during Harmony patching: {patchAllEx.Message}");
                    Logger?.LogError(patchAllEx.StackTrace);
                    throw; // Re-throw to prevent loading with broken patches
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Failed to load {PluginName}: {ex.Message}");
                Logger?.LogError($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to prevent loading
            }
        }





        /// <summary>
        /// Called when the plugin is destroyed.
        /// </summary>
        void OnDestroy()
        {
            // Nothing to do
        }
    }
}
