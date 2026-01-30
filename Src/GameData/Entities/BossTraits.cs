namespace System_Overload.Src.GameData.Entities
{
	public class BossTrait
	{
		public TraitType Type { get; set; }
		public float Magnitude { get; set; } // e.g. 1.5f for 50% boost
		public string LoreNote { get; set; } = "";

		public BossTrait() { }
		public BossTrait(TraitType type, float mag) { Type = type; Magnitude = mag; }
	}
}
