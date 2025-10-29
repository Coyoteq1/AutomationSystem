using System;
using System.Linq;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Stunlock.Core;
using ProjectM.Network;
using CrowbaneArena.Services;

namespace CrowbaneArena
{
    /// <summary>
    /// Main controller for CrowbaneArena mod with enhanced progression management.
    /// </summary>
    public class ArenaController
    {
        private static float3 entryPoint = float3.zero;
        private static float3 exitPoint = float3.zero;
        private static float3 spawnPoint = float3.zero;
        
        // Initialize the controller
        static ArenaController()
        {
            // Initialize the snapshot manager
            SnapshotManagerService.Initialize();
        }

        /// <summary>
        /// Gets the singleton instance of the ArenaController.
        /// </summary>
        public static ArenaController Instance { get; private set; } = new ArenaController();

        /// <summary>
        /// Gets the EntityManager instance from the current world.
        /// </summary>
        private static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        /// <summary>
        /// Handles player entering the arena with enhanced progression capture.
        /// </summary>
        public void OnPlayerEnterArena(Entity userEntity, Entity characterEntity)
        {
            try
            {
                Plugin.Logger?.LogInfo("=== ARENA ENTRY STARTED ===");

                // Get arena spawn location from configuration
                var arenaLocation = GetDefaultArenaLocation();

                // Get player's Steam ID and name
                var user = EntityManager.GetComponentData<User>(userEntity);
                
                // Use SnapshotManagerService to handle arena entry with proper state management
                bool success = SnapshotManagerService.EnterArena(
                    userEntity: userEntity,
                    characterEntity: characterEntity,
                    arenaLocation: arenaLocation,
                    loadoutName: "default",
                    zoneName: "ArenaZone"
                );

                if (success)
                {
                    Plugin.Logger?.LogInfo($"=== ARENA ENTRY COMPLETED FOR {user.CharacterName} ===");
                    ArenaHook.MarkPlayerEnteredArena();
                }
                else
                {
                    Plugin.Logger?.LogError("=== ARENA ENTRY FAILED ===");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger?.LogError($"Critical error in OnPlayerEnterArena: {ex.Message}");
                Plugin.Logger?.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Handles player exiting the arena with enhanced progression restoration.
        /// </summary>
        public void OnPlayerExitArena(Entity userEntity, Entity characterEntity)
        {
            try
            {
                Plugin.Logger?.LogInfo("=== ARENA EXIT STARTED ===");

                // Get player's Steam ID and name
                var user = EntityManager.GetComponentData<User>(userEntity);
                
                // Use SnapshotManagerService to handle arena exit with proper state restoration
                bool success = SnapshotManagerService.ExitArena(userEntity, characterEntity);

                if (success)
                {
                    Plugin.Logger?.LogInfo($"=== ARENA EXIT COMPLETED FOR {user.CharacterName} ===");
                    ArenaHook.MarkPlayerExitedArena();
                }
                else
                {
                    Plugin.Logger?.LogError("=== ARENA EXIT FAILED ===");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger?.LogError($"Critical error in OnPlayerExitArena: {ex.Message}");
                Plugin.Logger?.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Get the default arena spawn location from configuration.
        /// </summary>
        private float3 GetDefaultArenaLocation()
        {
            if (ArenaConfigLoader.ArenaSettings?.Zones != null && ArenaConfigLoader.ArenaSettings.Zones.Count > 0)
            {
                var defaultZone = ArenaConfigLoader.ArenaSettings.Zones.FirstOrDefault(z => z.Enabled);
                if (defaultZone != null)
                {
                    return new float3(defaultZone.SpawnX, defaultZone.SpawnY, defaultZone.SpawnZ);
                }
            }

            // Fallback to hardcoded location if config not available
            return new float3(-1000.0f, 0.0f, -500.0f);
        }

        /// <summary>
        /// Check if a player is currently in the arena.
        /// </summary>
        public bool IsPlayerInArena(ulong platformId)
        {
            return SnapshotManagerService.IsInArena(platformId);
        }

        /// <summary>
        /// Get the number of active arena snapshots.
        /// </summary>
        public int GetActiveSnapshotCount()
        {
            // This is a placeholder - SnapshotManagerService doesn't expose a direct count
            // of active arena players. We could add this to SnapshotManagerService if needed.
            // For now, we'll return 0 to indicate we don't have this information.
            return 0;
        }

        /// <summary>
        /// Force clear all arena snapshots (for admin use).
        /// </summary>
        public void ClearAllSnapshots()
        {
            SnapshotManagerService.ClearAllSnapshots();
            Plugin.Logger?.LogInfo("All arena snapshots have been cleared.");
        }

        // Removed SimulateArenaEntry and SimulateArenaExit as they are no longer needed
        // The functionality is now handled by SnapshotManagerService

        /// <summary>
        /// Set arena zone radius (static method for commands)
        /// </summary>
        public static void SetZoneRadius(float radius)
        {
            Plugin.Logger?.LogInfo($"Arena zone radius set to: {radius}");
            // TODO: Implement actual zone radius setting
        }

        /// <summary>
        /// Set entry point location and radius (static method for commands)
        /// </summary>
        public static void SetEntryPoint(float3 position, float radius)
        {
            entryPoint = position;
            Plugin.Logger?.LogInfo($"Entry point set at {position} with radius {radius}");
            // TODO: Implement actual entry point setting
        }

        /// <summary>
        /// Set exit point location and radius (static method for commands)
        /// </summary>
        public static void SetExitPoint(float3 position, float radius)
        {
            exitPoint = position;
            Plugin.Logger?.LogInfo($"Exit point set at {position} with radius {radius}");
            // TODO: Implement actual exit point setting
        }

        /// <summary>
        /// Set arena spawn point (static method for commands)
        /// </summary>
        public static void SetSpawnPoint(float3 position)
        {
            ZoneManager.SetSpawnPoint(position);
            spawnPoint = position; // Keep local copy for backward compatibility if needed
            Plugin.Logger?.LogInfo($"Arena spawn point set at: {position}");
        }

        /// <summary>
        /// Get the spawn point position.
        /// </summary>
        public static float3 GetSpawnPoint() => ZoneManager.SpawnPoint;

        /// <summary>
        /// Handle player entering arena (static method for commands)
        /// </summary>
        public static void PlayerEnterArena(Entity player)
        {
            Plugin.Logger?.LogInfo($"Player {player} entering arena");
            Instance.OnPlayerEnterArena(Entity.Null, player);
        }

        /// <summary>
        /// Handle player exiting arena (static method for commands)
        /// </summary>
        public static void PlayerExitArena(Entity player)
        {
            Plugin.Logger?.LogInfo($"Player {player} exiting arena");
            Instance.OnPlayerExitArena(Entity.Null, player);
        }

        /// <summary>
        /// Check if player is in arena (static method for commands)
        /// </summary>
        public static bool IsPlayerInArena(Entity player)
        {
            return PlayerManager.GetPlayerState(player).IsInArena;
        }

        /// <summary>
        /// Get the entry point position.
        /// </summary>
        public static float3 GetEntryPoint() => entryPoint;

        /// <summary>
        /// Get the exit point position.
        /// </summary>
        public static float3 GetExitPoint() => exitPoint;

        /// <summary>
        /// Get current arena status (static method for commands)
        /// </summary>
        public static string GetArenaStatus()
        {
            return "Arena Available";
        }
    }
}