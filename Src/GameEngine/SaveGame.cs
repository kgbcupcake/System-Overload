using System.Text.Json;
using System.Text.Json.Serialization;
using System_Overload.Src.GameData.Components;
using static System.Console;
namespace System_Overload.Src.GameEngine
{
	public static class SaveGame
	{
		// Points to the safe "Documents" folder defined in GameState
		private static string MasterRoot => GameState.MasterPath;

		/// <summary>
		/// Saves current GameState to the Documents folder.
		/// </summary>
		public static void Save() => GameState.Sync();

		/// <summary>
		/// Verifies the Documents folder is reachable and writable.
		/// </summary>
		public static bool RunSanityCheck()
		{
			try
			{
				if (!Directory.Exists(MasterRoot))
				{
					Directory.CreateDirectory(MasterRoot);
				}

				string testFile = Path.Combine(MasterRoot, "access_test.tmp");
				File.WriteAllText(testFile, "test");
				File.Delete(testFile);


				return true;
			}
			catch (Exception ex)
			{

				return false;
			}
		}

		public static void SaveData(string fileName, object data, string subFolder)
		{
			try
			{
				// 1. Determine the path using the subfolder (e.g., "profiles")
				string directoryPath = Path.Combine(MasterRoot, subFolder);

				// 2. Ensure the directory exists
				if (!Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}

				string fullPath = Path.Combine(directoryPath, fileName);

				// 3. Serialize the data to JSON
				var options = new JsonSerializerOptions
				{
					WriteIndented = true,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
				};
				string json = JsonSerializer.Serialize(data, options);

				// 4. Write to the folder
				File.WriteAllText(fullPath, json);
			}
			catch (Exception ex)
			{
				// Add a simple console log for debugging
				WriteLine($"[SAVE ERROR] Failed to save {fileName}: {ex.Message}");
			}
		}









	}
}