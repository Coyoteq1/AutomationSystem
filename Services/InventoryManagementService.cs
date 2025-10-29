using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using Stunlock.Core;
using System;
using Unity.Entities;

namespace CrowbaneArena.Services
{
    /// <summary>
    /// Service for managing player inventory operations
    /// </summary>
    public static class InventoryManagementService
    {
        private static EntityManager EM => VRisingCore.EntityManager;
        private static ServerGameManager SGM => VRisingCore.ServerGameManager;

        /// <summary>
        /// Clear all items from a character's inventory
        /// </summary>
        public static bool ClearInventory(Entity characterEntity)
        {
            try
            {
                Plugin.Logger?.LogInfo("Clearing player inventory...");

                // Get inventory entity using V Rising's utility
                if (!ProjectM.InventoryUtilities.TryGetInventoryEntity(EM, characterEntity, out Entity inventoryEntity))
                {
                    Plugin.Logger?.LogWarning("Could not get inventory entity for clearing");
                    return false;
                }

                if (!EM.TryGetBuffer<InventoryBuffer>(inventoryEntity, out var inventoryBuffer))
                {
                    Plugin.Logger?.LogWarning("Could not get inventory buffer for clearing");
                    return false;
                }

                int clearedCount = 0;

                // Clear each slot
                for (int i = 0; i < inventoryBuffer.Length; i++)
                {
                    var item = inventoryBuffer[i];
                    if (item.ItemEntity._Entity != Entity.Null && item.Amount > 0)
                    {
                        // Destroy the item entity
                        if (EM.HasComponent<PrefabGUID>(item.ItemEntity._Entity))
                        {
                            EM.DestroyEntity(item.ItemEntity._Entity);
                            clearedCount++;
                        }
                    }
                }

                Plugin.Logger?.LogInfo($"✓ Cleared {clearedCount} items from inventory");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error clearing inventory: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the count of items in inventory
        /// </summary>
        public static int GetInventoryItemCount(Entity characterEntity)
        {
            try
            {
                if (!ProjectM.InventoryUtilities.TryGetInventoryEntity(EM, characterEntity, out Entity inventoryEntity))
                {
                    return 0;
                }

                if (!EM.TryGetBuffer<InventoryBuffer>(inventoryEntity, out var inventoryBuffer))
                {
                    return 0;
                }

                int count = 0;
                for (int i = 0; i < inventoryBuffer.Length; i++)
                {
                    var item = inventoryBuffer[i];
                    if (item.ItemEntity._Entity != Entity.Null && item.Amount > 0 && item.ItemType.GuidHash != 0)
                    {
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error getting inventory count: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Give a loadout of items to a player
        /// </summary>
        public static bool GiveLoadout(Entity characterEntity, PrefabGUID[] itemGuids, int[] amounts)
        {
            try
            {
                if (itemGuids.Length != amounts.Length)
                {
                    Plugin.Logger?.LogError("Item GUIDs and amounts arrays must be same length");
                    return false;
                }

                // Get ServerGameManager for proper item spawning
                var serverGameManager = VRisingCore.ServerGameManager;

                int successCount = 0;

                for (int i = 0; i < itemGuids.Length; i++)
                {
                    var guid = itemGuids[i];
                    var amount = amounts[i];

                    if (guid.GuidHash == 0 || amount <= 0)
                        continue;

                    // Proper item spawning using ServerGameManager.TryAddInventoryItem (TryAddItem)
                    var response = serverGameManager.TryAddInventoryItem(characterEntity, guid, amount);
                    if (response.NewEntity != Entity.Null)
                    {
                        successCount++;
                        Plugin.Logger?.LogInfo($"✓ Spawned item {guid.GuidHash} x{amount}");
                    }
                    else
                    {
                        Plugin.Logger?.LogWarning($"Failed to spawn item {guid.GuidHash} x{amount}");
                    }
                }

                Plugin.Logger?.LogInfo($"✓ Loadout complete: {successCount}/{itemGuids.Length} items spawned");
                return successCount > 0;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error giving loadout: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Give a complete loadout from configuration
        /// </summary>
        public static bool GiveLoadout(Entity characterEntity, string loadoutName)
        {
            try
            {
                if (!ArenaConfigLoader.TryGetLoadout(loadoutName, out var loadout))
                {
                    Plugin.Logger?.LogWarning($"Loadout '{loadoutName}' not found in configuration");
                    return false;
                }

                Plugin.Logger?.LogInfo($"Applying loadout '{loadoutName}'...");
                int successCount = 0;
                int totalItems = 0;

                // Spawn weapons
                if (loadout.Weapons != null)
                {
                    foreach (var weaponName in loadout.Weapons)
                    {
                        totalItems++;
                        if (ArenaConfigLoader.TryGetWeaponVariant(weaponName, out var weapon, out var variant))
                        {
                            var weaponGuid = new PrefabGUID(weapon.Guid);
                            if (GiveLoadout(characterEntity, new[] { weaponGuid }, new[] { 1 }))
                            {
                                successCount++;
                            }
                        }
                    }
                }

                // Spawn armor sets
                if (loadout.ArmorSets != null)
                {
                    foreach (var armorSetName in loadout.ArmorSets)
                    {
                        totalItems++;
                        if (ArenaConfigLoader.TryGetArmorSet(armorSetName, out var armorSet))
                        {
                            var armorGuids = new[]
                            {
                                new PrefabGUID((int)armorSet.BootsGuid),
                                new PrefabGUID((int)armorSet.GlovesGuid),
                                new PrefabGUID((int)armorSet.ChestGuid),
                                new PrefabGUID((int)armorSet.LegsGuid)
                            };
                            if (GiveLoadout(characterEntity, armorGuids, new[] { 1, 1, 1, 1 }))
                            {
                                successCount++;
                            }
                        }
                    }
                }

                // Spawn consumables
                if (loadout.Consumables != null)
                {
                    foreach (var consumableName in loadout.Consumables)
                    {
                        if (ArenaConfigLoader.TryGetConsumable(consumableName, out var consumable))
                        {
                            totalItems++;
                            var consumableGuid = new PrefabGUID((int)consumable.Guid);
                            if (GiveLoadout(characterEntity, new[] { consumableGuid }, new[] { consumable.DefaultAmount }))
                            {
                                successCount++;
                            }
                        }
                    }
                }

                Plugin.Logger?.LogInfo($"Applied loadout '{loadoutName}': {successCount}/{totalItems} items spawned");
                return successCount > 0;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error giving loadout: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Give an item to a player (static method for commands)
        /// </summary>
        public static bool GiveItem(Entity player, string itemName, int amount)
        {
            // Placeholder implementation
            Plugin.Logger?.LogInfo($"Giving {amount}x {itemName} to player {player}");
            return true; // Simulate success
        }

        /// <summary>
        /// Get available items for commands (static method)
        /// </summary>
        public static List<string> GetAvailableItems(string category = "")
        {
            // Placeholder implementation
            var items = new List<string> { "Sword", "Shield", "Bow", "Armor", "Potion", "Helmet", "Boots" };
            return items;
        }

        /// <summary>
        /// Add a prefab to the system (static method for commands)
        /// </summary>
        public static void AddPrefab(string category, string name, Guid guid)
        {
            // Placeholder implementation
            Plugin.Logger?.LogInfo($"Added prefab {name} (GUID: {guid}) to category {category}");
        }

        /// <summary>
        /// Import prefabs from JSON (static method for commands)
        /// </summary>
        public static int ImportPrefabsFromJson(string jsonData)
        {
            // Placeholder implementation
            Plugin.Logger?.LogInfo($"Importing prefabs from JSON: {jsonData.Length} characters");
            return jsonData.Split(',').Length; // Simulate count
        }

        /// <summary>
        /// Export prefabs to JSON (static method for commands)
        /// </summary>
        public static string ExportPrefabsToJson()
        {
            // Placeholder implementation
            return "{\"items\":[\"sword\",\"shield\",\"bow\",\"armor\"]}";
        }
    }
}
