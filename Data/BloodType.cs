using System;
using System.Collections.Generic;
using ProjectM;
using Unity.Entities;
using Stunlock.Core;

namespace CrowbaneArena
{
    public class BloodType
    {
        public string Name { get; set; } = string.Empty;
        public int Guid { get; set; }
        public bool Enabled { get; set; }
        
        // Blood type GUID values
        public const int Frailed = -899826404;
        public const int Creature = -77658840;
        public const int Warrior = -1094467405;
        public const int Rogue = 793735874;
        public const int Brute = 581377887;
        public const int Scholar = -586506765;
        public const int Worker = -540707191;
        public const int Mutant = -2017994753;
        public const int Dracula = -1013980433;
        public const int Solarus = -1501241589;
        public const int Mairwyn = -1702718259;
        public const int Beast = -1327161224;
        public const int Pure = 2040223283;
        public const int Corrupted = -1889797341;

        public static Dictionary<string, int> GetAll()
        {
            return new Dictionary<string, int>()
            {
                { "Frailed", Frailed },
                { "Creature", Creature },
                { "Warrior", Warrior },
                { "Rogue", Rogue },
                { "Brute", Brute },
                { "Scholar", Scholar },
                { "Worker", Worker },
                { "Mutant", Mutant },
                { "Dracula", Dracula },
                { "Solarus", Solarus },
                { "Mairwyn", Mairwyn },
                { "Beast", Beast },
                { "Pure", Pure },
                { "Corrupted", Corrupted }
            };
        }

        public static PrefabGUID GetPrefabGUID(int bloodTypeGuid)
        {
            return new PrefabGUID(bloodTypeGuid);
        }

        public static bool HasBloodType(Entity entity, int bloodTypeGuid)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<Blood>(entity))
            {
                var blood = em.GetComponentData<Blood>(entity);
                return blood.BloodType.GuidHash == bloodTypeGuid;
            }
            return false;
        }
    }
}