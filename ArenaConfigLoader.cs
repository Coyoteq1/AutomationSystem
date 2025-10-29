using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Unity.Mathematics;

namespace CrowbaneArena
{
    public static class ArenaConfigLoader
    {
        private static readonly string ConfigPath = Path.Combine(BepInEx.Paths.ConfigPath, "crowbanearena", "arena_config.json");
        private static ConfigFile _configFile;

        public static ArenaConfig ArenaSettings => _configFile?.ArenaSettings;
        public static GameplaySettings GameplaySettings => _configFile?.GameplaySettings;

        public static void Initialize()
        {
            _configFile = LoadConfig();
        }

        private static ConfigFile LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonConvert.DeserializeObject<ConfigFile>(json);
                    if (config?.ArenaSettings == null)
                    {
                        config.ArenaSettings = new ArenaConfig();
                    }
                    if (config?.GameplaySettings == null)
                    {
                        config.GameplaySettings = new GameplaySettings();
                    }
                    return config;
                }
                else
                {
                    var defaultConfig = GetDefaultConfig();
                    SaveConfigInternal(defaultConfig);
                    return defaultConfig;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Failed to load config: {ex.Message}");
                return GetDefaultConfig();
            }
        }

        private static ConfigFile GetDefaultConfig()
        {
            return new ConfigFile
            {
                ArenaSettings = new ArenaConfig
                {
                    Enabled = true,
                    ZoneCenter = new float3(-1000f, 0f, -500f),
                    ZoneRadius = 60f,
                    EntryPoint = new float3(-1000f, 0f, -500f),
                    EntryRadius = 100f,
                    ExitPoint = new float3(-1000f, 0f, -500f),
                    ExitRadius = 90f,
                    SpawnPoint = new float3(-1000f, 0f, -500f)
                },
                GameplaySettings = new GameplaySettings()
            };
        }

        public static void SaveConfig()
        {
            if (_configFile != null)
            {
                SaveConfigInternal(_configFile);
            }
        }

        public static List<Weapon> GetWeapons()
        {
            return ArenaSettings?.Weapons ?? new List<Weapon>();
        }


        public static List<ArmorSet> GetArmorSets()
        {
            return ArenaSettings?.ArmorSets ?? new List<ArmorSet>();
        }

        public static List<Loadout> GetLoadouts()
        {
            return ArenaSettings?.Loadouts ?? new List<Loadout>();
        }

        public static List<Build> GetBuilds()
        {
            return ArenaSettings?.Builds ?? new List<Build>();
        }

        public static bool TryGetLoadout(string name, out Loadout loadout)
        {
            loadout = ArenaSettings?.Loadouts?.FirstOrDefault(l => l.Name == name);
            return loadout != null;
        }

        public static bool TryGetWeaponVariant(string name, out Weapon weapon, out WeaponVariant variant)
        {
            weapon = ArenaSettings?.Weapons?.FirstOrDefault(w => w.Name == name);
            variant = weapon?.Variants?.FirstOrDefault();
            return weapon != null;
        }

        public static bool TryGetArmorSet(string name, out ArmorSet armorSet)
        {
            armorSet = ArenaSettings?.ArmorSets?.FirstOrDefault(a => a.Name == name);
            return armorSet != null;
        }

        public static bool TryGetConsumable(string name, out Consumable consumable)
        {
            consumable = ArenaSettings?.Consumables?.FirstOrDefault(c => c.Name == name);
            return consumable != null;
        }

        private static void SaveConfigInternal(ConfigFile config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Failed to save config: {ex.Message}");
            }
        }
    }

    public class ConfigFile
    {
        public ArenaConfig ArenaSettings { get; set; }
        public GameplaySettings GameplaySettings { get; set; }
    }

    public class ArenaConfig
    {
        public bool Enabled { get; set; } = true;
        public float3 ZoneCenter { get; set; } = new float3(-1000f, 0f, -500f);
        public float ZoneRadius { get; set; } = 60f;
        public float3 EntryPoint { get; set; } = new float3(-1000f, 0f, -500f);
        public float EntryRadius { get; set; } = 100f;
        public float3 ExitPoint { get; set; } = new float3(-1000f, 0f, -500f);
        public float ExitRadius { get; set; } = 90f;
        public float3 SpawnPoint { get; set; } = new float3(-1000f, 0f, -500f);
        public List<ZoneConfig> Zones { get; set; } = new List<ZoneConfig>();
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        public List<ArmorSet> ArmorSets { get; set; } = new List<ArmorSet>();
        public List<Loadout> Loadouts { get; set; } = new List<Loadout>();
        public List<Build> Builds { get; set; } = new List<Build>();
        public List<Consumable> Consumables { get; set; } = new List<Consumable>();
    }

    public class GameplaySettings
    {
        public bool EnableGodMode { get; set; } = true;
        public bool RestoreOnExit { get; set; } = true;
        public bool AllowPvP { get; set; } = true;
        public bool VBloodProgression { get; set; } = false;
    }

    public class LoadoutConfig
    {
        public string Name { get; set; } = "default";
        public string Description { get; set; } = "Default loadout";
        public bool Enabled { get; set; } = true;
    }

    public class ZoneConfig
    {
        public string Name { get; set; } = "default";
        public string Description { get; set; } = "Default zone";
        public bool Enabled { get; set; } = true;
        public float3 Center { get; set; } = new float3(0f, 0f, 0f);
        public float Radius { get; set; } = 100f;
        public float SpawnX { get; set; } = 0f;
        public float SpawnY { get; set; } = 0f;
        public float SpawnZ { get; set; } = 0f;
    }
}