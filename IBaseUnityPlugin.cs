using BepInEx.Logging;
using UnityEngine;

namespace CrowbaneArena
{
    public interface IBaseUnityPlugin
    {
        ManualLogSource Log { get; }
        string Name { get; }
        string Version { get; }

        T AddComponent<T>() where T : Component;
        T GetComponent<T>() where T : Component;
        void Load();
    }
}