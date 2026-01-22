using System_Overload.Src.GameData.Components;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace System_Overload.GameEngine.Systems.Loaders
{
	public class AdminLoader
	{
		private static List<AdminData>? _cachedAdmins;
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter() }
		};

		/// <summary>
		/// Loads all Root-Admin entities from the local 'admins' directory.
		/// These entities serve as the final guardians of simulated sectors.
		/// </summary>
		public static List<AdminData> LoadAll()
		{
			if (_cachedAdmins != null)
				return _cachedAdmins;

			// Points to Documents/System_Overload/Data/admins
			var folderPath = GameState.GetGlobalPath("admins");
			Directory.CreateDirectory(folderPath);

			var admins = new List<AdminData>();
			var jsonFiles = Directory.GetFiles(folderPath, "*.json");

			foreach (var filePath in jsonFiles)
			{
				try
				{
					var jsonContent = File.ReadAllText(filePath);
					var admin = JsonSerializer.Deserialize<AdminData>(jsonContent, _jsonOptions);

					if (admin != null)
						admins.Add(admin);
				}
				catch (JsonException ex)
				{
					// Log error or handle corrupted admin profiles
					continue;
				}
			}

			_cachedAdmins = admins;
			return _cachedAdmins;
		}

		public static AdminData? GetAdminById(string adminId)
		{
			return LoadAll().FirstOrDefault(a => a.AdminID == adminId);
		}
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