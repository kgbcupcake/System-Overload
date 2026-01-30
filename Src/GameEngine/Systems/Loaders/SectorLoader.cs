using System.Text.Json;
using System_Overload.Src.GameData.Components; // For GameState
using System_Overload.Src.GameData.Entities;   // For SectorData
using static System.Console;
namespace System_Overload.GameEngine.Systems.Loaders
{
	public static class SectorLoader
	{
		// These private fields FIX the '_cachedSectors' and '_jsonOptions' errors
		private static List<SectorData>? _cachedSectors;
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};

		public static List<SectorData> LoadAll()
		{
			// If we already loaded them this session, return the cache
			if (_cachedSectors != null) return _cachedSectors;

			var allSectors = new List<SectorData>();

			// 1. PATH TO YOUR BUILD FOLDER (WSL/Visual Studio Side)
			// This looks in: bin/Debug/net8.0/GameData/Templates/sectors
			string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameData", "Templates", "sectors");

			// 2. PATH TO YOUR DOCUMENTS (D:\Documents Side)
			string documentPath = GameState.GetGlobalPath("sectors");

			// DEBUG: This will tell you exactly where the game is looking
			WriteLine($"[DEBUG] Checking Templates: {templatePath}");
			WriteLine($"[DEBUG] Checking Documents: {documentPath}");

			// Load from BOTH locations
			LoadFromDirectory(templatePath, allSectors);
			LoadFromDirectory(documentPath, allSectors);

			// De-duplicate: If a sector exists in both, only keep one
			_cachedSectors = allSectors
				.GroupBy(s => s.Id)
				.Select(g => g.First())
				.ToList();

			return _cachedSectors;
		}

		// This helper method FIXES the 'LoadFromDirectory' error
		private static void LoadFromDirectory(string folderPath, List<SectorData> list)
		{
			if (!Directory.Exists(folderPath))
			{
				WriteLine($"[DEBUG] Path not found: {folderPath}");
				return;
			}

			var jsonFiles = Directory.GetFiles(folderPath, "*.json");
			foreach (var filePath in jsonFiles)
			{
				try
				{
					string jsonContent = File.ReadAllText(filePath);
					// Try to load as a list [ { ... }, { ... } ]
					var data = JsonSerializer.Deserialize<List<SectorData>>(jsonContent, _jsonOptions);
					if (data != null)
					{
						list.AddRange(data);
						WriteLine($"[DEBUG] Successfully loaded {data.Count} sectors from {Path.GetFileName(filePath)}");
					}
				}
				catch (JsonException ex)
				{
					WriteLine($"[DEBUG] Format Error in {Path.GetFileName(filePath)}: {ex.Message}");
				}
				catch (Exception ex)
				{
					WriteLine($"[DEBUG] Unexpected Error in {Path.GetFileName(filePath)}: {ex.Message}");
				}
			}
		}

		public static void ClearCache() => _cachedSectors = null;
	}
}