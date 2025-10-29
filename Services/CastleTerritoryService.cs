using System.Collections.Generic;
using ProjectM;
using ProjectM.CastleBuilding;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using CrowbaneArena.Services;

namespace CrowbaneArena.Services;

/// <summary>
/// Service for managing castle territory-related (plot) operations and queries.
/// Supports creating new territories for arena battles.
/// </summary>
public class CastleTerritoryService
{
    const float BLOCK_SIZE = 10;
    static Dictionary<int2, int> blockCoordToTerritoryIndex = new();
    static Dictionary<int2, Entity> blockCoordToTerritory = new();

    /// <summary>
    /// Initializes a new instance of the CastleTerritoryService class.
    /// Loads and maps all castle territories and their blocks.
    /// </summary>
    public CastleTerritoryService()
    {
        var query = VRisingCore.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<CastleTerritory>());
        var entities = query.ToEntityArray(Allocator.Temp);
        foreach (var castleTerritory in entities)
        {
            var castleTerritoryData = VRisingCore.EntityManager.GetComponentData<CastleTerritory>(castleTerritory);
            var castleTerritoryIndex = castleTerritoryData.CastleTerritoryIndex;
            var ctb = VRisingCore.EntityManager.GetBuffer<CastleTerritoryBlocks>(castleTerritory);
            for (int i = 0; i < ctb.Length; i++)
            {
                blockCoordToTerritoryIndex[ctb[i].BlockCoordinate] = castleTerritoryIndex;
                blockCoordToTerritory[ctb[i].BlockCoordinate] = castleTerritory;
            }
        }
        entities.Dispose();
    }

    /// <summary>
    /// Attempts to get the castle territory entity for a given entity based on its position.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <param name="territoryEntity">The output territory entity if found.</param>
    /// <returns>True if a territory was found for the entity, false otherwise.</returns>
    public static bool TryGetCastleTerritory(Entity entity, out Entity territoryEntity)
    {
        if (entity.Has<Translation>())
        {
            var position = VRisingCore.EntityManager.GetComponentData<Translation>(entity).Value;
            var blockCoord = ConvertPosToBlockCoord(position);
            if (blockCoordToTerritory.TryGetValue(blockCoord, out territoryEntity))
            {
                return true;
            }
        }

        territoryEntity = Entity.Null;
        return false;
    }

    /// <summary>
    /// Gets the territory index for a given position.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    /// <returns>The territory index if found, -1 otherwise.</returns>
    public static int GetTerritoryIndex(float3 pos)
    {
        var blockCoord = ConvertPosToBlockCoord(pos);
        if (blockCoordToTerritoryIndex.TryGetValue(blockCoord, out var index))
            return index;
        return -1;
    }

    /// <summary>
    /// Gets the castle heart entity for a given territory index.
    /// </summary>
    /// <param name="territoryIndex">The territory index to search for.</param>
    /// <returns>The castle heart entity if found, Entity.Null otherwise.</returns>
    public static Entity GetHeartForTerritory(int territoryIndex)
    {
        if (territoryIndex == -1)
            return Entity.Null;
        var query = VRisingCore.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<CastleHeart>());
        var castleHearts = query.ToEntityArray(Allocator.Temp);
        foreach (var heart in castleHearts)
        {
            var heartData = VRisingCore.EntityManager.GetComponentData<CastleHeart>(heart);
            var castleTerritoryEntity = heartData.CastleTerritoryEntity;
            if (castleTerritoryEntity.Equals(Entity.Null))
                continue;
            var heartTerritoryIndex = VRisingCore.EntityManager.GetComponentData<CastleTerritory>(castleTerritoryEntity).CastleTerritoryIndex;
            if (heartTerritoryIndex == territoryIndex)
                return heart;
        }
        castleHearts.Dispose();
        return Entity.Null;
    }

    /// <summary>
    /// Creates a new castle territory at the specified position for arena battles.
    /// </summary>
    /// <param name="centerPosition">The center position of the new territory.</param>
    /// <param name="territorySize">The size of the territory in blocks (radius).</param>
    /// <param name="newTerritoryIndex">The territory index to assign (auto-generated if -1).</param>
    /// <returns>The created castle territory entity.</returns>
    public static Entity CreateCastleTerritory(float3 centerPosition, int territorySize = 5, int newTerritoryIndex = -1)
    {
        try
        {
            // Generate new territory index if not provided
            if (newTerritoryIndex == -1)
            {
                newTerritoryIndex = GetNextAvailableTerritoryIndex();
            }

            var territoryEntity = VRisingCore.EntityManager.CreateEntity();

            // Add basic components
            VRisingCore.EntityManager.AddComponentData(territoryEntity, new LocalToWorld());
            VRisingCore.EntityManager.AddComponentData(territoryEntity, new Translation { Value = centerPosition });

            // Add castle territory component
            VRisingCore.EntityManager.AddComponentData(territoryEntity, new CastleTerritory {
                CastleTerritoryIndex = newTerritoryIndex
            });

            // Create territory blocks buffer
            var buffer = VRisingCore.EntityManager.AddBuffer<CastleTerritoryBlocks>(territoryEntity);

            // Fill the buffer with block coordinates in the territory area
            for (int x = -territorySize; x <= territorySize; x++)
            {
                for (int z = -territorySize; z <= territorySize; z++)
                {
                    var blockCoord = new int2(
                        (int)math.floor((centerPosition.x + x * BLOCK_SIZE) / BLOCK_SIZE),
                        (int)math.floor((centerPosition.z + z * BLOCK_SIZE) / BLOCK_SIZE)
                    );

                    buffer.Add(new CastleTerritoryBlocks {
                        BlockCoordinate = blockCoord,
                        // Add other fields as needed
                    });

                    // Update mappings
                    blockCoordToTerritoryIndex[blockCoord] = newTerritoryIndex;
                    blockCoordToTerritory[blockCoord] = territoryEntity;
                }
            }

            VRisingCore.Log?.LogInfo($"Created new castle territory at {centerPosition} with index {newTerritoryIndex}");
            return territoryEntity;
        }
        catch (System.Exception ex)
        {
            VRisingCore.Log?.LogError($"Failed to create castle territory: {ex.Message}");
            return Entity.Null;
        }
    }

    /// <summary>
    /// Converts a world position to a grid position.
    /// </summary>
    /// <param name="pos">The world position to convert.</param>
    /// <returns>The grid position.</returns>
    public static float3 ConvertPosToGrid(float3 pos)
    {
        return new float3(Mathf.FloorToInt(pos.x * 2) + 6400, pos.y, Mathf.FloorToInt(pos.z * 2) + 6400);
    }

    /// <summary>
    /// Converts a world position to a block coordinate.
    /// </summary>
    /// <param name="pos">The world position to convert.</param>
    /// <returns>The block coordinate as an int2.</returns>
    public static int2 ConvertPosToBlockCoord(float3 pos)
    {
        var gridPos = ConvertPosToGrid(pos);
        return new int2((int)math.floor(gridPos.x / BLOCK_SIZE), (int)math.floor(gridPos.z / BLOCK_SIZE));
    }

    /// <summary>
    /// Gets entities with a specific component type.
    /// </summary>
    private static NativeArray<Entity> GetEntitiesByComponentType<T>() where T : IComponentData
    {
        var query = VRisingCore.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<T>());
        return query.ToEntityArray(Allocator.Temp);
    }

    /// <summary>
    /// Gets the next available territory index for new territories.
    /// </summary>
    private static int GetNextAvailableTerritoryIndex()
    {
        int maxIndex = 0;
        foreach (var index in blockCoordToTerritoryIndex.Values)
        {
            if (index > maxIndex)
                maxIndex = index;
        }
        return maxIndex + 1;
    }
}
