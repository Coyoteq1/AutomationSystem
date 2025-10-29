using System.Collections.Generic;
using Stunlock.Core;

namespace CrowbaneArena.Data
{
    /// <summary>
    /// Central repository for VRising item prefabs used in CrowbaneArena.
    /// Stores PrefabGUIDs indexed by human-readable item names for easy reference in commands.
    /// </summary>
    public static class Prefabs
    {
        // === WEAPONS ===
        public static readonly Dictionary<string, PrefabGUID> Weapons = new()
        {
            // Base Weapons
            ["sword"] = new PrefabGUID(-774462329),
            ["mace"] = new PrefabGUID(-1569279652),
            ["spear"] = new PrefabGUID(1532449451),
            ["greatsword"] = new PrefabGUID(147836723),
            ["crossbow"] = new PrefabGUID(1389040540),
            ["daggers"] = new PrefabGUID(1031107636),

            // Enhanced Weapons (Add your custom GUIDs here)
            // ["enchanted_sword"] = new PrefabGUID(0), // Replace 0 with actual GUID
            // ["legendary_greatsword"] = new PrefabGUID(0), // Replace 0 with actual GUID
        };

        // === ARMOR SETS ===
        public static readonly Dictionary<string, Dictionary<string, PrefabGUID>> ArmorSets = new()
        {
            ["warrior"] = new Dictionary<string, PrefabGUID>
            {
                ["chest"] = new PrefabGUID(1392314162),  // Dracula Warrior Chest
                ["legs"] = new PrefabGUID(205207385),    // Dracula Warrior Legs
                ["boots"] = new PrefabGUID(-382349289),  // Dracula Warrior Boots
                ["gloves"] = new PrefabGUID(1982551454), // Dracula Warrior Gloves
            },
            ["rogue"] = new Dictionary<string, PrefabGUID>
            {
                ["chest"] = new PrefabGUID(933057100),   // Dracula Rogue Chest
                ["legs"] = new PrefabGUID(-345596442),   // Dracula Rogue Legs
                ["boots"] = new PrefabGUID(1855323424),  // Dracula Rogue Boots
                ["gloves"] = new PrefabGUID(-1826382550), // Dracula Rogue Gloves
            },
            ["scholar"] = new Dictionary<string, PrefabGUID>
            {
                ["chest"] = new PrefabGUID(114259912),   // Dracula Scholar Chest
                ["legs"] = new PrefabGUID(1592149279),   // Dracula Scholar Legs
                ["boots"] = new PrefabGUID(1531721602),  // Dracula Scholar Boots
                ["gloves"] = new PrefabGUID(-1899539896), // Dracula Scholar Gloves
            },
            ["brute"] = new Dictionary<string, PrefabGUID>
            {
                ["chest"] = new PrefabGUID(1033753207),  // Dracula Brute Chest
                ["legs"] = new PrefabGUID(993033515),    // Dracula Brute Legs
                ["boots"] = new PrefabGUID(1646489863),  // Dracula Brute Boots
                ["gloves"] = new PrefabGUID(1039083725), // Dracula Brute Gloves
            }
        };

        // === CONSUMABLES ===
        public static readonly Dictionary<string, PrefabGUID> Consumables = new()
        {
            ["blood_rose_potion"] = new PrefabGUID(828432508),
            ["exquisite_brew"] = new PrefabGUID(1223264867),
            ["physical_brew"] = new PrefabGUID(-1568756102),
            ["spell_brew"] = new PrefabGUID(1510182325),

            // Add more consumables as needed
            // ["health_potion"] = new PrefabGUID(0),
            // ["mana_potion"] = new PrefabGUID(0),
        };

        // === SPELLS/ABILITIES (For .ability command) ===
        public static readonly Dictionary<string, PrefabGUID> Spells = new()
        {
            ["bat"] = new PrefabGUID(-1905691330),
            ["frost"] = new PrefabGUID(-1342764880),
            ["bear"] = new PrefabGUID(1699865363),
            ["stone"] = new PrefabGUID(-2025101517),
            ["teleport"] = new PrefabGUID(1362041468),
            ["rage"] = new PrefabGUID(-1065970933),

            // Add more spells as needed
        };

        // === UNITS/MOBS ===
        public static readonly Dictionary<string, PrefabGUID> Units = new()
        {
            ["bat"] = new PrefabGUID(-1905691330),
            ["wolf"] = new PrefabGUID(-1342764880),
            ["bear"] = new PrefabGUID(1699865363),

            // Add more units as needed
        };

        // === BUFFS/DEBUFFS ===
        public static class Buffs
        {
            // Common buff references
            public static readonly PrefabGUID GeneralVampireWounded = new PrefabGUID(0); // Add actual GUID

            // Magic source buffs (from EquipmentPatches)
            public static readonly PrefabGUID BloodKeyT01 = new PrefabGUID(0); // Add actual GUIDs
            public static readonly PrefabGUID GeneralMagic = new PrefabGUID(0);
            // Add more as needed...
        }

        /// <summary>
        /// Attempts to find a weapon prefab by name.
        /// </summary>
        /// <param name="name">The weapon name to search for.</param>
        /// <param name="guid">The PrefabGUID if found.</param>
        /// <returns>True if the weapon was found.</returns>
        public static bool TryGetWeapon(string name, out PrefabGUID guid)
        {
            return Weapons.TryGetValue(name.ToLowerInvariant(), out guid);
        }

