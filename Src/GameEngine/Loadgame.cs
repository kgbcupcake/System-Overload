using System_Overload.Src.GameData.Components;
using System.Text.Json;
using static System_Overload.Src.GameData.Entities.PlayerData;

namespace System_Overload.Src.GameEngine
{
	public static class LoadGame
	{
		// Points directly to Documents/System_Overload/profiles
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
		
				/// <summary>
				/// Loads all JSON files from a specified global folder into a List of type T.
				/// </summary>
				/// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
				/// <param name="folder">The name of the subfolder within MasterPath (e.g., "gems", "bosses").</param>
				/// <returns>A List<T> containing all deserialized objects, or an empty list if the folder doesn't exist or contains no valid files.</returns>
				public static List<T> LoadAllFromFolder<T>(string folder)
				{
					List<T> loadedData = new List<T>();
					string targetDir = GameState.GetGlobalPath(folder); // Use GameState.GetGlobalPath
		
					if (!Directory.Exists(targetDir))
					{
						// DevLog.Write($"[LOAD] Folder not found: {targetDir}", "SYSTEM"); // Optionally log
						return loadedData; // Return empty list if folder doesn't exist
					}
		
					string[] jsonFiles = Directory.GetFiles(targetDir, "*.json");
					var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		
					foreach (string filePath in jsonFiles)
					{
						try
						{
							string json = File.ReadAllText(filePath);
							T? data = JsonSerializer.Deserialize<T>(json, options);
							if (data != null)
							{
								loadedData.Add(data);
							}
						}
						catch (Exception ex)
						{
							// Log individual file errors, but continue processing other files
							Console.WriteLine($"[LOAD ERROR] Failed to load/parse {filePath}: {ex.Message}"); // Use Console for now
							// DevLog.Write($"[LOAD ERROR] Failed to load/parse {filePath}: {ex.Message}", "ERROR"); // Use DevLog if accessible
						}
					}
		
					return loadedData;
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