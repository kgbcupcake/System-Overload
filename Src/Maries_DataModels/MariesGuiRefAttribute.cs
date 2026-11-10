namespace MariesGui.Contracts
{
	/// <summary>
	/// Marks a string property (or a list of strings) as a link to content in other categories.
	/// MariesGui shows a pick-list of the valid values and warns when a value matches nothing.
	/// The game itself ignores this attribute.
	/// </summary>
	/// <example>
	/// [MariesGuiRef("Name", "items", "weapons", "armor", "gems")] links by the target's Name.
	/// [MariesGuiRef("Id", "monsters", "bosses")] links by the target's Id.
	/// [MariesGuiRef("EquipSlots.Id", "equipment")] links to the Id of any entry in the
	/// target's EquipSlots list.
	/// [MariesGuiRef("Id", "floors", When = "Type=discover_location")] only applies while the
	/// same object's Type is discover_location. Several When variants can sit on one property.
	/// </example>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class MariesGuiRefAttribute : Attribute
	{
		/// <summary>
		/// The property on the target content that this value must match, e.g. "Id" or "Name".
		/// A dotted path such as "EquipSlots.Id" looks inside a list on the target.
		/// </summary>
		public string KeyProperty { get; }

		/// <summary>The category folders the value can point into, e.g. "monsters".</summary>
		public string[] Categories { get; }

		/// <summary>
		/// Optional "Property=value": the link only applies while that property of the same object
		/// has that value (compared loosely, like quest targets).
		/// </summary>
		public string When { get; set; } = "";

		/// <summary>
		/// Compare the way quest targets are: ignoring case, spaces and hyphens counting as "_",
		/// and apostrophes dropped. Otherwise values are compared ignoring case only.
		/// </summary>
		public bool LooseMatch { get; set; }

		/// <summary>Extra values that are always valid, comma-separated, e.g. "*" or "ashford,home".</summary>
		public string AlsoAllowed { get; set; } = "";

		public MariesGuiRefAttribute(string keyProperty, params string[] categories)
		{
			KeyProperty = keyProperty;
			Categories = categories;
		}
	}
}
