using System_Overload.Src.GameData.Components;
using System_Overload.Src.GameData.Entities;
using System.Text.Json;
using static System.Console;
namespace System_Overload.Src.GameData.Loaders
{
	public static class DronesLoader
	{
		private static List<NewEnemyData>? _cache;
		private static readonly Random _rng = new();

		public static List<NewEnemyData> LoadAll()
		{
			if (_cache != null) return _cache;

			_cache = new List<NewEnemyData>();
			string dronesDir = GameState.GetGlobalPath("drones");

			if (!Directory.Exists(dronesDir)) return _cache;

			// Change: Get every .json file in the drones directory
			string[] filePaths = Directory.GetFiles(dronesDir, "*.json");

			foreach (string path in filePaths)
			{
				try
				{
					string json = File.ReadAllText(path);
					var dronesFromFile = JsonSerializer.Deserialize<List<NewEnemyData>>(json, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (dronesFromFile != null)
					{
						_cache.AddRange(dronesFromFile);
					}
				}
				catch (JsonException ex)
				{
					WriteLine($"[DRONE LOAD ERROR] Corruption in {path}: {ex.Message}");
				}
			}

			return _cache;
		}

		public static NewEnemyData GetRandom()
		{
			var drones = LoadAll();
			if (drones.Count == 0) return new NewEnemyData();
			return drones[_rng.Next(drones.Count)];
		}

		public static NewEnemyData? GetById(string id)
		{
			return LoadAll().FirstOrDefault(e => e.Id == id);
		}
	}
}