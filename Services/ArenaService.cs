using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using ProjectM;
using Stunlock.Core;

namespace CrowbaneArena.Services
{
    /// <summary>
    /// Service for managing arena-related functionality
    /// </summary>
    public static class ArenaService
    {
        private static EntityManager _entityManager => CrowbaneArenaCore.EntityManager;
        private static readonly HashSet<Entity> _playersInArena = new();
        private static float3 _arenaCenter = new(0, 0, 0);
        private static float _arenaRadius = 100f;

        /// <summary>
        /// Initialize the arena service
        /// </summary>
        public static void Initialize(float3 center, float radius)
        {
            _arenaCenter = center;
            _arenaRadius = radius;
            Plugin.Logger?.LogInfo($"ArenaService initialized at {center} with radius {radius}");
        }

        /// <summary>
        /// Check if a player is in the arena
        /// </summary>
        public static bool IsPlayerInArena(Entity player)
        {
            return _playersInArena.Contains(player);
        }

        /// <summary>
        /// Add a player to the arena
        /// </summary>
        public static void AddPlayerToArena(Entity player)
        {
            if (!_playersInArena.Add(player)) return;
            
            // Apply arena buffs or effects
            // BuffUtility.BuffPlayer(_entityManager, player, Prefabs.Buff_Power_General_Buff, -1);
            
            Plugin.Logger?.LogInfo($"Player {player.Index} added to arena");
        }

        /// <summary>
        /// Remove a player from the arena
        /// </summary>
        public static void RemovePlayerFromArena(Entity player)
        {
            if (!_playersInArena.Remove(player)) return;
            
            // Remove arena-specific buffs
            // BuffUtility.RemoveBuff(_entityManager, player, Prefabs.Buff_Power_General_Buff);
            
            Plugin.Logger?.LogInfo($"Player {player.Index} removed from arena");
        }

        /// <summary>
        /// Check if a position is within arena bounds
        /// </summary>
        public static bool IsInArenaBounds(float3 position)
        {
            var distance = math.distance(position, _arenaCenter);
            return distance <= _arenaRadius;
        }

        /// <summary>
        /// Get arena spawn point
        /// </summary>
        public static float3 GetSpawnPoint()
        {
            return _arenaCenter;
        }

        /// <summary>
        /// Handle player death in arena
        /// </summary>
        public static void HandleArenaDeath(Entity victim, Entity killer)
        {
            if (!_playersInArena.Contains(victim)) return;
            
            // Handle respawn or other death logic
            Plugin.Logger?.LogInfo($"Player {victim.Index} died in arena");
            
            // Example: Respawn after delay
            // _ = RespawnPlayer(victim, 5f);
        }

        /// <summary>
        /// Get all players in the arena
        /// </summary>
        public static IEnumerable<Entity> GetPlayersInArena()
        {
            return _playersInArena;
        }
    }
}
