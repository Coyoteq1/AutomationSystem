using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Stunlock.Core;

namespace CrowbaneArena.Services
{
    public static class SnapshotManagerService
    {
        private const string SnapshotsFileName = "PlayerSnapshots.json";
        private static Dictionary<ulong, PlayerSnapshot> _snapshots = new();
        private static EntityManager EM => CrowbaneArenaCore.EntityManager;

        /// <summary>
        /// Initializes the snapshot manager service by loading existing snapshots from disk.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                LoadSnapshots();
                ValidateSnapshots();
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Critical error during SnapshotManagerService initialization: {ex.Message}");
                // Reset to empty state if loading fails completely
                _snapshots = new Dictionary<ulong, PlayerSnapshot>();
            }
        }

        private static void LoadSnapshots()
        {
            // For now, use a simple in-memory storage
            // In the future, this could be enhanced to use file persistence
            _snapshots = new Dictionary<ulong, PlayerSnapshot>();
        }

        /// <summary>
        /// Validates loaded snapshots and removes any invalid data to prevent entity corruption
        /// </summary>
        private static void ValidateSnapshots()
        {
            if (_snapshots == null || _snapshots.Count == 0)
            {
                Plugin.Logger?.LogInfo("Snapshot validation: No snapshots to validate");
                return;
            }

            int removedCount = 0;
            var keysToRemove = new List<ulong>();

            foreach (var kvp in _snapshots)
            {
                try
                {
                    // Basic validation - check for reasonable data
                    if (kvp.Value == null ||
                        string.IsNullOrEmpty(kvp.Value.OriginalName) ||
                        kvp.Value.InventoryItems == null ||
                        kvp.Value.EquippedItems == null ||
                        kvp.Value.AbilityGuids == null)
                    {
                        Plugin.Logger?.LogWarning($"[Snapshot Validation] Invalid snapshot data for player {kvp.Key} - removing");
                        keysToRemove.Add(kvp.Key);
                        removedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[Snapshot Validation] Error validating snapshot for player {kvp.Key}: {ex.Message}");
                    keysToRemove.Add(kvp.Key);
                    removedCount++;
                }
            }

            foreach (var key in keysToRemove)
            {
                _snapshots.Remove(key);
            }

            Plugin.Logger?.LogInfo($"Snapshot validation completed: {_snapshots.Count} valid snapshots retained");
        }

        /// <summary>
        /// Saves all player snapshots to disk.
        /// </summary>
        public static void SaveSnapshots()
        {
            // For now, use in-memory storage
            // In the future, this could be enhanced to use file persistence
        }

        /// <summary>
        /// Retrieves the number of player snapshots currently stored.
        /// </summary>
        /// <returns>The number of player snapshots.</returns>
        public static int GetSnapshotCount()
        {
            return _snapshots.Count;
        }

        public static void ClearAllSnapshots()
        {
            _snapshots.Clear();
            SaveSnapshots();
            Plugin.Logger?.LogInfo("All player snapshots cleared");
        }

        public static void DeleteSnapshot(ulong platformId)
        {
            if (_snapshots.ContainsKey(platformId))
            {
                _snapshots.Remove(platformId);
                SaveSnapshots();
                Plugin.Logger?.LogInfo($"Player snapshot for {platformId} deleted.");
            }
            else
            {
                Plugin.Logger?.LogInfo($"No player snapshot found for {platformId} to delete.");
            }
        }

        /// <summary>
        /// Checks if a player is currently in the arena.
        /// </summary>
        /// <param name="platformId">The player's platform ID</param>
        /// <returns>True if the player is in the arena, false otherwise</returns>
        public static bool IsInArena(ulong platformId)
        {
            return _snapshots.TryGetValue(platformId, out var snapshot) && snapshot.IsInArena;
        }

        /// <summary>
        /// Enters a player into the arena, creating a snapshot of their current state.
        /// </summary>
        /// <param name="userEntity">The user entity</param>
        /// <param name="characterEntity">The character entity</param>
        /// <param name="arenaLocation">The spawn location in the arena</param>
        /// <param name="loadoutName">The loadout to apply (default: "default")</param>
        /// <param name="zoneName">The arena zone name (optional)</param>
        /// <returns>True if the player successfully entered the arena, false otherwise</returns>
        public static bool EnterArena(Entity userEntity, Entity characterEntity, float3 arenaLocation, string loadoutName = "default", string zoneName = null)
        {
            try
            {
                if (userEntity == Entity.Null || characterEntity == Entity.Null)
                {
                    Plugin.Logger?.LogError("EnterArena: Null entity provided");
                    return false;
                }

                // Basic entity validation
                try
                {
                    if (!EM.HasComponent<PlayerCharacter>(characterEntity))
                    {
                        Plugin.Logger?.LogError("EnterArena: characterEntity is not a valid player character or entity is corrupted.");
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("EnterArena: Entity validation failed for PlayerCharacter - entity may be from different world");
                    return false;
                }

                // Safe get User component
                if (!EM.TryGetComponentData(userEntity, out User user))
                {
                    Plugin.Logger?.LogError("EnterArena: Failed to get User component from userEntity - entity may be corrupted");
                    return false;
                }

                if (IsInArena(user.PlatformId))
                {
                    Plugin.Logger?.LogInfo($"Player {user.CharacterName} (ID: {user.PlatformId}) already in arena, skipping entry");
                    return true;
                }

                // Safe check for Dead and Disabled components
                try
                {
                    if (EM.HasComponent<Dead>(characterEntity) || EM.HasComponent<Disabled>(characterEntity))
                    {
                        Plugin.Logger?.LogWarning($"Player {user.CharacterName} cannot enter arena, character is dead or disabled.");
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("EnterArena: Entity validation failed for Dead/Disabled - entity may be from different world");
                    return false;
                }

                // Safe check for LocalToWorld component
                try
                {
                    if (!EM.HasComponent<LocalToWorld>(characterEntity))
                    {
                        Plugin.Logger?.LogError("EnterArena: Character entity missing LocalToWorld component - cannot teleport");
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("EnterArena: Entity validation failed for LocalToWorld - entity may be from different world");
                    return false;
                }

                Plugin.Logger?.LogInfo("Capturing snapshot OUTSIDE arena (original loadout and progression)...");
                var snapshot = CreateSnapshot(characterEntity);
                snapshot.IsInArena = true;
                snapshot.LoadoutName = loadoutName;
                _snapshots[user.PlatformId] = snapshot;
                SaveSnapshots();

                if (!string.IsNullOrEmpty(zoneName))
                {
                    // TODO: Implement zone tracking if needed
                }

                Plugin.Logger?.LogInfo("✓ Snapshot captured outside arena");

                Plugin.Logger?.LogInfo($"Teleporting player to arena location: {arenaLocation}");
                // Try the simple approach - modify Translation component directly (LocalToWorld.Position is read-only)
                try
                {
                    if (EM.TryGetComponentData(characterEntity, out Translation translation))
                    {
                        translation.Value = arenaLocation;
                        EM.SetComponentData(characterEntity, translation);
                        Plugin.Logger?.LogInfo("Successfully teleported player to arena location");
                    }
                    else
                    {
                        Plugin.Logger?.LogError("Failed to teleport: No Translation component found");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"Failed to teleport player to arena: {ex.Message}");
                    return false;
                }

                Plugin.Logger?.LogInfo("✓ Teleported to arena zone, now applying arena buffs...");

                PrepareCharacterForArena(userEntity, characterEntity, loadoutName);

                Plugin.Logger?.LogInfo($"Player {user.CharacterName} entered arena successfully");

                return true;
            }
            catch (System.Exception ex)
            {
                Plugin.Logger?.LogError($"Critical error in EnterArena: {ex.Message}");
                Plugin.Logger?.LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Exits a player from the arena, restoring their snapshot state.
        /// </summary>
        /// <param name="userEntity">The user entity</param>
        /// <param name="characterEntity">The character entity</param>
        /// <returns>True if the player successfully exited the arena, false otherwise</returns>
        public static bool ExitArena(Entity userEntity, Entity characterEntity)
        {
            try
            {
                if (userEntity == Entity.Null || characterEntity == Entity.Null)
                {
                    Plugin.Logger?.LogError("ExitArena: Null entity provided");
                    return false;
                }

                try
                {
                    if (!EM.HasComponent<PlayerCharacter>(characterEntity))
                    {
                        Plugin.Logger?.LogError("ExitArena: characterEntity is not a valid player character.");
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("ExitArena: Entity validation failed for PlayerCharacter - entity may be from different world");
                    return false;
                }

                try
                {
                    if (EM.HasComponent<Dead>(characterEntity) || EM.HasComponent<Disabled>(characterEntity))
                    {
                        Plugin.Logger?.LogWarning("ExitArena: Character is dead or disabled");
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("ExitArena: Entity validation failed for Dead/Disabled - entity may be from different world");
                    return false;
                }

                if (!EM.TryGetComponentData(userEntity, out User user))
                {
                    Plugin.Logger?.LogError("ExitArena: Failed to get User component from userEntity");
                    return false;
                }

                if (!_snapshots.TryGetValue(user.PlatformId, out var snapshot))
                {
                    Plugin.Logger?.LogError($"ExitArena: No snapshot found for player {user.PlatformId} - player may not be in arena");
                    return false;
                }

                if (!snapshot.IsInArena)
                {
                    Plugin.Logger?.LogWarning($"ExitArena: Player {user.PlatformId} snapshot indicates not in arena - skipping exit");
                    return false;
                }

                ClearCharacter(characterEntity);
                RestoreSnapshot(characterEntity, snapshot);

                snapshot.IsInArena = false;
                SaveSnapshots();

                try
                {
                    if (!EM.HasComponent<LocalToWorld>(characterEntity))
                    {
                        Plugin.Logger?.LogError("ExitArena: Character entity missing LocalToWorld component - cannot teleport back");
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("ExitArena: Entity validation failed for LocalToWorld - entity may be from different world");
                    return false;
                }

                // Teleport back to original location using Translation component (LocalToWorld.Position is read-only)
                try
                {
                    if (EM.TryGetComponentData(characterEntity, out Translation translation))
                    {
                        translation.Value = snapshot.OriginalLocation;
                        EM.SetComponentData(characterEntity, translation);
                        Plugin.Logger?.LogInfo($"Successfully teleported player back to: {snapshot.OriginalLocation}");
                    }
                    else
                    {
                        Plugin.Logger?.LogError("Failed to teleport: No Translation component found");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"Failed to teleport player back: {ex.Message}");
                    return false;
                }

                Plugin.Logger?.LogInfo($"Player {user.CharacterName} exited arena and progress restored");

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Critical error in ExitArena: {ex.Message}");
                Plugin.Logger?.LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private static void RestoreSnapshot(Entity characterEntity, PlayerSnapshot snapshot)
        {
            if (characterEntity == Entity.Null)
            {
                Plugin.Logger?.LogError("RestoreSnapshot: characterEntity is null - cannot restore snapshot");
                return;
            }

            try
            {
                if (!EM.HasComponent<PlayerCharacter>(characterEntity))
                {
                    Plugin.Logger?.LogError("RestoreSnapshot: characterEntity missing PlayerCharacter component - not a valid player character");
                    return;
                }
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("RestoreSnapshot: Entity validation failed for PlayerCharacter - entity may be from different world");
                return;
            }

            if (snapshot == null)
            {
                Plugin.Logger?.LogError("RestoreSnapshot: snapshot is null - cannot restore");
                return;
            }

            Plugin.Logger?.LogInfo($"Starting snapshot restoration. Snapshot has {snapshot.InventoryItems.Count} items");

            try
            {
                if (!string.IsNullOrEmpty(snapshot.OriginalName) && EM.TryGetComponentData(characterEntity, out PlayerCharacter playerCharacter))
                {
                    playerCharacter.Name = new Unity.Collections.FixedString64Bytes(snapshot.OriginalName);
                    EM.SetComponentData(characterEntity, playerCharacter);
                    Plugin.Logger?.LogInfo($"Restored original name: {snapshot.OriginalName}");
                }
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("RestoreSnapshot: Entity validation failed while restoring name - entity may be from different world");
            }

            try
            {
                if (EM.TryGetComponentData(characterEntity, out Health health))
                {
                    health.Value = snapshot.Health;
                    EM.SetComponentData(characterEntity, health);
                }
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("RestoreSnapshot: Entity validation failed while restoring health - entity may be from different world");
            }

            try
            {
                if (EM.TryGetComponentData(characterEntity, out Blood blood))
                {
                    blood.BloodType = new PrefabGUID(snapshot.BloodTypeGuid);
                    blood.Quality = snapshot.BloodQuality;
                    EM.SetComponentData(characterEntity, blood);
                }
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("RestoreSnapshot: Entity validation failed while restoring blood - entity may be from different world");
            }

            try
            {
                // Clear current inventory and equipment
                InventoryManagementService.ClearInventory(characterEntity);

                // Restore original inventory
                RestoreInventory(characterEntity, snapshot);

                // Restore equipment
                RestoreEquipment(characterEntity, snapshot);

                // Restore abilities
                RestoreAbilities(characterEntity, snapshot);

                // Restore progression
                RestoreProgression(characterEntity, snapshot);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error during snapshot restoration: {ex.Message}");
            }
        }

        private static void ClearCharacter(Entity characterEntity)
        {
            // Enhanced entity validation
            if (characterEntity == Entity.Null)
            {
                Plugin.Logger?.LogError("ClearCharacter: characterEntity is null - cannot clear character");
                return;
            }

            // Additional validation - check for required components
            try
            {
                if (!EM.HasComponent<PlayerCharacter>(characterEntity))
                {
                    Plugin.Logger?.LogError("ClearCharacter: characterEntity missing PlayerCharacter component - not a valid player character");
                    return;
                }
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("ClearCharacter: Entity validation failed for PlayerCharacter - entity may be from different world");
                return;
            }

            // CRITICAL: Clear inventory before arena using enhanced service
            try
            {
                Plugin.Logger?.LogInfo("Clearing player inventory for arena...");
                InventoryManagementService.ClearInventory(characterEntity);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error during character clearing: {ex.Message}");
            }
        }

        private static PlayerSnapshot CreateSnapshot(Entity characterEntity)
        {
            try
            {
                if (characterEntity == Entity.Null)
                {
                    Plugin.Logger?.LogError("CreateSnapshot: characterEntity is null.");
                    return new PlayerSnapshot();
                }

                try
                {
                    if (!EM.HasComponent<PlayerCharacter>(characterEntity))
                    {
                        Plugin.Logger?.LogError("CreateSnapshot: characterEntity missing PlayerCharacter component.");
                        return new PlayerSnapshot();
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("CreateSnapshot: Entity validation failed for PlayerCharacter - entity may be from different world");
                    return new PlayerSnapshot();
                }

                Plugin.Logger?.LogInfo("--- Starting Enhanced Snapshot Creation ---");
                var snapshot = new PlayerSnapshot();

                // Get user entity for progression data
                Entity userEntity = Entity.Null;
                try
                {
                    if (EM.TryGetComponentData<PlayerCharacter>(characterEntity, out var playerChar))
                    {
                        userEntity = playerChar.UserEntity;
                        Plugin.Logger?.LogInfo($"Found user entity: {userEntity.Index}");
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("CreateSnapshot: Entity validation failed while getting PlayerCharacter - entity may be from different world");
                }

                CaptureCharacterStats(characterEntity, snapshot);
                CaptureInventory(characterEntity, snapshot);
                CaptureEquipment(characterEntity, snapshot);
                CaptureAbilities(characterEntity, snapshot);
                CaptureProgression(characterEntity, snapshot);

                Plugin.Logger?.LogInfo("--- Finished Enhanced Snapshot Creation ---");
                return snapshot;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Critical error in CreateSnapshot: {ex.Message}");
                Plugin.Logger?.LogError($"Stack trace: {ex.StackTrace}");
                return new PlayerSnapshot();
            }
        }

        private static void CaptureCharacterStats(Entity characterEntity, PlayerSnapshot snapshot)
        {
            try
            {
                if (EM.TryGetComponentData<LocalToWorld>(characterEntity, out var ltw))
                {
                    snapshot.OriginalLocation = ltw.Position;
                }
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("CaptureCharacterStats: Entity validation failed for LocalToWorld - entity may be from different world");
            }

            try
            {
                if (EM.TryGetComponentData(characterEntity, out PlayerCharacter playerCharacter))
                {
                    snapshot.OriginalName = playerCharacter.Name.ToString();
                    Plugin.Logger?.LogInfo($"Captured original name: {snapshot.OriginalName}");
                }
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("CaptureCharacterStats: Entity validation failed for PlayerCharacter - entity may be from different world");
            }

            try
            {
                if (EM.TryGetComponentData(characterEntity, out Health health)) snapshot.Health = health.Value;
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("CaptureCharacterStats: Entity validation failed for Health - entity may be from different world");
            }

            try
            {
                if (EM.TryGetComponentData(characterEntity, out Blood blood))
                {
                    snapshot.BloodTypeGuid = blood.BloodType.GuidHash;
                    snapshot.BloodQuality = blood.Quality;
                }
            }
            catch (ArgumentException)
            {
                Plugin.Logger?.LogError("CaptureCharacterStats: Entity validation failed for Blood - entity may be from different world");
            }

            // Note: Experience and Level components may not exist in current VRising version.
            snapshot.Experience = 0;
            snapshot.Level = 1;
            Plugin.Logger?.LogWarning("Experience and Level capture not available - using default values.");
        }

        private static void CaptureInventory(Entity characterEntity, PlayerSnapshot snapshot)
        {
            Plugin.Logger?.LogInfo("--- Capturing Inventory ---");
            try
            {
                // Use V Rising's built-in utility to get inventory entity
                if (ProjectM.InventoryUtilities.TryGetInventoryEntity(EM, characterEntity, out Entity inventoryEntity))
                {
                    Plugin.Logger?.LogInfo($"Found inventory entity: {inventoryEntity.Index}");

                    if (EM.TryGetBuffer<InventoryBuffer>(inventoryEntity, out var inventory))
                    {
                        int capturedItems = 0;
                        Plugin.Logger?.LogInfo($"Found InventoryBuffer with {inventory.Length} slots");

                        for (int i = 0; i < inventory.Length; i++)
                        {
                            var item = inventory[i];
                            if (item.ItemEntity._Entity != Entity.Null && item.Amount > 0 && item.ItemType.GuidHash != 0)
                            {
                                snapshot.InventoryItems[i] = new ItemData { ItemGuidHash = item.ItemType.GuidHash, Amount = item.Amount };
                                capturedItems++;
                                Plugin.Logger?.LogInfo($"Slot {i}: {item.ItemType.GuidHash} x{item.Amount}");
                            }
                        }
                        Plugin.Logger?.LogInfo($"✓ Captured {capturedItems} inventory items");
                    }
                    else
                    {
                        Plugin.Logger?.LogWarning("Could not get InventoryBuffer from inventory entity");
                    }
                }
                else
                {
                    Plugin.Logger?.LogWarning("Could not get inventory entity from character");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error capturing inventory: {ex.Message}");
                Plugin.Logger?.LogError($"Stack: {ex.StackTrace}");
            }
        }

        private static void CaptureEquipment(Entity characterEntity, PlayerSnapshot snapshot)
        {
            Plugin.Logger?.LogInfo("--- Capturing Equipment ---");
            try
            {
                if (!EM.TryGetComponentData<Equipment>(characterEntity, out var equipment))
                {
                    Plugin.Logger?.LogWarning("No Equipment component found");
                    return;
                }

                // Use GetAllEquipmentEntities from V Rising's equipment system
                var equippedItems = new Unity.Collections.NativeList<Entity>(Unity.Collections.Allocator.Temp);
                equipment.GetAllEquipmentEntities(equippedItems);

                int capturedItems = 0;
                foreach (var equippedItem in equippedItems)
                {
                    if (equippedItem == Entity.Null) continue;

                    // Get item PrefabGUID
                    if (EM.TryGetComponentData<PrefabGUID>(equippedItem, out var prefabGuid))
                    {
                        // Store equipment item
                        snapshot.EquippedItems.Add(new ItemData
                        {
                            ItemGuidHash = prefabGuid.GuidHash,
                            Amount = 1  // Equipment items are always 1
                        });
                        capturedItems++;
                        Plugin.Logger?.LogInfo($"Captured equipped item: {prefabGuid.GuidHash}");
                    }
                }

                equippedItems.Dispose();
                Plugin.Logger?.LogInfo($"✓ Captured {capturedItems} equipped items");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error capturing equipment: {ex.Message}");
            }
        }

        private static void CaptureAbilities(Entity characterEntity, PlayerSnapshot snapshot)
        {
            Plugin.Logger?.LogInfo("--- Capturing Abilities ---");
            try
            {
                // Use ProgressionCaptureService if available
                snapshot.AbilityGuids = ProgressionCaptureService.CaptureAbilities(characterEntity);
                Plugin.Logger?.LogInfo($"✓ Captured {snapshot.AbilityGuids.Count} abilities");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error capturing abilities: {ex.Message}");
            }
        }

        private static void RestoreInventory(Entity characterEntity, PlayerSnapshot snapshot)
        {
            Plugin.Logger?.LogInfo($"--- Restoring Inventory ({snapshot.InventoryItems.Count} items) ---");
            try
            {
                // Use ServerGameManager for proper item restoration (TryAddItem)
                var serverGameManager = VRisingCore.ServerGameManager;

                foreach (var kvp in snapshot.InventoryItems)
                {
                    var itemData = kvp.Value;
                    if (itemData.Amount > 0)
                    {
                        // Restore original inventory using ServerGameManager.TryAddInventoryItem
                        var itemGuid = new PrefabGUID(itemData.ItemGuidHash);
                        var response = serverGameManager.TryAddInventoryItem(characterEntity, itemGuid, itemData.Amount);
                        if (response.NewEntity != Entity.Null)
                        {
                            Plugin.Logger?.LogInfo($"✓ Restored inventory item: {itemData.ItemGuidHash} x{itemData.Amount}");
                        }
                        else
                        {
                            Plugin.Logger?.LogWarning($"Failed to restore inventory item: {itemData.ItemGuidHash} x{itemData.Amount}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in RestoreInventory: {ex.Message}");
                Plugin.Logger?.LogError($"Stack: {ex.StackTrace}");
            }
        }

        private static void RestoreEquipment(Entity characterEntity, PlayerSnapshot snapshot)
        {
            Plugin.Logger?.LogInfo($"--- Restoring Equipment ({snapshot.EquippedItems.Count} items) ---");
            try
            {
                // Use ServerGameManager for proper equipment restoration as inventory items (TryAddItem)
                var serverGameManager = VRisingCore.ServerGameManager;

                foreach (var itemData in snapshot.EquippedItems)
                {
                    if (itemData.Amount > 0)
                    {
                        // Restore equipment as inventory items (they will be equipped by the player)
                        var itemGuid = new PrefabGUID(itemData.ItemGuidHash);
                        var response = serverGameManager.TryAddInventoryItem(characterEntity, itemGuid, itemData.Amount);
                        if (response.NewEntity != Entity.Null)
                        {
                            Plugin.Logger?.LogInfo($"✓ Restored equipment item: {itemData.ItemGuidHash}");
                        }
                        else
                        {
                            Plugin.Logger?.LogWarning($"Failed to restore equipment item: {itemData.ItemGuidHash}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in RestoreEquipment: {ex.Message}");
            }
        }

        private static void RestoreAbilities(Entity characterEntity, PlayerSnapshot snapshot)
        {
            Plugin.Logger?.LogInfo($"--- Restoring Abilities ({snapshot.AbilityGuids.Count} abilities) ---");
            try
            {
                // Use ProgressionCaptureService if available
                ProgressionCaptureService.RestoreAbilities(characterEntity, snapshot.AbilityGuids);
                Plugin.Logger?.LogInfo($"✓ Restored {snapshot.AbilityGuids.Count} abilities");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error restoring abilities: {ex.Message}");
            }
        }

        private static void CaptureProgression(Entity characterEntity, PlayerSnapshot snapshot)
        {
            Plugin.Logger?.LogInfo("--- Capturing Progression (Abilities, Equipment) ---");

            // CRITICAL: Capture Ability progression BEFORE arena
            // Players get abilities restored INSIDE arena based on their loadout
            // On EXIT, we RESTORE to their original abilities

            snapshot.AbilityGuids = ProgressionCaptureService.CaptureAbilities(characterEntity);
            Plugin.Logger?.LogInfo($"✓ Captured {snapshot.AbilityGuids.Count} abilities (will restore on exit)");

            Plugin.Logger?.LogInfo("✓ Progression capture completed");
        }

        private static void RestoreProgression(Entity characterEntity, PlayerSnapshot snapshot)
        {
            Plugin.Logger?.LogInfo("--- Restoring Progression (Abilities) ---");

            // CRITICAL: RESTORE player's original abilities
            // Any abilities granted INSIDE arena are CLEARED and original ones restored

            ProgressionCaptureService.RestoreAbilities(characterEntity, snapshot.AbilityGuids);
            Plugin.Logger?.LogInfo($"✓ Restored {snapshot.AbilityGuids.Count} original abilities");

            Plugin.Logger?.LogInfo("✓ Progression restoration completed");
        }

        /// <summary>
        /// A unified method to strip a character and prepare them for an arena match.
        /// </summary>
        private static void PrepareCharacterForArena(Entity userEntity, Entity characterEntity, string loadoutName)
        {
            try
            {
                // Safe get User component for logging
                string characterName = "Unknown";
                try
                {
                    if (EM.TryGetComponentData(userEntity, out User user))
                    {
                        characterName = user.CharacterName.ToString();
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("PrepareCharacterForArena: Failed to get User component - entity may be invalid");
                    return;
                }

                Plugin.Logger?.LogInfo($"--- Preparing {characterName} for Arena ---");

                // 1. Clear everything
                InventoryManagementService.ClearInventory(characterEntity);

                // 2. Set Arena State
                // Apply basic arena setup (simplified version)
                try
                {
                    if (EM.TryGetComponentData(characterEntity, out Health health))
                    {
                        health.Value = health.MaxHealth._Value; // Full heal
                        EM.SetComponentData(characterEntity, health);
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("PrepareCharacterForArena: Failed to restore health - entity may be invalid");
                }

                // 3. Update Character Name
                try
                {
                    if (EM.TryGetComponentData<PlayerCharacter>(characterEntity, out var playerChar))
                    {
                        var arenaName = $"[Arena] {characterName}";
                        playerChar.Name = new Unity.Collections.FixedString64Bytes(arenaName);
                        EM.SetComponentData(characterEntity, playerChar);
                        Plugin.Logger?.LogInfo($"Changed player name to: {arenaName}");
                    }
                }
                catch (ArgumentException)
                {
                    Plugin.Logger?.LogError("PrepareCharacterForArena: Failed to update character name - entity may be invalid");
                }
                catch (Exception nameEx)
                {
                    Plugin.Logger?.LogWarning($"Failed to change arena name: {nameEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Critical error in PrepareCharacterForArena: {ex.Message}");
                Plugin.Logger?.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
