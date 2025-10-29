using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Core;
using Stunlock.Network;
using CrowbaneArena.Data;
using CrowbaneArena.Services;

// Note: VRising.GameData and VRising.Systems are part of the game's assembly references
// Make sure your project has the correct references to the game's DLLs

namespace CrowbaneArena
{
    /// <summary>
    /// Manages V Blood unlocks and tracking for players in the arena
    /// </summary>
    public static class VBloodManager
    {
        // Track original VBloods for each player
        private static readonly Dictionary<ulong, List<PrefabGUID>> _originalVBloods = new();
        
        /// <summary>
        /// Unlock all V Bloods for a player
        /// </summary>
        public static void UnlockAllVBloods(Entity playerEntity)
        {
            try
            {
                var em = VRisingCore.EntityManager;
                
                if (playerEntity == Entity.Null || !em.Exists(playerEntity))
                {
                    Plugin.Logger?.LogError("Cannot unlock VBloods: Invalid player entity");
                    return;
                }

                // Get user entity and Steam ID
                var userEntity = em.GetComponentData<PlayerCharacter>(playerEntity).UserEntity;
                if (userEntity == Entity.Null)
                {
                    Plugin.Logger?.LogError("Cannot unlock VBloods: User entity not found");
                    return;
                }

                var user = em.GetComponentData<User>(userEntity);
                ulong steamId = user.PlatformId;
                
                Plugin.Logger?.LogInfo($"Unlocking all VBloods for player {user.CharacterName} (SteamID: {steamId})");

                // Save original VBloods if not already saved
                if (!_originalVBloods.ContainsKey(steamId))
                {
                    SaveOriginalVBloods(playerEntity, steamId);
                }

                // Initialize VBloodService if needed
                VBloodService.Initialize();
                
                // Unlock all VBloods through the service
                VBloodService.UnlockAllVBloods(steamId);

                // Get all VBlood prefabs
                var vbloodPrefabs = GetVBloodPrefabs();
                if (vbloodPrefabs == null || vbloodPrefabs.Count == 0)
                {
                    Plugin.Logger?.LogError("No VBlood prefabs found");
                    return;
                }

                // Add to VBloodConsumed buffer
                if (em.HasBuffer<VBloodConsumed>(playerEntity))
                {
                    var vbloodBuffer = em.GetBuffer<VBloodConsumed>(playerEntity);
                    
                    foreach (var vblood in vbloodPrefabs)
                    {
                        if (!HasVBlood(vbloodBuffer, vblood))
                        {
                            // Create VBloodConsumed with the correct GUID
                            try
                            {
                                var consumed = new VBloodConsumed();
                                var prefabGuidField = typeof(VBloodConsumed).GetField("PrefabGUID", 
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                
                                if (prefabGuidField != null)
                                {
                                    prefabGuidField.SetValue(consumed, vblood);
                                    vbloodBuffer.Add(consumed);
                                    Plugin.Logger?.LogDebug($"Added VBlood to buffer: {vblood.GuidHash}");
                                }
                                else
                                {
                                    Plugin.Logger?.LogError("Failed to find PrefabGUID field in VBloodConsumed");
                                }
                            }
                            catch (Exception ex)
                            {
                                Plugin.Logger?.LogError($"Error adding VBlood to buffer: {ex.Message}");
                            }
                            Plugin.Logger?.LogDebug($"Added VBlood to buffer: {vblood.GuidHash}");
                        }
                    }
                }

                // Trigger VBlood unlocked using server commands
                Plugin.Logger?.LogDebug($"Unlocked {vbloodPrefabs.Count} VBloods for player {user.CharacterName}");

                // Refresh player's VBlood UI by sending a network event
                try
                {
                    var networkId = em.GetComponentData<NetworkId>(playerEntity);
                    var eventEntity = em.CreateEntity(
                        ComponentType.ReadOnly<NetworkEventType>(),
                        ComponentType.ReadOnly<SendEventToUser>()
                    );
                    
                    // Use a custom event type for V Blood refresh
                    // Use a custom event ID for V Blood refresh
                    // The actual event ID should match what your client-side code expects
                    var networkEvent = new NetworkEventType()
                    {
                        EventId = 2001, // Custom event ID (using int as a fallback)
                        IsAdminEvent = false,
                        IsDebugEvent = false
                    };
                    // Note: You may need to use a specific enum type for EventId
                    // based on your game's networking implementation
                    em.SetComponentData(eventEntity, networkEvent);
                    
                    em.SetComponentData(eventEntity, new SendEventToUser() {
                        UserIndex = user.Index
                    });
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"Error refreshing VBlood UI: {ex.Message}");
                }
                
                Plugin.Logger?.LogInfo($"Successfully unlocked {vbloodPrefabs.Count} VBloods for player {user.CharacterName}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in UnlockAllVBloods: {ex.Message}");
                Plugin.Logger?.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Save player's original VBloods
        /// </summary>
        private static void SaveOriginalVBloods(Entity playerEntity, ulong steamId)
        {
            try
            {
                var em = VRisingCore.EntityManager;
                var originalVBloods = new List<PrefabGUID>();
                
                if (em.HasBuffer<VBloodConsumed>(playerEntity))
                {
                    var vbloodBuffer = em.GetBuffer<VBloodConsumed>(playerEntity);
                    foreach (var vblood in vbloodBuffer)
                    {
                        // Get the PrefabGUID from VBloodConsumed using reflection
                var prefabGuidField = typeof(VBloodConsumed).GetField("PrefabGUID", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (prefabGuidField != null)
                {
                    try 
                    {
 var prefabGuidValue = prefabGuidField.GetValue(vblood);
                        if (prefabGuidValue is PrefabGUID prefabGuid)
                        {
                            if (prefabGuid.GuidHash != 0) // Only add valid GUIDs
                            {
                                originalVBloods.Add(prefabGuid);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger?.LogError($"Error getting PrefabGUID: {ex.Message}");
                    }
                }
                    }
                }
                
                _originalVBloods[steamId] = originalVBloods;
                Plugin.Logger?.LogInfo($"Saved {originalVBloods.Count} original VBloods for player {steamId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error saving original VBloods: {ex.Message}");
            }
        }

        // Removed duplicate RestoreOriginalVBloods method

        /// <summary>
        /// Get all VBlood prefabs
        /// </summary>
        public static List<PrefabGUID> GetVBloodPrefabs()
        {
            try
            {
                var em = VRisingCore.EntityManager;
                var query = em.CreateEntityQuery(ComponentType.ReadOnly<VBloodUnit>());
                var entities = query.ToEntityArray(Allocator.Temp);
                var vbloods = new List<PrefabGUID>();

                foreach (var entity in entities)
                {
                    if (em.HasComponent<PrefabGUID>(entity))
                    {
                        var prefab = em.GetComponentData<PrefabGUID>(entity);
                        vbloods.Add(prefab);
                    }
                }

                entities.Dispose();
                return vbloods;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error getting VBlood prefabs: {ex.Message}");
                return new List<PrefabGUID>();
            }
        }

        /// <summary>
        /// Check if player already has a specific VBlood
        /// </summary>
        /// <summary>
        /// Check if player already has a specific VBlood
        /// </summary>
        private static bool HasVBlood(DynamicBuffer<VBloodConsumed> vbloodBuffer, PrefabGUID vblood)
        {
            try 
            {
                var prefabGuidField = typeof(VBloodConsumed).GetField("PrefabGUID", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (prefabGuidField == null)
                {
                    Plugin.Logger?.LogError("PrefabGUID field not found in VBloodConsumed");
                    return false;
                }
                
                foreach (var consumed in vbloodBuffer)
                {
                    try 
                    {
                        var prefabGuid = (PrefabGUID)prefabGuidField.GetValue(consumed);
                        if (prefabGuid.GuidHash == vblood.GuidHash)
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger?.LogError($"Error checking VBlood: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in HasVBlood: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Restore player's original VBloods when leaving arena
        /// </summary>
        public static void RestoreOriginalVBloods(Entity playerEntity)
        {
            try
            {
                var em = VRisingCore.EntityManager;
                if (!em.Exists(playerEntity) || !em.HasComponent<PlayerCharacter>(playerEntity))
                {
                    Plugin.Logger?.LogError("Cannot restore VBloods: Invalid player entity");
                    return;
                }

                // Get user entity and Steam ID
                var userEntity = em.GetComponentData<PlayerCharacter>(playerEntity).UserEntity;
                if (userEntity == Entity.Null)
                {
                    Plugin.Logger?.LogError("Cannot restore VBloods: User entity not found");
                    return;
                }

                var user = em.GetComponentData<User>(userEntity);
                ulong steamId = user.PlatformId;
                
                if (!_originalVBloods.TryGetValue(steamId, out var originalVBloods))
                {
                    Plugin.Logger?.LogWarning($"No original VBloods found for player {user.CharacterName}");
                    return;
                }

                // Clear current VBloods and restore originals
                if (em.HasBuffer<VBloodConsumed>(playerEntity))
                {
                    var vbloodBuffer = em.GetBuffer<VBloodConsumed>(playerEntity);
                    vbloodBuffer.Clear();
                    
                    // Add back original VBloods
                    var prefabGuidField = typeof(VBloodConsumed).GetField("PrefabGUID", 
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    foreach (var vblood in originalVBloods)
                    {
                        var consumed = new VBloodConsumed();
                        prefabGuidField?.SetValue(consumed, vblood);
                        vbloodBuffer.Add(consumed);
                    }
                    
                    Plugin.Logger?.LogInfo($"Restored {originalVBloods.Count} VBloods for player {user.CharacterName}");
                }
                
                // Remove from tracking after restoration
                _originalVBloods.Remove(steamId);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in RestoreOriginalVBloods: {ex.Message}");
                Plugin.Logger?.LogError(ex.StackTrace);
            }

            // Clean up any remaining references
            try
            {
                var em = VRisingCore.EntityManager;
                if (playerEntity != Entity.Null && em.Exists(playerEntity))
                {
                    var userEntity = em.GetComponentData<PlayerCharacter>(playerEntity).UserEntity;
                    if (userEntity != Entity.Null && em.Exists(userEntity))
                    {
                        var user = em.GetComponentData<User>(userEntity);
                        Plugin.Logger?.LogInfo($"VBlood restoration completed for player {user.CharacterName}");
                        return;
                    }
                }
                Plugin.Logger?.LogInfo("VBlood restoration completed");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error during VBlood restoration cleanup: {ex.Message}");
                Plugin.Logger?.LogError(ex.StackTrace);
            }
        }

        private static List<PrefabGUID> GetAllVBloodGUIDs()
        {
            return new List<PrefabGUID>
            {
                new PrefabGUID(-1905691330), new PrefabGUID(-1342764880), new PrefabGUID(1699865363),
                new PrefabGUID(-2025101517), new PrefabGUID(1362041468), new PrefabGUID(-1065970933),
                new PrefabGUID(435934037), new PrefabGUID(-1208888966), new PrefabGUID(1124739990),
                new PrefabGUID(2054432370), new PrefabGUID(-1449631170), new PrefabGUID(1106458752),
                new PrefabGUID(-1347412392), new PrefabGUID(1896428751), new PrefabGUID(-484556888),
                new PrefabGUID(2089106511), new PrefabGUID(-2137261854), new PrefabGUID(1233988687),
                new PrefabGUID(-1391546313), new PrefabGUID(-680831417), new PrefabGUID(114912615),
                new PrefabGUID(-1659822956)
            };
        }
    }
}
