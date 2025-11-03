using ProjectM;
using Unity.Entities;

namespace AutomationSystem.Data
{
    public class FoundVBlood
    {
        public Stunlock.Core.PrefabGUID GUID { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsAlive { get; set; }
        public Unity.Entities.Entity Entity { get; set; }
        
        public FoundVBlood() { }
        
        public FoundVBlood(Stunlock.Core.PrefabGUID guid, string name, Unity.Entities.Entity entity)
        {
            GUID = guid;
            Name = name;
            Entity = entity;
            IsAlive = false;
        }
    }
}
