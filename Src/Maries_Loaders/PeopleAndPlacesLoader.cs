using Maries_DataModels;

namespace Maries_Loaders
{
	/// <summary>
	/// Reads the people the hero can talk to ("npcs") and the things they can examine
	/// ("points_of_interest"), and finds which ones are at a given location.
	/// </summary>
	public static class PeopleAndPlacesLoader
	{
		public static List<NpcData> Npcs { get; private set; } = new();
		public static List<PointOfInterest> Places { get; private set; } = new();

		public static void LoadAll()
		{
			Npcs = UniversalLoader.LoadCollection<NpcData>("npcs")
				.Where(n => !string.IsNullOrWhiteSpace(n.Id))
				.ToList();
			Places = UniversalLoader.LoadCollection<PointOfInterest>("points_of_interest")
				.Where(p => !string.IsNullOrWhiteSpace(p.Id))
				.ToList();
		}

		/// <summary>People at a location, in file order. hasFlag decides RequiresFlag.</summary>
		public static List<NpcData> NpcsAt(string location, Func<string, bool> hasFlag) =>
			Npcs.Where(n => n.Location.Equals(location, StringComparison.OrdinalIgnoreCase)
			                && (string.IsNullOrWhiteSpace(n.RequiresFlag) || hasFlag(n.RequiresFlag)))
				.ToList();

		/// <summary>Things to examine at a location, in file order. hasFlag decides RequiresFlag.</summary>
		public static List<PointOfInterest> PlacesAt(string location, Func<string, bool> hasFlag) =>
			Places.Where(p => p.Location.Equals(location, StringComparison.OrdinalIgnoreCase)
			                  && (string.IsNullOrWhiteSpace(p.RequiresFlag) || hasFlag(p.RequiresFlag)))
				.ToList();
	}
}
