namespace CrowbaneArena
{
    public partial class Loadout
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Weapons { get; set; } = new List<string>();
        public List<string> ArmorSets { get; set; } = new List<string>();
        public List<string> Consumables { get; set; } = new List<string>();
        public bool Enabled { get; set; }
    }
}