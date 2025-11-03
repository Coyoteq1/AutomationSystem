namespace CrowbaneArena
{
    public class Build
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Weapon { get; set; } = string.Empty;
        public string WeaponMods { get; set; } = string.Empty;
        public string ArmorSet { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }
}