        /// <summary>
        /// Attempts to find a consumable prefab by name.
        /// </summary>
        /// <param name="name">The consumable name to search for.</param>
        /// <param name="guid">The PrefabGUID if found.</param>
        /// <returns>True if the consumable was found.</returns>
        public static bool TryGetConsumable(string name, out PrefabGUID guid)
        {
            return Consumables.TryGetValue(name.ToLowerInvariant(), out guid);
        }

        /// <summary>
        /// Attempts to find a spell/ability prefab by name.
        /// </summary>
        /// <param name="name">The spell name to search for.</param>
        /// <param name="guid">The PrefabGUID if found.</param>
        /// <returns>True if the spell was found.</returns>
        public static bool TryGetSpell(string name, out PrefabGUID guid)
        {
            return Spells.TryGetValue(name.ToLowerInvariant(), out guid);
        }

        /// <summary>
        /// Attempts to find a unit/mob prefab by name.
        /// </summary>
        /// <param name="name">The unit name to search for.</param>
        /// <param name="guid">The PrefabGUID if found.</param>
        /// <returns>True if the unit was found.</returns>
        public static bool TryGetUnit(string name, out PrefabGUID guid)
        {
            return Units.TryGetValue(name.ToLowerInvariant(), out guid);
        }

        /// <summary>
        /// Attempts to find an armor piece by set name and slot.
        /// </summary>
        /// <param name="setName">The armor set name (warrior, rogue, scholar, brute).</param>
        /// <param name="slotName">The slot name (chest, legs, boots, gloves).</param>
        /// <param name="guid">The PrefabGUID if found.</param>
        /// <returns>True if the armor piece was found.</returns>
        public static bool TryGetArmorPiece(string setName, string slotName, out PrefabGUID guid)
        {
            guid = PrefabGUID.Empty;
            if (ArmorSets.TryGetValue(setName.ToLowerInvariant(), out var set))
            {
                return set.TryGetValue(slotName.ToLowerInvariant(), out guid);
            }
            return false;
        }

        /// <summary>
        /// Gets a complete armor set by name.
        /// </summary>
        /// <param name="setName">The armor set name.</param>
        /// <returns>Dictionary of slot -> GUID, or empty dict if not found.</returns>
        public static Dictionary<string, PrefabGUID> GetArmorSet(string setName)
        {
            if (ArmorSets.TryGetValue(setName.ToLowerInvariant(), out var set))
            {
                return new Dictionary<string, PrefabGUID>(set);
            }
            return new();
        }

        /// <summary>
        /// Gets all available item categories and their item counts.
        /// </summary>
        /// <returns>Dictionary of category -> item count.</returns>
        public static Dictionary<string, int> GetItemCategories()
        {
            return new Dictionary<string, int>
            {
                ["Weapons"] = Weapons.Count,
                ["Armor Sets"] = ArmorSets.Count,
                ["Consumables"] = Consumables.Count,
                ["Spells/Abilities"] = Spells.Count,
                ["Units"] = Units.Count,
            };
        }

        /// <summary>
        /// Gets all items in a specific category as a formatted list.
        /// </summary>
        /// <param name="category">The category name (Weapons, ArmorSets, Consumables, etc.).</param>
        /// <returns>Formatted string list of items.</returns>
        public static string GetItemsInCategory(string category)
        {
            switch (category.ToLowerInvariant())
            {
                case "weapons":
                    return string.Join(", ", Weapons.Keys);
                case "armorsets":
                case "armor sets":
                    return string.Join(", ", ArmorSets.Keys);
                case "consumables":
                    return string.Join(", ", Consumables.Keys);
                case "spells":
                case "abilities":
                case "spells/abilities":
                    return string.Join(", ", Spells.Keys);
                case "units":
                    return string.Join(", ", Units.Keys);
                default:
                    return "Category not found. Available: Weapons, ArmorSets, Consumables, Spells/Abilities, Units";
            }
        }

        /// <summary>
        /// Attempts to find any item across all categories.
        /// </summary>
        /// <param name="name">The item name to search for.</param>
        /// <param name="guid">The PrefabGUID if found.</param>
        /// <param name="category">The category where the item was found.</param>
        /// <returns>True if the item was found in any category.</returns>
        public static bool TryGetAnyItem(string name, out PrefabGUID guid, out string category)
        {
            string lowerName = name.ToLowerInvariant();

            // Check weapons
            if (Weapons.TryGetValue(lowerName, out guid))
            {
                category = "Weapon";
                return true;
            }

            // Check consumables
            if (Consumables.TryGetValue(lowerName, out guid))
            {
                category = "Consumable";
                return true;
            }

            // Check spells
            if (Spells.TryGetValue(lowerName, out guid))
            {
                category = "Spell/Ability";
                return true;
            }

            // Check units
            if (Units.TryGetValue(lowerName, out guid))
            {
                category = "Unit";
                return true;
            }

            // Check armor sets (full set or pieces)
            foreach (var armorSet in ArmorSets)
            {
                if (armorSet.Key == lowerName)
                {
                    // Return first piece as representative (usually chest)
                    guid = armorSet.Value.ContainsKey("chest") ? armorSet.Value["chest"] : armorSet.Value.Values.First();
                    category = "Armor Set";
                    return true;
                }
            }

            guid = PrefabGUID.Empty;
            category = "Not Found";
            return false;
        }
    }
}
