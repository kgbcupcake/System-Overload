using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using System.Text.Json;
using static Maries_DataModels.PlayerData;

namespace DungeonAdventures.Src.GameEngine
{
	public static class LoadGame
	{
		// Points directly to Documents/DungeonAdventures/profiles
		private static string ProfilesPath => Path.Combine(GameState.MasterPath, "profiles");

		public static loadPlayer? LoadProfile(string filePath)
		{
			if (!File.Exists(filePath))
			{
				Console.WriteLine($"[LOAD FAIL] File not found: {filePath}");
				return null;
			}

			try
			{
				string json = File.ReadAllText(filePath);
				var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				return JsonSerializer.Deserialize<loadPlayer>(json, options);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LOAD ERROR] Failed to parse JSON: {ex.Message}");
				return null;
			}
		}

				public static string[] GetAllProfiles()
				{
					// 1. Ensure the directory exists first
					if (!Directory.Exists(ProfilesPath))
					{
						Directory.CreateDirectory(ProfilesPath);
						return Array.Empty<string>();
					}
		
					// 2. Simply return the files.
					// DO NOT call LoadGame.GetAllProfiles() inside here.
					return Directory.GetFiles(ProfilesPath, "*.json");
				}
		
		public static T? LoadConfig<T>(string folder, string fileName)
		{
			string targetPath = Path.Combine(GameState.GetGlobalPath(folder), fileName + ".json");

			if (!File.Exists(targetPath)) return default;

			try
			{
				string json = File.ReadAllText(targetPath);
				var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				return JsonSerializer.Deserialize<T>(json, options);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[CONFIG LOAD ERROR] {fileName}: {ex.Message}");
				return default;
			}
		}

	}
}