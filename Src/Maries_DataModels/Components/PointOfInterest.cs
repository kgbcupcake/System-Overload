namespace Maries_DataModels
{
	/// <summary>
	/// Something the hero can examine at a location, loaded from the "points_of_interest" content
	/// folder. Examining it shows its story lines and reports an "examine" event with its Id. It
	/// can also hand over an item the first time, which counts for "collect_item" objectives.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("points_of_interest")]
	public class PointOfInterest
	{
		public string Id { get; set; } = "";
		/// <summary>What the look-around menu shows, e.g. "Her belongings".</summary>
		public string Name { get; set; } = "";

		/// <summary>Where it is: "home" (the hero's house), "ashford" (the village), or a floor Id.</summary>
		[MariesGui.Contracts.MariesGuiRef("Id", "floors", AlsoAllowed = "ashford,home")]
		public string Location { get; set; } = "home";

		/// <summary>Story key for what the hero sees when examining it.</summary>
		[MariesGui.Contracts.MariesGuiRef("Key", "story")]
		public string StoryKey { get; set; } = "";

		/// <summary>Optional item name given the first time it's examined.</summary>
		[MariesGui.Contracts.MariesGuiRef("Name", "items", "weapons", "armor", "gems")]
		public string GivesItem { get; set; } = "";

		/// <summary>Optional story flag that must be set before it appears. Empty = always there.</summary>
		public string RequiresFlag { get; set; } = "";
	}
}
