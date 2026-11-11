namespace Maries_DataModels
{
	/// <summary>
	/// The life the hero led before the story starts, loaded from the "backgrounds" content folder.
	/// It doesn't lock a calling: it adds a small head start (one attribute, some gold, an item or two)
	/// and the {background} story token, so dialogue can answer to it.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("backgrounds")]
	public class BackgroundData
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		/// <summary>Hex colour for the background's name in menus.</summary>
		public string Color { get; set; } = "#DCDCDC";

		/// <summary>Story key for the background's description (shown when picking it).</summary>
		[MariesGui.Contracts.MariesGuiRef("Key", "story")]
		public string StoryKey { get; set; } = "";

		/// <summary>Attribute that gets the bonus points: Vitality, Strength, Fortitude or Wisdom. Empty for none.</summary>
		public string Attribute { get; set; } = "";
		/// <summary>Points added to Attribute on top of the ones spent in training.</summary>
		public int Points { get; set; } = 0;

		/// <summary>Gold added to the calling's starting gold.</summary>
		public int Gold { get; set; } = 0;

		/// <summary>Item names (from the items, weapons or armor data) added to the starting kit.</summary>
		public List<string> Items { get; set; } = new();
	}
}
