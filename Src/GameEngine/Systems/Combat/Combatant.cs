using Maries_DataModels;

namespace System_Overload.Src.GameEngine.Combat
{
	/// <summary>
	/// One side of a fight. Built from the player, a drone or an admin boss by EnemyFactory.
	/// </summary>
	public class Combatant
	{
		public string Name { get; }
		public int MaxHealth { get; }
		public int Health { get; set; }
		public int Level { get; set; } = 1;

		// Damage per hit is rolled between these, before defense.
		public int MinDamage { get; set; }
		public int MaxDamage { get; set; }
		public int Defense { get; set; }

		// Percent chances (0-100).
		public int Accuracy { get; set; } = 90;
		public int CritChance { get; set; } = 5;

		// Shown in the combat log, e.g. "Packet Mite bites for 6 damage".
		public string AttackVerb { get; set; } = "strikes";

		// Average hit, used by the old Attack-minus-Defense formula and the UI.
		public int Attack => (MinDamage + MaxDamage) / 2;

		public List<ItemData> Inventory { get; set; } = new List<ItemData>();

		// --- Rewards for defeating this combatant ---
		public int XpReward { get; set; }
		public int CoinReward { get; set; }

		/// <summary>Item names with a 0-1 drop chance, rolled on victory.</summary>
		public List<(string ItemName, double Chance)> Drops { get; } = new();

		// --- Temporary effects during one fight ---
		public int BonusDefense { get; set; }
		public int SkipTurns { get; set; }

		// --- Boss behaviour (only set for admins) ---
		public BossProfile? Boss { get; set; }
		public bool IsBoss => Boss != null;

		public Combatant(string name, int maxHealth, int attack, int defense)
		{
			Name = name;
			MaxHealth = maxHealth;
			Health = maxHealth;
			Defense = defense;
			// A flat attack value becomes a +/-20% damage range.
			MinDamage = Math.Max(1, (int)Math.Round(attack * 0.8));
			MaxDamage = Math.Max(MinDamage, (int)Math.Round(attack * 1.2));
		}
	}

	/// <summary>
	/// Extra behaviour for boss fights, filled from an admin's JSON.
	/// </summary>
	public class BossProfile
	{
		public List<string> AttackNames { get; } = new();
		public string SpecialAbility { get; set; } = "";

		/// <summary>The boss uses its special ability every this many turns.</summary>
		public int SpecialEvery { get; set; } = 3;

		/// <summary>Damage multiplier once the boss drops below half health.</summary>
		public double EnrageMultiplier { get; set; } = 1.25;
		public bool Enraged { get; set; }

		public string HexColor { get; set; } = "#B3001B";
		public string OnBreach { get; set; } = "";
		public string OnDamage { get; set; } = "";
		public string OnDeletion { get; set; } = "";

		public int TurnCount { get; set; }
	}
}
