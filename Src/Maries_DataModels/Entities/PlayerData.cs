using System.Collections.Generic;
using System.Linq;
using System;

namespace Maries_DataModels
{
	public class PlayerData
	{
		[MariesGui.Contracts.MariesGuiContent("profiles")]
		public class loadPlayer
		{
			private Random rand = new Random();

			// Core Identity
			public string PlayerName { get; set; } = "Unnamed Hero";
			public string PlayerClass { get; set; } = "Adventurer";
			public int Level { get; set; } = 1;
			public int InventorySize { get; set; } = 20;

			// --- ATTRIBUTE POINTS (spent at character creation) ---
			public int VitalityPoints { get; set; } = 0;   // Vitality: spent on Max HP
			public int StrengthPoints { get; set; } = 0;  // Strength: spent on Attack Power
			public int FortitudePoints { get; set; } = 0;    // Fortitude: spent on Defense



			// --- Store --- ///
			public int StorePotionStock { get; set; } = 5;
			public int StorePriceModifier { get; set; } = 0; // Tracks how many you've ever bought
			public DateTime LastRestockTime { get; set; } = DateTime.Now;








			// Clamped on read, not on write: JSON sets Health before HitPoints and the stat points
			// are loaded, so clamping in the setter cut a saved 200 HP down to the default max.
			private int _health = 100;
			public int Health
			{
				get => Math.Clamp(_health, 0, (int)GetTotalMaxHealth());
				set => _health = Math.Max(0, value);
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
				// Base Damage (10) + Strength Points (3 per point)
				float baseDmg = 10 + (StrengthPoints * 3);

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
				// Base Health + Vitality Points (15 per point)
				float baseMax = HitPoints + (VitalityPoints * 15);

				float gemBonus = Inventory
					.SelectMany(i => i.SocketedGems)
					.SelectMany(g => g.Attributes.Select(attr => new { Gem = g, Attribute = attr }))
					.Where(x => x.Attribute == EffectType.Restorative || x.Attribute == EffectType.Defense)
					.Sum(x => x.Gem.Power);

				return baseMax + gemBonus;
			}

			public float GetTotalDefense()
			{
				// Base Defense (10) + Fortitude Points (2 per point)
				float baseDefense = 10.0f + (FortitudePoints * 2);

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