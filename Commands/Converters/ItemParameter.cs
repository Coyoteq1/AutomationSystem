using ProjectM;
using VampireCommandFramework;
using Stunlock.Core;

namespace AutomationSystem.Commands.Converters
{
    public class ItemParameter
    {
        public Stunlock.Core.PrefabGUID Value { get; set; }
        
        public static implicit operator ItemParameter(Stunlock.Core.PrefabGUID guid) => new ItemParameter { Value = guid };
        public static implicit operator Stunlock.Core.PrefabGUID(ItemParameter item) => item.Value;
    }
}
