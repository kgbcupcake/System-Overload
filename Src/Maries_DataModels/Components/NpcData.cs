namespace Maries_DataModels
{
	/// <summary>
	/// Someone the hero can talk to, loaded from the "npcs" content folder. Talking to them reports
	/// a "talk_to" event with their Id, so quest objectives can wait for a conversation. What they
	/// say comes from the quest objective being completed (its StoryKey), or their greeting if no
	/// quest needs them right now.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("npcs")]
	public class NpcData
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		/// <summary>Shown next to the name in the talk menu, e.g. "Village elder".</summary>
		public string Role { get; set; } = "";

		/// <summary>Where they can be found: "ashford" for the village, or a floor Id.</summary>
		[MariesGui.Contracts.MariesGuiRef("Id", "floors", AlsoAllowed = "ashford,home")]
		public string Location { get; set; } = "ashford";

		/// <summary>Story key for what they say when no quest needs them.</summary>
		[MariesGui.Contracts.MariesGuiRef("Key", "story")]
		public string GreetingKey { get; set; } = "";

		/// <summary>Optional story flag that must be set before they appear. Empty = always there.</summary>
		public string RequiresFlag { get; set; } = "";
	}
}
