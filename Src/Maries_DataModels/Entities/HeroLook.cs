namespace Maries_DataModels
{
	/// <summary>
	/// What the hero looks like, as picked on the "Make your character" screen: pieces of the Universal
	/// LPC character catalog (assets/LPC) and the palette colour of each material. Saved with the hero.
	/// </summary>
	public class HeroLook
	{
		/// <summary>"female" or "male".</summary>
		public string BodyType { get; set; } = "female";

		/// <summary>Part type (head, expression, hair, beard, clothes, legs, shoes, wings) -> catalog item id, or "none".</summary>
		public Dictionary<string, string> Parts { get; set; } = new();

		/// <summary>Material (body = skin, hair, eye) -> palette ramp name.</summary>
		public Dictionary<string, string> Colours { get; set; } = new();

		/// <summary>Part type -> material -> ramp, for colours of one part only (shirt, pants, shoes cloth).</summary>
		public Dictionary<string, Dictionary<string, string>> PartColours { get; set; } = new();

		/// <summary>
		/// Wardrobe slot (outfit, cape, boots, gloves, belt) -> the Raven clothing piece worn there
		/// (an id from GameData/Templates/wardrobe/wardrobe.json). Its LPC layer and colour are in Parts/PartColours.
		/// </summary>
		public Dictionary<string, string> Wardrobe { get; set; } = new();
	}
}
