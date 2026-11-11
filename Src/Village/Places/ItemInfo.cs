using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;
using static DungeonAdventures.Src.Village.Places.ItemPanels;

namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>What kind of item something is, and the words and colours used for it.</summary>
	internal static class ItemInfo
	{
		public static bool IsGem(ItemData i) => i is GemData || i.Type.Equals("Gem", StringComparison.OrdinalIgnoreCase);
		public static bool IsPotion(ItemData i) => i.Type.Equals("Consumable", StringComparison.OrdinalIgnoreCase);

		public static string TypeLabel(ItemData item) => item switch
		{
			WeaponData w => $"{w.Type} weapon",
			ArmorData a => $"{a.Type.ToLower()} ({EquipmentRules.Current.SlotLabel(a.Slot).ToLower()})",
			_ when IsGem(item) => "gem",
			_ when IsPotion(item) => "potion",
			_ => item.Type.ToLower(),
		};

		public static bool IsElement(EffectType e) =>
			e is EffectType.Fire or EffectType.Ice or EffectType.Poison or EffectType.Electric;

		public static string PotionEffect(ItemData item) => item.Effect switch
		{
			EffectType.Restorative => $"Heals {item.Value} HP",
			EffectType.Speed => "Foe loses a turn (battle)",
			EffectType.Defense => "+5 defense (battle)",
			_ => "None",
		};

		public static string RarityColor(ItemRarity rarity) => rarity switch
		{
			ItemRarity.Uncommon => "#3CDC3C",
			ItemRarity.Rare => "#4DA6FF",
			ItemRarity.Epic => "#B06CFF",
			ItemRarity.Legendary => "#FF8000",
			ItemRarity.Mythic => "#FF4040",
			_ => Text,
		};

		/// <summary>"Rogue's Dagger" -> "rogues_dagger", the key used for item descriptions.</summary>
		public static string Slug(string name) =>
			Regex.Replace(name.ToLowerInvariant().Replace("'", ""), "[^a-z0-9]+", "_").Trim('_');
	}
}
