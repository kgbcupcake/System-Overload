using Src.GameData.Components;
using System.Collections.Generic;
using System.Linq;
using System;

namespace System_Overload.Src.GameData.Entities
{
	public class PlayerData
	{
		public class loadPlayer
		{
			private Random rand = new Random();

			// Core Identity
			public string PlayerName { get; set; } = "Unnamed Operative";
			public string PlayerClass { get; set; } = "Runner";
			public int Level { get; set; } = 1;
			public int InventorySize { get; set; } = 20;

			// --- NEURAL ALLOCATION STATS (The missing pieces) ---
			public int IntegrityPoints { get; set; } = 0;   // Spent on Max HP
			public int ThroughputPoints { get; set; } = 0;  // Spent on Attack Power
			public int FirewallPoints { get; set; } = 0;    // Spent on Defense



			// --- Store --- ///
			public int StoreStimStock { get; set; } = 5;
			public int StorePriceModifier { get; set; } = 0; // Tracks how many you've ever bought
			public DateTime LastRestockTime { get; set; } = DateTime.Now;








			private int _health = 100;
			public int Health
			{
				get => _health;
				set => _health = Math.Clamp(value, 0, (int)GetTotalMaxHealth());
			}
			public int HitPoints { get; set; } = 100;

			// Progression & Economy
			public int Experience { get; set; } = 0;
			public int ExperienceToLevel { get; set; } = 100;
			public int Coins { get; set; } = 50;
			public int DifficultyRating { get; set; } = 0;

			public List<ItemData> Inventory { get; set; } = new List<ItemData>();
			public List<WeaponData> Weapons { get; set; } = new List<WeaponData>();

			// --- CALCULATIONS ---

			public void AddXP(int amount)
			{
				Experience += amount;
				while (Experience >= ExperienceToLevel)
				{
					Experience -= ExperienceToLevel;
					Level++;
					ExperienceToLevel = (int)(ExperienceToLevel * 1.25f);
				}
			}

			public void AddGold(int amount) => Coins += amount;

			public float GetTotalDamage()
			{
				// Base Damage (10) + Throughput Points (3 per point)
				float baseDmg = 10 + (ThroughputPoints * 3);

				float gemBonus = Inventory
					.SelectMany(item => item.SocketedGems)
					.Where(gem => gem != null)
					.SelectMany(gem => gem.Attributes.Select(attr => new { gem.Power, Attribute = attr }))
					.Where(x => x.Attribute == EffectType.Fire ||
								x.Attribute == EffectType.Ice ||
								x.Attribute == EffectType.Poison ||
								x.Attribute == EffectType.Electric)
					.Sum(x => x.Power);

				return baseDmg + gemBonus;
			}

			public float GetTotalMaxHealth()
			{
				// Base Health + Integrity Points (15 per point)
				float baseMax = HitPoints + (IntegrityPoints * 15);

				float gemBonus = Inventory
					.SelectMany(i => i.SocketedGems)
					.SelectMany(g => g.Attributes.Select(attr => new { Gem = g, Attribute = attr }))
					.Where(x => x.Attribute == EffectType.Restorative || x.Attribute == EffectType.Defense)
					.Sum(x => x.Gem.Power);

				return baseMax + gemBonus;
			}

			public float GetTotalDefense()
			{
				// Base Defense (10) + Firewall Points (2 per point)
				float baseDefense = 10.0f + (FirewallPoints * 2);

				float gemDefense = Inventory
					.SelectMany(i => i.SocketedGems)
					.SelectMany(g => g.Attributes.Select(attr => new { Gem = g, Attribute = attr }))
					.Where(x => x.Attribute == EffectType.Defense)
					.Sum(x => x.Gem.Power);

				return baseDefense + gemDefense;
			}

			public int ArmorValue => (int)GetTotalDefense();

			public int GetEnemyPower() => rand.Next(DifficultyRating + 1, 2 * DifficultyRating + 3);
			public int GetLootCoins() => rand.Next(10 * DifficultyRating + 10, 15 * DifficultyRating + 50);
		}
	}
}