using System;
using AutomationSystem.Data;
using AutomationSystem.Services;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace AutomationSystem.Core
{
    public delegate void ArenaEventHandler(object playerEntity);

    public interface IAutomationSystem
    {
        event ArenaEventHandler OnEnter;
        event ArenaEventHandler OnExit;

        void EnterArena(object playerEntity);
        void ExitArena(object playerEntity);
    }

    /// <summary>
    /// AutomationSystem for progression management
    /// </summary>
    public class AutomationSystem : IAutomationSystem
    {
        public event ArenaEventHandler OnEnter;
        public event ArenaEventHandler OnExit;

        private bool originalProgressionCaptured = false;

        public AutomationSystem()
        {
        }

        /// <summary>
        /// Fills player blood to maximum quality (100.0f)
        /// </summary>
        private bool FillBloodToMax(Entity characterEntity)
        {
            try
            {
                if (CrowbaneArenaCore.EntityManager.TryGetComponentData(characterEntity, out Blood blood))
                {
                    // Set blood quality to maximum (100%)
                    blood.Quality = 100.0f;

                    // Also ensure blood type is set to a valid type if not already
                    if (blood.BloodType.GuidHash == 0)
                        // Set to rogue blood as default for arena
                        blood.BloodType = BloodTypeGUIDs.GetBloodTypeGUID("rogue");

                    CrowbaneArenaCore.EntityManager.SetComponentData(characterEntity, blood);
                    Plugin.Logger?.LogInfo(
                        $"Set blood quality to {blood.Quality}% with type {blood.BloodType.GuidHash}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error filling blood to max: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     Clears arena items and destroys all equipment (does not restore original equipment)
        /// </summary>
        private bool ClearInventoryandunequipitems(Entity characterEntity)
        {
            try
            {
                // Clear inventory items
                if (!InventoryManagementService.ClearInventory(characterEntity))
                    Plugin.Logger?.LogWarning("Failed to clear inventory");

                // Destroy all equipped items
                if (CrowbaneArenaCore.EntityManager.TryGetComponentData(characterEntity, out Equipment equipment))
                {
                    var equippedItems = new NativeList<Entity>(Allocator.Temp);
                    equipment.GetAllEquipmentEntities(equippedItems);

                    var destroyedCount = 0;
                    foreach (var equippedItem in equippedItems)
                        if (equippedItem != Entity.Null)
                        {
                            CrowbaneArenaCore.EntityManager.DestroyEntity(equippedItem);
                            destroyedCount++;
                        }

                    equippedItems.Dispose();
                    Plugin.Logger?.LogInfo($"Destroyed {destroyedCount} equipped items");
                }

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error clearing arena items and destroying equipment: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     Renames player for arena mode
        /// </summary>
        private bool RenamePlayerForAutomation(Entity userEntity, Entity characterEntity)
        {
            try
            {
                if (CrowbaneArenaCore.EntityManager.TryGetComponentData(userEntity, out User user))
                {
                    var originalName = user.CharacterName.ToString();
                    var automationName = $"[Automation] {originalName}";

                    // Store original name in snapshot for restoration
                    // Note: This would need to be implemented in the snapshot system

                    user.CharacterName = automationName;
                    CrowbaneArenaCore.EntityManager.SetComponentData(userEntity, user);

                    Plugin.Logger?.LogInfo($"Renamed player from '{originalName}' to '{automationName}'");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error renaming player for automation: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     Restores player name from automation mode
        /// </summary>
        private bool RestorePlayerName(Entity userEntity)
        {
            try
            {
                if (CrowbaneArenaCore.EntityManager.TryGetComponentData(userEntity, out User user))
                {
                    var currentName = user.CharacterName.ToString();

                    // Remove [Automation] prefix if present
                    if (currentName.StartsWith("[Automation] "))
                    {
                        var originalName = currentName.Substring(12); // Remove "[Automation] " prefix
                        user.CharacterName = originalName;
                        CrowbaneArenaCore.EntityManager.SetComponentData(userEntity, user);

                        Plugin.Logger?.LogInfo($"Restored player name from '{currentName}' to '{originalName}'");
                        return true;
                    }

                    Plugin.Logger?.LogInfo("Player name doesn't have automation prefix, no restoration needed");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error restoring player name from automation: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Enters automation mode: captures progression, clears items, provides default loadout, unlocks all VBlood.
        /// </summary>
        public void EnterArena(object playerEntity)
        {
            try
            {
                // Extract platform ID and entities from player entity
                ulong platformId = 0;
                var userEntity = Entity.Null;
                var characterEntity = Entity.Null;

                if (playerEntity is Entity entity)
                {
                    if (CrowbaneArenaCore.EntityManager.TryGetComponentData(entity, out User user))
                    {
                        platformId = user.PlatformId;
                        userEntity = entity;

                        // Get character entity from user
                        characterEntity = user.LocalCharacter._Entity;
                    }
                }
                else if (playerEntity is ulong pid)
                {
                    platformId = pid;
                }

                if (platformId != 0 && userEntity != Entity.Null && characterEntity != Entity.Null)
                {
                    Plugin.Logger?.LogInfo($"Player {platformId} entering automation mode...");

                    // 1. Activate VBlood hook for this player
                    GameSystems.MarkPlayerEnteredAutomation(platformId);
                    Plugin.Logger?.LogInfo($"VBlood hook activated for player {platformId}");

                    // 2. Clear all existing items (inventory and equipment) and destroy equipment
                    if (ClearInventoryandunequipitems(characterEntity))
                        Plugin.Logger?.LogInfo("Player inventory and equipment cleared and destroyed");
                    else
                        Plugin.Logger?.LogWarning("Failed to clear player inventory and equipment");

                    // 3. Fill blood to maximum quality
                    if (FillBloodToMax(characterEntity))
                        Plugin.Logger?.LogInfo("Player blood filled to maximum quality");
                    else
                        Plugin.Logger?.LogWarning("Failed to fill player blood");

                    // 4. Rename player for automation
                    if (RenamePlayerForAutomation(userEntity, characterEntity))
                        Plugin.Logger?.LogInfo("Player renamed for automation");
                    else
                        Plugin.Logger?.LogWarning("Failed to rename player for automation");

                    // 5. Provide default automation loadout
                    if (InventoryManagementService.GiveLoadout(characterEntity, "default"))
                        Plugin.Logger?.LogInfo("Default automation loadout provided");
                    else
                        Plugin.Logger?.LogWarning("Failed to provide automation loadout");

                    // 5. Capture progression handled by SnapshotManagerService
                    originalProgressionCaptured = true;

                    Plugin.Logger?.LogInfo($"Player {platformId} entered automation mode successfully");
                }
                else
                {
                    Plugin.Logger?.LogError("Invalid player entity or missing components for automation entry");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error entering automation mode: {ex.Message}");
            }

            OnEnter?.Invoke(playerEntity);
        }

        /// <summary>
        /// Exits automation mode: restores progression, clears automation items, restores original items.
        /// </summary>
        public void ExitArena(object playerEntity)
        {
            try
            {
                if (originalProgressionCaptured)
                {
                    // Extract platform ID and entities from player entity
                    ulong platformId = 0;
                    var userEntity = Entity.Null;
                    var characterEntity = Entity.Null;

                    if (playerEntity is Entity entity)
                    {
                        if (CrowbaneArenaCore.EntityManager.TryGetComponentData(entity, out User user))
                        {
                            platformId = user.PlatformId;
                            userEntity = entity;

                            // Get character entity from user
                            characterEntity = user.LocalCharacter._Entity;
                        }
                    }
                    else if (playerEntity is ulong pid)
                    {
                        platformId = pid;
                    }

                    if (platformId != 0 && userEntity != Entity.Null && characterEntity != Entity.Null)
                    {
                        Plugin.Logger?.LogInfo($"Player {platformId} exiting automation mode...");

                        // 1. Deactivate VBlood hook for this player
                        GameSystems.MarkPlayerExitedAutomation(platformId);
                        Plugin.Logger?.LogInfo($"VBlood hook deactivated for player {platformId}");

                        // 2. Clear automation items and destroy equipment (do not restore original equipment)
                        if (ClearInventoryandunequipitems(characterEntity))
                            Plugin.Logger?.LogInfo("Automation items cleared and equipment destroyed");
                        else
                            Plugin.Logger?.LogWarning("Failed to clear automation items or destroy equipment");

                        // 3. Restore player name from automation
                        if (RestorePlayerName(userEntity))
                            Plugin.Logger?.LogInfo("Player name restored from automation");
                        else
                            Plugin.Logger?.LogWarning("Failed to restore player name from automation");

                        // 4. Restore original progression handled by SnapshotManagerService
                        originalProgressionCaptured = false;

                        Plugin.Logger?.LogInfo($"Player {platformId} exited automation mode successfully");
                    }
                    else
                    {
                        Plugin.Logger?.LogError("Invalid player entity or missing components for automation exit");
                    }
                }
                else
                {
                    Plugin.Logger?.LogWarning("ExitAutomationSystem called but no progression was captured");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error exiting automation mode: {ex.Message}");
            }

            OnExit?.Invoke(playerEntity);
        }
    }
}
