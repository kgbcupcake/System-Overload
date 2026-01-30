using System.Text.Json;
using System.Text.Json.Serialization;
namespace Src.GameData.Components
{
	// Note: If you want Gems to have a Name, Value, and Weight, 
	// you should inherit from ItemData like your WeaponData does.
	public class GemData : ItemData
	{
		public List<EffectType> Attributes { get; set; } = new();

		public float Power { get; set; } = 0f;




		[JsonConverter(typeof(JsonStringEnumConverter))]
		public enum StatType
		{
			FireResist,
			IceResist,
			PoisonResist,
			ElectricResist,
			Speed,
			Strength,
			Dexterity,
			CritChance,
			Damage,
			Health,
			Defense,
			Haste
			

		}
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public class StatModifier
		{
			public StatType Stat { get; set; }
			public float Amount { get; set; }
		}

	

		public GemData() : base()
		{
			Type = "Gem";
			Rarity = ItemRarity.Uncommon;
		}
	}
}