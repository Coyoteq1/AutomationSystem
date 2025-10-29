using ProjectM;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using ProjectM.Network;

namespace CrowbaneArena.Services;

/// <summary>
/// Provides methods for PvP arena player positioning.
/// </summary>
public static class SpawnService
{
    /// <summary>
    /// Teleports a player to the specified position in the PvP arena.
    /// </summary>
    /// <param name="playerEntity">The character entity of the player to teleport.</param>
    /// <param name="position">The position in 3D space where the player will be teleported.</param>
    public static void TeleportPlayerToArena(Entity playerEntity, float3 position)
    {
        // Teleport player to position using Translation component
        if (VRisingCore.EntityManager.HasComponent<Translation>(playerEntity))
        {
            var translation = VRisingCore.EntityManager.GetComponentData<Translation>(playerEntity);
            translation.Value = position;
            VRisingCore.EntityManager.SetComponentData(playerEntity, translation);

            VRisingCore.Log?.LogInfo($"Teleported player {playerEntity} to arena position: {position}");
        }
        else if (VRisingCore.EntityManager.HasComponent<LocalToWorld>(playerEntity))
        {
            // Fallback: try to modify LocalToWorld matrix directly
            var localToWorld = VRisingCore.EntityManager.GetComponentData<LocalToWorld>(playerEntity);
            var matrix = localToWorld.Value;
            matrix.c3 = new Unity.Mathematics.float4(position, 1);
            localToWorld = new LocalToWorld { Value = matrix };
            VRisingCore.EntityManager.SetComponentData(playerEntity, localToWorld);

            VRisingCore.Log?.LogInfo($"Teleported player {playerEntity} to arena position: {position}");
        }
    }



    /// <summary>
    /// Sets up a PvP match by teleporting players to spawn positions.
    /// </summary>
    /// <param name="players">List of player entities to teleport.</param>
    /// <param name="spawnPositions">List of positions to teleport players to.</param>
    public static void SetupPvPMatch(List<Entity> players, List<float3> spawnPositions)
    {
        if (players.Count != spawnPositions.Count)
        {
            VRisingCore.Log?.LogError("Player count doesn't match spawn position count");
            return;
        }

        for (int i = 0; i < players.Count; i++)
        {
            TeleportPlayerToArena(players[i], spawnPositions[i]);
        }

        VRisingCore.Log?.LogInfo($"Set up PvP match with {players.Count} players");
    }
}
