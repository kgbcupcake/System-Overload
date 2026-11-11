using Maries_DataModels;

namespace Maries_Loaders
{
	/// <summary>Reads the playable races ("races") and backgrounds ("backgrounds"), in file order.</summary>
	public static class OriginLoader
	{
		public static List<RaceData> Races { get; private set; } = new();
		public static List<BackgroundData> Backgrounds { get; private set; } = new();

		public static void LoadAll()
		{
			Races = UniversalLoader.LoadCollection<RaceData>("races")
				.Where(r => !string.IsNullOrWhiteSpace(r.Id))
				.ToList();
			Backgrounds = UniversalLoader.LoadCollection<BackgroundData>("backgrounds")
				.Where(b => !string.IsNullOrWhiteSpace(b.Id))
				.ToList();
		}

		/// <summary>The race with this id, or null (heroes made before races have none).</summary>
		public static RaceData? Race(string? id) =>
			Races.FirstOrDefault(r => r.Id.Equals(id ?? "", StringComparison.OrdinalIgnoreCase));

		/// <summary>The background with this id, or null.</summary>
		public static BackgroundData? Background(string? id) =>
			Backgrounds.FirstOrDefault(b => b.Id.Equals(id ?? "", StringComparison.OrdinalIgnoreCase));
	}
}
