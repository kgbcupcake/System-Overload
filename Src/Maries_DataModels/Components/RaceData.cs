namespace Maries_DataModels
{
	/// <summary>
	/// A people the hero can be born to, loaded from the "races" content folder. The race sets which
	/// LPC heads, ears and skin colours the "Make your character" screen offers, so the look always
	/// matches the race. What the race is like is story text under StoryKey.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("races")]
	public class RaceData
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		/// <summary>Hex colour for the race's name in menus, e.g. "#C8A97E".</summary>
		public string Color { get; set; } = "#DCDCDC";

		/// <summary>Story key for the race's description (shown when picking it).</summary>
		[MariesGui.Contracts.MariesGuiRef("Key", "story")]
		public string StoryKey { get; set; } = "";

		/// <summary>Body type ("female", "male") -> the LPC head ids this race may wear. The first is the default.</summary>
		public Dictionary<string, List<string>> Heads { get; set; } = new();

		/// <summary>LPC ears item every hero of this race has (e.g. "head_ears_elven"), or empty for none.</summary>
		public string Ears { get; set; } = "";

		/// <summary>Skin ramps (LPC "body" palette names) this race may have. The first is the default.</summary>
		public List<string> Skins { get; set; } = new();
	}
}
