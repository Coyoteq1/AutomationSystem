using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using CrowbaneArena.Services;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Physics;
using ProjectM.Gameplay.Scripting;
using ProjectM.Scripting;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Collections.Generic;
using System.Linq;

// Note: Some patches have been commented out due to compilation errors
// These will need to be reimplemented once the correct system types are identified

namespace CrowbaneArena.Patches
{
    /// <summary>
    /// Handles buffs applied to players in the arena
    /// </summary>
    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    public static class ArenaBuffSystemPatch
    {
        /// <summary>
        /// Postfix for handling buffs in arena
        /// </summary>
        public static void Postfix(BuffSystem_Spawn_Server __instance)
        {
            try
            {
                var entityManager = __instance.EntityManager;
                
                // Get all buff entities that were just spawned
                var buffEntities = __instance.__query_401358634_0.ToEntityArray(Allocator.Temp);

                foreach (var buffEntity in buffEntities)
                {
                    if (!entityManager.Exists(buffEntity)) continue;
                    if (!entityManager.HasComponent<EntityOwner>(buffEntity)) continue;
                    
                    var owner = entityManager.GetComponentData<EntityOwner>(buffEntity).Owner;
                    if (!entityManager.Exists(owner) || !entityManager.HasComponent<PlayerCharacter>(owner)) continue;
                    
                    var buffPrefab = entityManager.GetComponentData<PrefabGUID>(buffEntity);
                    
                    // Example: Prevent specific buffs in arena
                    // if (buffPrefab == Prefabs.Buff_InCombat_PvPVampire && ArenaService.IsPlayerInArena(owner))
                    // {
                    //     entityManager.DestroyEntity(buffEntity);
                    // }
                }
                
                buffEntities.Dispose();
            }
            catch (System.Exception e)
            {
                Plugin.Logger?.LogError($"Error in ArenaBuffSystemPatch: {e}");
            }
        }
    }

    /// <summary>
    /// Handles ability usage in the arena
    /// </summary>
    [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
    public static class ArenaAbilityPatch
    {
        /// <summary>
        /// Postfix for handling abilities in arena
        /// </summary>
        public static void Postfix(AbilityRunScriptsSystem __instance)
        {
            try
            {
                var entityManager = __instance.EntityManager;
                
                // Get all ability cast events
                var entities = __instance._OnCastStartedQuery.ToEntityArray(Allocator.Temp);

                foreach (var entity in entities)
                {
                    if (!entityManager.Exists(entity) || !entityManager.HasComponent<EntityOwner>(entity)) continue;
                    
                    var owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!entityManager.Exists(owner) || !entityManager.HasComponent<PlayerCharacter>(owner)) continue;
                    
                    if (!entityManager.HasComponent<AbilityCastStartedEvent>(entity)) continue;
                    var abilityEvent = entityManager.GetComponentData<AbilityCastStartedEvent>(entity);
                    
                    if (!entityManager.Exists(abilityEvent.Ability) || !entityManager.HasComponent<PrefabGUID>(abilityEvent.Ability)) continue;
                    var abilityPrefab = entityManager.GetComponentData<PrefabGUID>(abilityEvent.Ability);
                    
                    // Example: Restrict specific abilities in arena
                    // if (ArenaService.IsRestrictedAbility(abilityPrefab) && ArenaService.IsPlayerInArena(owner))
                    // {
                    //     // Cancel the ability
                    //     entityManager.DestroyEntity(entity);
                    //     
                    //     // Notify player
                    //     // ServerChatUtils.SendSystemMessageToClient(entityManager, owner, "This ability is restricted in the arena!");
                    // }
                }
                
                entities.Dispose();
            }
            catch (System.Exception e)
            {
                Plugin.Logger?.LogError($"Error in ArenaAbilityPatch: {e}");
            }
        }
    }

    /// <summary>
    /// Handles player damage in the arena using the StatChangeSystem
    /// </summary>
    [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
    public static class ArenaDamagePatch
    {
        /// <summary>
        /// Handles damage-related events in the arena
        /// </summary>
        public static void Postfix(StatChangeSystem __instance)
        {
            try
            {
                // This system would handle damage modifications in the arena
                // Implementation depends on specific arena damage rules
                
                // Example: Check for players with health changes
                // var query = __instance.EntityManager.CreateEntityQuery(
                //     ComponentType.ReadOnly<PlayerCharacter>(),
                //     ComponentType.ReadOnly<Health>());
                //     
                // using (var players = query.ToEntityArray(Allocator.Temp))
                // {
                //     foreach (var player in players)
                //     {
                //         if (ArenaService.IsPlayerInArena(player))
                //         {
                //             // Handle arena-specific damage rules
                //         }
                //     }
                // }
            }
            catch (System.Exception e)
            {
                Plugin.Logger?.LogError($"Error in ArenaDamagePatch: {e}");
            }
        }
    }

    // Death system patch removed due to compilation errors
    // Will need to be reimplemented once the correct system type is identified

    // Movement system patch removed due to compilation errors
    // Will need to be reimplemented once the correct system type is identified
}
