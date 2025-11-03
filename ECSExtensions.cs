using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Terrain;
using Stunlock.Core;
using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CrowbaneArena
{
    public static class ECSExtensions
    {
        static EntityManager EntityManager => CrowbaneArenaCore.EntityManager;

        public static unsafe void Write<T>(this Entity entity, T componentData) where T : struct
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            byte[] byteArray = StructureToByteArray(componentData);
            int size = Marshal.SizeOf<T>();

            fixed (byte* p = byteArray)
            {
                EntityManager.SetComponentDataRaw(entity, ct.TypeIndex, p, size);
            }
        }

        static byte[] StructureToByteArray<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            byte[] byteArray = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, byteArray, 0, size);
            Marshal.FreeHGlobal(ptr);

            return byteArray;
        }

        public static unsafe T Read<T>(this Entity entity) where T : struct
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            void* rawPointer = EntityManager.GetComponentDataRawRO(entity, ct.TypeIndex);
            T componentData = Marshal.PtrToStructure<T>(new IntPtr(rawPointer));
            return componentData;
        }

        public static DynamicBuffer<T> ReadBuffer<T>(this Entity entity) where T : struct
        {
            return EntityManager.GetBuffer<T>(entity);
        }

        public static bool Has<T>(this Entity entity)
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            return EntityManager.HasComponent(entity, ct);
        }

        public static string LookupName(this PrefabGUID prefabGuid)
        {
            return (CrowbaneArenaCore.PrefabCollection._PrefabLookupMap.TryGetName(prefabGuid, out var name)
                ? name + " " + prefabGuid : "GUID Not Found").ToString();
        }

        public static void Add<T>(this Entity entity)
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            EntityManager.AddComponent(entity, ct);
        }

        public static void Remove<T>(this Entity entity)
        {
            var ct = new ComponentType(Il2CppType.Of<T>());
            EntityManager.RemoveComponent(entity, ct);
        }

        public static TerrainChunk GetChunk(this Entity entity)
        {
            var pos = entity.Read<Translation>().Value;
            return pos.GetChunk();
        }

        public static TerrainChunk GetChunk(this float3 pos)
        {
            return new TerrainChunk { X = (sbyte)((pos.x + 3200) / 160), Y = (sbyte)((pos.z + 3200) / 160) };
        }
    }
}
