using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace CrowbaneArena.Services;

/// <summary>
/// Service for managing and querying castle heart entities in the arena.
/// Supports dynamic creation of castle hearts anywhere on the map.
/// </summary>
public class CastleHeartService
{
    /// <summary>
    /// Query for filtering castle heart entities.
    /// </summary>
    public static EntityQuery CastleHeartQuery;
    private static EntityQueryDesc queryDesc;

    /// <summary>
    /// Initializes a new instance of the CastleHeartService class.
    /// Sets up the entity query for castle hearts with required components.
    /// </summary>
    public CastleHeartService()
    {
        queryDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<CastleHeart>(),
                ComponentType.ReadOnly<Team>(),
            },
        };
        CastleHeartQuery = VRisingCore.EntityManager.CreateEntityQuery(queryDesc);
    }

    /// <summary>
    /// Attempts to find a castle heart entity by its network ID.
    /// </summary>
    /// <param name="castleId">The network ID of the castle to find.</param>
    /// <param name="castle">When this method returns, contains the castle entity if found; otherwise, Entity.Null.</param>
    /// <returns>True if the castle was found; otherwise, false.</returns>
    public static bool TryGetByID(NetworkId castleId, out Entity castle)
    {
        var castleEntities = CastleHeartQuery.ToEntityArray(Allocator.Temp);
        foreach (var castleEntity in castleEntities)
        {
            var networkId = VRisingCore.EntityManager.GetComponentData<NetworkId>(castleEntity);
            if (networkId.Equals(castleId))
            {
                castle = castleEntity;
                castleEntities.Dispose();
                return true;
            }
        }
        castleEntities.Dispose();
        castle = Entity.Null;
        return false;
    }

    /// <summary>
    /// Attempts to find a castle heart entity owned by a specific user.
    /// </summary>
    /// <param name="user">The user whose castle heart should be found.</param>
    /// <param name="castle">When this method returns, contains the castle entity if found; otherwise, Entity.Null.</param>
    /// <returns>True if the castle was found; otherwise, false.</returns>
    public static bool TryGetByOwnerUser(User user, out Entity castle)
    {
        var castleEntities = CastleHeartQuery.ToEntityArray(Allocator.Temp);

        foreach (var castleEntity in castleEntities)
        {
            if (!VRisingCore.EntityManager.Exists(castleEntity)) continue;
            if (VRisingCore.EntityManager.HasComponent<UserOwner>(castleEntity))
            {
                var userOwner = VRisingCore.EntityManager.GetComponentData<UserOwner>(castleEntity);
                if (!VRisingCore.EntityManager.Exists(userOwner.Owner._Entity)) continue;

                var tUser = VRisingCore.EntityManager.GetComponentData<User>(userOwner.Owner._Entity);

                if (tUser.Equals(user))
                {
                    castle = castleEntity;
                    castleEntities.Dispose();
                    return true;
                }
            }
        }

        castleEntities.Dispose();
        castle = Entity.Null;
        return false;
    }

    /// <summary>
    /// Creates a new castle heart entity at the specified position.
    /// This method allows building castle hearts anywhere on the map for arena purposes.
    /// Note: Implementation requires additional VRising component definitions.
    /// </summary>
    /// <param name="position">The position where to create the castle heart.</param>
    /// <param name="ownerUser">The owning user entity.</param>
    /// <param name="team">The team the castle belongs to.</param>
    /// <param name="territoryIndex">The territory index to assign.</param>
    /// <returns>The created castle heart entity.</returns>
    public static Entity CreateCastleHeart(Unity.Mathematics.float3 position, Entity ownerUser, int team = 0, int territoryIndex = -1)
    {
        VRisingCore.Log?.LogWarning("CreateCastleHeart not fully implemented - requires additional VRising component definitions");
        return Entity.Null;
    }
}
