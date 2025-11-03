
using Unity.Entities;

namespace AutomationSystem.Core
{
    public interface IVRisingCore
    {
        EntityManager EntityManager { get; }
        void Initialize();
    }
}
