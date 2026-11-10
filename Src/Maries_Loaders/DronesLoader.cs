using Maries_DataModels;

namespace Maries_Loaders
{
	public static class DronesLoader
	{
		private static List<NewEnemyData>? _cache;
		private static readonly Random _rng = new();

		public static List<NewEnemyData> LoadAll()
		{
			if (_cache != null) return _cache;

			_cache = UniversalLoader.LoadCollection<NewEnemyData>("drones");
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

		public static void ClearCache() => _cache = null;
	}
}
