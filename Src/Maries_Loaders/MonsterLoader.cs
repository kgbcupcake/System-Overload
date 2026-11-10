using Maries_DataModels;

namespace Maries_Loaders
{
	public static class MonsterLoader
	{
		private static List<MonsterData>? _cache;
		private static readonly Random _rng = new();

		public static List<MonsterData> LoadAll()
		{
			if (_cache != null) return _cache;

			_cache = UniversalLoader.LoadCollection<MonsterData>("monsters");
			return _cache;
		}

		public static MonsterData GetRandom()
		{
			var monsters = LoadAll();
			if (monsters.Count == 0) return new MonsterData();
			return monsters[_rng.Next(monsters.Count)];
		}

		public static MonsterData? GetById(string id)
		{
			return LoadAll().FirstOrDefault(e => e.Id == id);
		}

		public static void ClearCache() => _cache = null;
	}
}
