using Maries_DataModels;

namespace Maries_Loaders
{
	public static class AdminLoader
	{
		private static List<AdminData>? _cachedAdmins;

		/// <summary>
		/// Loads all Root-Admin entities from the shared content root.
		/// These entities serve as the final guardians of simulated sectors.
		/// </summary>
		public static List<AdminData> LoadAll()
		{
			if (_cachedAdmins != null)
				return _cachedAdmins;

			_cachedAdmins = UniversalLoader.LoadCollection<AdminData>("admins");
			return _cachedAdmins;
		}

		public static AdminData? GetAdminById(string adminId)
		{
			return LoadAll().FirstOrDefault(a => a.AdminID == adminId);
		}

		public static void ClearCache() => _cachedAdmins = null;
	}

	// The Data Structure matching your new admin.json template
	public class AdminData
	{
		public string AdminID { get; set; } = "";
		public string SystemName { get; set; } = "";
		public string AccessLevel { get; set; } = "ROOT_ADMIN";

		public AdminStats Stats { get; set; } = new();
		public AdminCombat CombatLogic { get; set; } = new();
		public AdminVisuals Visuals { get; set; } = new();

		public List<AdminDrop> DropTable { get; set; } = new();
		public AdminDialogue Dialogue { get; set; } = new();
	}

	public class AdminStats
	{
		public int Integrity { get; set; } // HP
		public int ProcessingPower { get; set; } // Attack Power
		public double CorruptionRate { get; set; } // Speed/Difficulty Multiplier
	}

	public class AdminCombat
	{
		public string PrimaryAttack { get; set; } = "";
		public string SecondaryAttack { get; set; } = "";
		public string SpecialAbility { get; set; } = "";
	}

	public class AdminVisuals
	{
		public string HexColor { get; set; } = "#FF0000";
		public double GlitchIntensity { get; set; } = 0.5;
		public string AsciiArtRef { get; set; } = "";
	}

	public class AdminDrop
	{
		public string ItemID { get; set; } = "";
		public double Chance { get; set; }
	}

	public class AdminDialogue
	{
		public string OnBreach { get; set; } = "";
		public string OnDamage { get; set; } = "";
		public string OnDeletion { get; set; } = "";
	}
}
