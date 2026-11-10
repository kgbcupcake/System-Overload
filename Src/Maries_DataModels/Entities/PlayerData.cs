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
			public int WisdomPoints { get; set; } = 0;       // Wisdom: +1% critical hit chance per point



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

			/// <summary>Name of the weapon the hero has chosen to fight with (set from the inventory screen).</summary>
			public string EquippedWeapon { get; set; } = "";

			/// <summary>
			/// The weapon the hero fights with: the one they equipped, or their highest-damage weapon
			/// if they haven't picked one (or no longer carry it).
			/// </summary>
			public WeaponData? GetEquippedWeapon()
			{
				var weapons = Inventory.OfType<WeaponData>().ToList();
				return weapons.FirstOrDefault(w => w.Name == EquippedWeapon)
					?? weapons.OrderByDescending(w => w.Damage).FirstOrDefault();
			}

			/// <summary>Armor and clothing the hero is wearing (at most one per slot).</summary>
			public List<ArmorData> GetEquippedArmor() =>
				Inventory.OfType<ArmorData>()
					.Where(a => a.IsEquipped)
					.GroupBy(a => a.Slot, StringComparer.OrdinalIgnoreCase)
					.Select(g => g.First())
					.ToList();

			/// <summary>What the hero is wearing in a slot, or null.</summary>
			public ArmorData? GetArmorInSlot(string slot) =>
				GetEquippedArmor().FirstOrDefault(a => a.Slot.Equals(slot, StringComparison.OrdinalIgnoreCase));

			/// <summary>Wears a piece of gear, taking off whatever was in its slot.</summary>
			public void EquipArmor(ArmorData armor)
			{
				foreach (var worn in Inventory.OfType<ArmorData>().Where(a => a.Slot.Equals(armor.Slot, StringComparison.OrdinalIgnoreCase)))
					worn.IsEquipped = false;
				armor.IsEquipped = true;
			}

			/// <summary>Everything that counts for stats: the equipped weapon plus worn gear.</summary>
			public IEnumerable<ItemData> GetEquippedItems()
			{
				if (GetEquippedWeapon() is { } weapon) yield return weapon;
				foreach (var armor in GetEquippedArmor()) yield return armor;
			}

			/// <summary>
			/// Total bonus to one stat from gems socketed in equipped items. What each gem gives
			/// comes from the equipment rules JSON (EquipmentRules.Current).
			/// </summary>
			public float GetSocketBonus(SocketStat stat) =>
				GetEquippedItems().Sum(item => EquipmentRules.Current.ItemBonuses(item).GetValueOrDefault(stat));

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
				// Base Damage (10) + Strength Points (1 per point). Combat adds everything above the
				// base 10 on top of the weapon's range, so keep this small or weapons stop mattering.
				float baseDmg = 10 + StrengthPoints;

				return baseDmg + GetSocketBonus(SocketStat.Damage);
			}

			public float GetTotalMaxHealth()
			{
				// Base Health + Vitality Points (15 per point) + worn gear + gems
				float baseMax = HitPoints + (VitalityPoints * 15);
				float gearHealth = GetEquippedArmor().Sum(a => a.BonusHealth);

				return baseMax + gearHealth + GetSocketBonus(SocketStat.Health);
			}

			public float GetTotalDefense()
			{
				// Base Defense (10) + Fortitude Points (2 per point) + worn gear + gems
				float baseDefense = 10.0f + (FortitudePoints * 2);
				float gearDefense = GetEquippedArmor().Sum(a => a.Defense);

				return baseDefense + gearDefense + GetSocketBonus(SocketStat.Defense);
			}

			public int ArmorValue => (int)GetTotalDefense();

			public int GetEnemyPower() => rand.Next(DifficultyRating + 1, 2 * DifficultyRating + 3);
			public int GetLootCoins() => rand.Next(10 * DifficultyRating + 10, 15 * DifficultyRating + 50);
		}
	}
}