using Maries_DataModels;   // For SectorData

namespace Maries_Loaders
{
	public static class SectorLoader
	{
		private static List<SectorData>? _cachedSectors;

		public static List<SectorData> LoadAll()
		{
			// If we already loaded them this session, return the cache
			if (_cachedSectors != null) return _cachedSectors;

			_cachedSectors = UniversalLoader.LoadCollection<SectorData>("sectors");
			return _cachedSectors;
		}

		public static void ClearCache() => _cachedSectors = null;
	}
}
