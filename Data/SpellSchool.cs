namespace AutomationSystem.Data
{
    public class SpellSchool
    {
        public string Name { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public int Guid { get; set; }
        public bool Enabled { get; set; }
    }
}