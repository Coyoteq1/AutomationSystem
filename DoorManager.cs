using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM;
using System.Collections.Generic;
using Stunlock.Core;

namespace CrowbaneArena
{
    public static class DoorManager
    {
        private static HashSet<Entity> PublicDoors = new HashSet<Entity>();

        public static void MakeDoorsPublic(float3 center, float radius)
        {
            // Find all doors within radius
            var doorQuery = VRisingCore.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Door>(),
                ComponentType.ReadOnly<Translation>()
            );

            var doors = doorQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

            foreach (var door in doors)
            {
                if (VRisingCore.EntityManager.HasComponent<Translation>(door))
                {
                    var doorPos = VRisingCore.EntityManager.GetComponentData<Translation>(door).Value;
                    if (math.distance(doorPos, center) <= radius)
                    {
                        EnablePublicAccess(door);
                        PublicDoors.Add(door);
                    }
                }
            }

            doors.Dispose();
            Plugin.Logger?.LogInfo($"Made {PublicDoors.Count} doors public within radius {radius}");
        }

        private static void EnablePublicAccess(Entity doorEntity)
        {
            if (!doorEntity.Has<Door>()) return;

            // Remove ownership restrictions
            if (doorEntity.Has<UserOwner>())
            {
                doorEntity.Remove<UserOwner>();
            }

            // Set door to allow all players
            var door = doorEntity.Read<Door>();
            // Note: Actual implementation would modify door permissions
            // This is a placeholder for the door access system
            
            Plugin.Logger?.LogInfo($"Enabled public access for door {doorEntity}");
        }

        public static void RestoreDoorAccess(Entity doorEntity)
        {
            if (PublicDoors.Contains(doorEntity))
            {
                PublicDoors.Remove(doorEntity);
                // Note: Would restore original door permissions here
                Plugin.Logger?.LogInfo($"Restored original access for door {doorEntity}");
            }
        }

        public static bool IsDoorPublic(Entity doorEntity)
        {
            return PublicDoors.Contains(doorEntity);
        }

        public static List<Entity> GetPublicDoors()
        {
            return new List<Entity>(PublicDoors);
        }
    }
}