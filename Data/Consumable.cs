namespace AutomationSystem.Data
{
    public class Consumable
    {
        public string Name { get; set; } = string.Empty;
        public uint Guid { get; set; }
        public int DefaultAmount { get; set; }
        public bool Enabled { get; set; }
    }
}