using Maries_DataModels;

namespace Maries_Loaders
{
	public static class ShopLoader
	{
		private static List<ShopDefinition>? _cache;

		public static List<ShopDefinition> LoadAll()
		{
			if (_cache != null) return _cache;

			_cache = UniversalLoader.LoadCollection<ShopDefinition>("shops");
			return _cache;
		}

		public static ShopDefinition? GetByName(string storeName)
		{
			// Callers often pass the file name ("GeneralStore") while the JSON says "General Store".
			static string Norm(string n) => n.Replace(" ", "");
			return LoadAll().FirstOrDefault(s => Norm(s.StoreName).Equals(Norm(storeName), StringComparison.OrdinalIgnoreCase));
		}

		public static void ClearCache() => _cache = null;
	}
}
