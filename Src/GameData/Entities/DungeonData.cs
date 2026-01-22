using System.Text.Json.Serialization;

namespace System_Overload.Src.GameData.Entities
{
    public class DungeonData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("recommended_level")]
        public int RecommendedLevel { get; set; }

        [JsonPropertyName("enemy_pool")]
        public List<string> EnemyPool { get; set; } = new List<string>();

        [JsonPropertyName("boss_id")]
        public string BossId { get; set; } = string.Empty;

        [JsonPropertyName("room_count")]
        public int RoomCount { get; set; }

        [JsonPropertyName("reward_table")]
        public RewardTableData RewardTable { get; set; } = new RewardTableData();
    }

    public class RewardTableData
    {
        [JsonPropertyName("coins")]
        public int Coins { get; set; }

        [JsonPropertyName("xp")]
        public int Xp { get; set; }

        [JsonPropertyName("possible_items")]
        public List<string> PossibleItems { get; set; } = new List<string>();
    }
}
