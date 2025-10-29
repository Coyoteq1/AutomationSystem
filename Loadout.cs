using System.Collections.Generic;

namespace CrowbaneArena
{
    public class Loadout
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Weapons { get; set; } = new List<string>();
        public List<string> ArmorSets { get; set; } = new List<string>();
        public List<string> Consumables { get; set; } = new List<string>();
        public bool Enabled { get; set; }
    }
}
