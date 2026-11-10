namespace Maries_DataModels
{
	/// <summary>
	/// One piece of narrative text, looked up by key. Every story and flavour line the game shows
	/// lives in the "story" content folder instead of in code. Lines can use {tokens} that the
	/// caller fills in, such as {name}, {class}, {enemy} or {title}.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("story")]
	public class StoryEntry
	{
		public string Key { get; set; } = "";
		public List<string> Lines { get; set; } = new();
	}
}
