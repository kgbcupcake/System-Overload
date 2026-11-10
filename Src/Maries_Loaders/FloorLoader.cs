using Maries_DataModels;   // For FloorData

namespace Maries_Loaders
{
	public static class FloorLoader
	{
		private static List<FloorData>? _cachedFloors;

		public static List<FloorData> LoadAll()
		{
			// If we already loaded them this session, return the cache
			if (_cachedFloors != null) return _cachedFloors;

			_cachedFloors = UniversalLoader.LoadCollection<FloorData>("floors");
			return _cachedFloors;
		}

		public static void ClearCache() => _cachedFloors = null;
	}
}
