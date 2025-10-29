using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM;
using ProjectM.CastleBuilding;
using System.Collections.Generic;
using CrowbaneArena.Services;
using Stunlock.Core;

namespace CrowbaneArena
{
    public static class CastleManager
    {
        private static List<Entity> ArenaCastleHearts = new List<Entity>();
        private static Dictionary<Entity, float> CastleRadii = new Dictionary<Entity, float>();

        public static Entity CreateCastleHeart(float3 position, float radius = 50f)
        {
            var castleHeartPrefab = new PrefabGUID(-1843552918); // Castle Heart GUID

            // TODO: Implement proper spawning using ServerGameManager
            var heartEntity = VRisingCore.EntityManager.CreateEntity();
            VRisingCore.EntityManager.AddComponentData(heartEntity, new Translation { Value = position });
            VRisingCore.EntityManager.AddComponentData(heartEntity, new PrefabGUID(-1843552918));

            SetupCastleTerritory(heartEntity, position, radius);

            ArenaCastleHearts.Add(heartEntity);
            CastleRadii[heartEntity] = radius;
            Plugin.Logger?.LogInfo($"Created castle heart at {position} with radius {radius}");

            return heartEntity;
        }

        private static void SetupCastleTerritory(Entity heartEntity, float3 position, float radius)
        {
            if (!heartEntity.Has<CastleHeart>()) return;

            var castleHeart = heartEntity.Read<CastleHeart>();
            var territoryIndex = CastleTerritoryService.GetTerritoryIndex(position);
            
            // Create territory blocks
            CreateTerritoryBlocks(heartEntity, position, radius);
            
            // Enable public access
            EnablePublicAccess(heartEntity);
            
            // Make all doors public in territory
            DoorManager.MakeDoorsPublic(position, radius);
            
            Plugin.Logger?.LogInfo($"Setup castle territory for heart {heartEntity} at index {territoryIndex}");
        }

        private static void CreateTerritoryBlocks(Entity heartEntity, float3 center, float radius)
        {
            var blockRadius = (int)(radius / 10f); // 10 units per block
            var centerBlock = CastleTerritoryService.ConvertPosToBlockCoord(center);
            
            var territoryBlocks = new List<int2>();
            
            for (int x = -blockRadius; x <= blockRadius; x++)
            {
                for (int z = -blockRadius; z <= blockRadius; z++)
                {
                    var blockCoord = new int2(centerBlock.x + x, centerBlock.y + z);
                    var blockWorldPos = new float3(blockCoord.x * 10f + 5f, center.y, blockCoord.y * 10f + 5f);
                    
                    if (math.distance(blockWorldPos, center) <= radius)
                    {
                        territoryBlocks.Add(blockCoord);
                    }
                }
            }
            
            Plugin.Logger?.LogInfo($"Created {territoryBlocks.Count} territory blocks for castle heart");
        }

        private static void EnablePublicAccess(Entity heartEntity)
        {
            if (!heartEntity.Has<CastleHeart>()) return;

            // Set castle to allow public access
            var castleHeart = heartEntity.Read<CastleHeart>();
            // Note: Actual implementation would modify castle permissions
            // This is a placeholder for the permission system
            
            Plugin.Logger?.LogInfo($"Enabled public access for castle heart {heartEntity}");
        }

        public static void RemoveCastleHeart(Entity heartEntity)
        {
            if (ArenaCastleHearts.Contains(heartEntity))
            {
                ArenaCastleHearts.Remove(heartEntity);
                CastleRadii.Remove(heartEntity);

                if (VRisingCore.EntityManager.Exists(heartEntity))
                {
                    VRisingCore.EntityManager.DestroyEntity(heartEntity);
                }

                Plugin.Logger?.LogInfo($"Removed castle heart {heartEntity}");
            }
        }

        public static bool IsInCastleTerritory(float3 position)
        {
            // TODO: Implement proper territory checking
            return false;
        }

        public static List<Entity> GetArenaCastleHearts()
        {
            return new List<Entity>(ArenaCastleHearts);
        }
    }
}