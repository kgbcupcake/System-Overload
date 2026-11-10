using System.Text.Json.Serialization;

namespace Maries_DataModels
{
    [MariesGui.Contracts.MariesGuiContent("drones")]
    public class NewEnemyData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("maxHealth")]
        public int MaxHealth { get; set; }

        [JsonPropertyName("attackPower")]
        public int AttackPower { get; set; }

        [JsonPropertyName("defense")]
        public int Defense { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        /// <summary>Percent chance to hit (before level difference).</summary>
        [JsonPropertyName("accuracy")]
        public int Accuracy { get; set; } = 85;

        /// <summary>Percent chance of a critical hit (1.5x damage).</summary>
        [JsonPropertyName("critChance")]
        public int CritChance { get; set; } = 5;

        /// <summary>Verb for the combat log, e.g. "bites" in "Packet Mite bites for 6 damage".</summary>
        [JsonPropertyName("attackVerb")]
        public string AttackVerb { get; set; } = "strikes";

        [JsonPropertyName("rewards")]
        public EnemyRewardData Rewards { get; set; } = new EnemyRewardData();
    }

    public class EnemyRewardData
    {
        [JsonPropertyName("xp")]
        public int Xp { get; set; }

        [JsonPropertyName("coins")]
        public int Coins { get; set; }
    }
}
