using System.Text.Json.Serialization;
namespace Maries_DataModels
{
	/// <summary>
	/// A gem that can be set into a socket on a weapon, armor or clothing. What it gives depends on
	/// the item it sits in: by default its Attributes x Power through the equipment rules'
	/// GemEffects, or its own Modifiers when it has any for that kind of item.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("gems")]
	public class GemData : ItemData
	{
		public List<EffectType> Attributes { get; set; } = new();

		public float Power { get; set; } = 0f;

		/// <summary>Optional flat bonuses that replace the attribute rules for the matching hosts.</summary>
		public List<GemModifier> Modifiers { get; set; } = new();

		public GemData() : base()
		{
			Type = "Gem";
			Rarity = ItemRarity.Uncommon;
		}
	}
}
