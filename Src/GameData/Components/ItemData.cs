using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Src.GameData.Components
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary, Mythic, Ethereal, Armored, Enraged }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum EffectType { None, Fire, Ice, Poison, Electric, Restorative, Speed, Defense, Ethereal, Armored, Enraged, LifeLeech }
	public enum GlobalAura { None, Decay, Frost, Static }
	public enum AttachmentSlot { None, Optics, Barrel, Underbarrel, Magazine }

	[JsonDerivedType(typeof(ItemData), typeDiscriminator: "base")]
	[JsonDerivedType(typeof(WeaponData), typeDiscriminator: "weapon")]
	[JsonDerivedType(typeof(GemData), typeDiscriminator: "gem")]
	public class ItemData
	{
		public string Name { get; set; } = "Default Item";
		public int Value { get; set; } = 0;
		public float Weight { get; set; } = 0.0f;
		public int Amount { get; set; } = 1;
		public string Type { get; set; } = "Misc";

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ItemRarity Rarity { get; set; } = ItemRarity.Common;
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public EffectType Effect { get; set; } = EffectType.None;
		public int Durability { get; set; } = 100;
		public int LevelReq { get; set; } = 0;
		public bool IsQuestItem { get; set; } = false;
		public int IconId { get; set; } = 0;

		public int NumSockets { get; set; } = 0;
		public List<GemData> SocketedGems { get; set; } = new List<GemData>();

		// 1. Parameterless Constructor (Required for JSON)
		public ItemData() { }

		// 2. Copy Constructor (Used for cloning items in the Store)
		public ItemData(ItemData other)
		{
			if (other == null) return;
			Name = other.Name;
			Value = other.Value;
			Weight = other.Weight;
			Amount = other.Amount;
			Type = other.Type;
			Rarity = other.Rarity;
			Effect = other.Effect;
			Durability = other.Durability;
			LevelReq = other.LevelReq;
			IsQuestItem = other.IsQuestItem;
			IconId = other.IconId;
			NumSockets = other.NumSockets;
			SocketedGems = new List<GemData>(other.SocketedGems);
		}

		// 3. Full Constructor
		public ItemData(string name, int value, float weight, int amount, string type, ItemRarity rarity, EffectType effect, int durability, int levelReq, bool isQuestItem, int iconId)
		{
			Name = name;
			Value = value;
			Weight = weight;
			Amount = amount;
			Type = type;
			Rarity = rarity;
			Effect = effect;
			Durability = durability;
			LevelReq = levelReq;
			IsQuestItem = isQuestItem;
			IconId = iconId;
		}

		public bool HasEmptySocket() => SocketedGems.Count < NumSockets;

		public override string ToString()
		{
			string stackText = (Amount > 1) ? $" x{Amount}" : "";
			string socketText = NumSockets > 0 ? $" | Sockets: {SocketedGems.Count}/{NumSockets}" : "";
			return $"[{Rarity}] {Name} ({Type}){stackText} | Effect: {Effect} | Value: {Value * Amount}g{socketText}";
		}
	}
}