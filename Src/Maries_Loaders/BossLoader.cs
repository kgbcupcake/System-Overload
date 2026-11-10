using Maries_DataModels;

namespace Maries_Loaders
{
	public static class BossLoader
	{
		private static List<BossDefinition>? _cachedBosses;

		/// <summary>
		/// Loads every boss from the shared content root.
		/// Each dungeon floor ends with one of them.
		/// </summary>
		public static List<BossDefinition> LoadAll()
		{
			if (_cachedBosses != null)
				return _cachedBosses;

			_cachedBosses = UniversalLoader.LoadCollection<BossDefinition>("bosses")
				.Where(a => !string.IsNullOrWhiteSpace(a.Id))
				.ToList();
			return _cachedBosses;
		}

		public static BossDefinition? GetBossById(string bossId)
		{
			return LoadAll().FirstOrDefault(a => a.Id == bossId);
		}

		public static void ClearCache() => _cachedBosses = null;
	}

	// Matches the boss JSON files in the bosses folder
	[MariesGui.Contracts.MariesGuiContent("bosses")]
	public class BossDefinition
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public string Rank { get; set; } = "DUNGEON_LORD";
		public int Level { get; set; } = 10;
		public int XpReward { get; set; } = 300;
		public int CoinReward { get; set; } = 200;

		public BossStats Stats { get; set; } = new();
		public BossMoves Moves { get; set; } = new();
		public BossVisuals Visuals { get; set; } = new();

		public List<BossDrop> DropTable { get; set; } = new();
		public BossDialogue Dialogue { get; set; } = new();
	}

	public class BossStats
	{
		public int Health { get; set; } // Max HP
		public int Attack { get; set; } // Attack Power
		public int Defense { get; set; } = 10; // Defense
		public double EnrageMultiplier { get; set; } // Damage multiplier once below half Health (enraged)
	}

	public class BossMoves
	{
		public string PrimaryAttack { get; set; } = "";
		public string SecondaryAttack { get; set; } = "";
		public string SpecialAbility { get; set; } = "";
		/// <summary>Uses SpecialAbility (1.5x damage, can't miss) every this many turns. 0 = never.</summary>
		public int SpecialEvery { get; set; } = 3;
	}

	public class BossVisuals
	{
		public string HexColor { get; set; } = "#FF0000";
		public double AuraIntensity { get; set; } = 0.5;
		public string ArtRef { get; set; } = "";
	}

	public class BossDrop
	{
		public string ItemID { get; set; } = "";
		public double Chance { get; set; } // 0-1, e.g. 0.35 = 35%
	}

	public class BossDialogue
	{
		public string OnEncounter { get; set; } = "";
		public string OnDamage { get; set; } = "";
		public string OnDeath { get; set; } = "";
	}
}
