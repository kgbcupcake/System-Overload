using System.Text.Json.Serialization;

namespace System_Overload.Src.GameData.Entities
{
    public class NewEnemyData
    {
        [JsonIgnore]
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
