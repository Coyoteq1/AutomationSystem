using Unity.Entities;

namespace CrowbaneArena
{
    /// <summary>
    /// Extension methods for Unity Entities to provide convenient entity manipulation.
    /// These are commonly provided by VRising modding frameworks like Wetstone.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Checks if an entity has a specific component.
        /// </summary>
        public static bool Has<T>(this Entity entity)
        {
            return VRisingCore.EntityManager.HasComponent<T>(entity);
        }

        /// <summary>
        /// Reads a component from an entity.
        /// </summary>
        public static T Read<T>(this Entity entity)
        {
            return VRisingCore.EntityManager.GetComponentData<T>(entity);
        }

        /// <summary>
        /// Writes a component to an entity.
        /// </summary>
        public static void Write<T>(this Entity entity, T component)
        {
            VRisingCore.EntityManager.SetComponentData(entity, component);
        }

        /// <summary>
        /// Removes a component from an entity.
        /// </summary>
        public static void Remove<T>(this Entity entity)
        {
            VRisingCore.EntityManager.RemoveComponent<T>(entity);
        }

        /// <summary>
        /// Destroys an entity.
        /// </summary>
        public static void Destroy(this Entity entity)
        {
            VRisingCore.EntityManager.DestroyEntity(entity);
        }

        /// <summary>
        /// Gets the translation position of an entity.
        /// </summary>
        public static Unity.Mathematics.float3 Translation(this Entity entity)
        {
            return entity.Read<Unity.Transforms.Translation>().Value;
        }
    }
}
