using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using ProjectM;
using ProjectM.Network;
using CrowbaneArena.Data;

namespace CrowbaneArena.Services
{
    /// <summary>
    /// Service for managing V Blood boss data and operations
    /// </summary>
    public static class VBloodService
    {
        private static bool _initialized = false;
        private static List<int> _allVBloodIds = new();
        private static Dictionary<ulong, HashSet<int>> _unlockedVBloods = new();

        /// <summary>
        /// Initialize the V Blood service
        /// </summary>
        public static bool Initialize()
        {
            try
            {
                if (_initialized)
                {
                    Plugin.Logger?.LogInfo("VBloodService already initialized");
                    return true;
                }

                Plugin.Logger?.LogInfo("Initializing VBloodService...");

                // Initialize V Blood data
                InitializeVBloodData();

                _initialized = true;
                Plugin.Logger?.LogInfo($"VBloodService initialized with {_allVBloodIds.Count} V Blood entries");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Failed to initialize VBloodService: {ex.Message}");
                Plugin.Logger?.LogError(ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Initialize V Blood boss data from game data
        /// </summary>
        private static void InitializeVBloodData()
        {
            try
            {
                // Get V Blood IDs from our VBloodGUIDs class
                _allVBloodIds = VBloodGUIDs.GetAll();
                Plugin.Logger?.LogInfo($"Initialized {_allVBloodIds.Count} V Blood entries");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error initializing V Blood data: {ex.Message}");
                _allVBloodIds = new List<int>();
            }
        }

        /// <summary>
        /// Unlock a V Blood for a player
        /// </summary>
        public static bool UnlockVBlood(ulong steamId, int vbloodId)
        {
            try
            {
                if (!_allVBloodIds.Contains(vbloodId))
                {
                    Plugin.Logger?.LogWarning($"Attempted to unlock invalid VBlood ID: {vbloodId}");
                    return false;
                }

                if (!_unlockedVBloods.ContainsKey(steamId))
                {
                    _unlockedVBloods[steamId] = new HashSet<int>();
                }

                if (_unlockedVBloods[steamId].Add(vbloodId))
                {
                    Plugin.Logger?.LogInfo($"Unlocked VBlood {vbloodId} for player {steamId}");
                    return true;
                }

                Plugin.Logger?.LogInfo($"Player {steamId} already has VBlood {vbloodId} unlocked");
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in UnlockVBlood: {ex.Message}");
                Plugin.Logger?.LogError(ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Unlock all V Bloods for a player
        /// </summary>
        public static bool UnlockAllVBloods(ulong steamId)
        {
            try
            {
                if (!_unlockedVBloods.ContainsKey(steamId))
                {
                    _unlockedVBloods[steamId] = new HashSet<int>();
                }

                int unlockedCount = 0;
                foreach (var vbloodId in _allVBloodIds)
                {
                    if (_unlockedVBloods[steamId].Add(vbloodId))
                    {
                        unlockedCount++;
                    }
                }

                Plugin.Logger?.LogInfo($"Unlocked {unlockedCount} V Bloods for player {steamId}");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in UnlockAllVBloods: {ex.Message}");
                Plugin.Logger?.LogError(ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Check if a player has a specific V Blood unlocked
        /// </summary>
        public static bool HasVBlood(ulong steamId, int vbloodId)
        {
            return _unlockedVBloods.ContainsKey(steamId) && _unlockedVBloods[steamId].Contains(vbloodId);
        }

        /// <summary>
        /// Get all unlocked V Bloods for a player
        /// </summary>
        public static IEnumerable<int> GetUnlockedVBloods(ulong steamId)
        {
            return _unlockedVBloods.ContainsKey(steamId) 
                ? _unlockedVBloods[steamId].ToList() 
                : Enumerable.Empty<int>();
        }
    }
}
