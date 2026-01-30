using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Src.GameData.Components; // To access EffectType and GlobalAura

namespace System_Overload.Src.GameData.Entities
{
	public class EnemyData
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Level { get; set; }

		// Match the JSON names (Health vs MaxHealth, Damage vs Attack)
		public int Health { get; set; }
		public int Damage { get; set; }

		public int Defense { get; set; }
		public int Speed { get; set; }
		public int Accuracy { get; set; }
		public int CritChance { get; set; }
		public int XpReward { get; set; }
		public int RewardCoins { get; set; }

		// Logic for the Aura and Attributes
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public GlobalAura Aura { get; set; } = GlobalAura.None;

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public List<EffectType> Attributes { get; set; } = new();

		// Updated LootTable to handle the complex list from your JSON
		public List<LootEntry> LootTable { get; set; } = new();
	}

	public class LootEntry
	{
		public string ItemName { get; set; }
		public int DropChance { get; set; }
		public int MinQuantity { get; set; }
		public int MaxQuantity { get; set; }
	}
}