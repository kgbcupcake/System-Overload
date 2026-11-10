namespace MariesGui.Contracts
{
	/// <summary>
	/// Marks a data class as editable content in MariesGui. The category is the folder name
	/// under "Realm Data" that holds its JSON files. MariesGui matches this attribute by
	/// its full name, so the game doesn't need a reference to the MariesGui projects.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class MariesGuiContentAttribute : Attribute
	{
		public string Category { get; }
		public MariesGuiContentAttribute(string category) => Category = category;
	}
}
