using System_Overload.Src.GameData.Components;
using Src.GameData.Components;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace System_Overload.Src.GameData.Entities
{
    public enum TraitType { None, Fire, Ice, Poison, Electric, Ethereal, Armored, Enraged }

    public class BossData
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int Damage { get; set; }
        public float Speed { get; set; }
        public int Level { get; set; }
        public GlobalAura Aura { get; set; }
        public string Description { get; set; }
        public List<TraitType> Attributes { get; set; }
        public List<LootTableData> LootTable { get; set; }

        public BossData()
        {
            // Default constructor for deserialization
            Name = "Default Boss";
            Health = 1000;
            Damage = 50;
            Speed = 1.0f;
            Level = 1;
            Aura = GlobalAura.None;
            Description = "A formidable foe.";
            Attributes = new List<TraitType>();
            LootTable = new List<LootTableData>();
        }

        public BossData(string name, int health, float speed, int level, List<TraitType> attributes)
        {
            Name = name;
            Health = health;
            Speed = speed;
            Level = level;
            Attributes = attributes;
            // Set default values for properties not passed in the constructor
            Damage = 50;
            Aura = GlobalAura.None;
            Description = "A formidable foe.";
        }
    }
}