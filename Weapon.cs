namespace CrowbaneArena
{
    public class Weapon
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Guid { get; set; }
        public List<WeaponVariant> Variants { get; set; } = new List<WeaponVariant>();
        public bool Enabled { get; set; }
    }

    public class WeaponVariant
    {
        public string ModCombo { get; set; }
        public int VariantGuid { get; set; }
        public string FriendlyName { get; set; }
    }
}
