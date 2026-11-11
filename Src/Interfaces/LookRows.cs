using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Lpc;
using Maries_DataModels;
using Maries_Ui;
using Pastel;
using static System.Console;
using DungeonAdventures.Src.Village.Places;
using static DungeonAdventures.Src.Village.Places.ItemPanels;
using static DungeonAdventures.Src.Village.Places.PanelText;
using static DungeonAdventures.Src.Village.Places.ItemInfo;
using static DungeonAdventures.Src.Village.Places.ItemDetails;
using static DungeonAdventures.Src.Village.Places.PanelMenu;
using Kind = DungeonAdventures.Src.Game.Interfaces.LookRowKind;
using Row = DungeonAdventures.Src.Game.Interfaces.LookRow;

namespace DungeonAdventures.Src.Game.Interfaces
{
	internal enum LookRowKind { Name, Body, Part, Colour, PartColour, Wardrobe }

	/// <summary>One ◀ value ▶ row of the creator: a catalog part type, a palette material, or the name/body.</summary>
	internal sealed record LookRow(string Title, LookRowKind Kind, string Key, bool Optional = true);

	/// <summary>The creator's rows on each tab (LOOK, CLOTHES, TAILOR), and which of them show for a look.</summary>
	internal static class LookRows
	{
		public static readonly Row[] Look =
		{
			new("NAME", Kind.Name, ""),
			new("BODY", Kind.Body, ""),
			new("HEAD", Kind.Part, "head", Optional: false),
			new("FACE", Kind.Part, "expression"),
			new("HAIR", Kind.Part, "hair"),
			new("BEARD", Kind.Part, "beard"),
			new("WINGS", Kind.Part, "wings"),
			new("SKIN", Kind.Colour, "body"),
			new("HAIR COLOUR", Kind.Colour, "hair"),
			new("EYES", Kind.Colour, "eye"),
		};

		/// <summary>Raven clothing pieces by wardrobe slot, then the trousers (the pack has none).</summary>
		public static readonly Row[] Clothes =
		{
			new("OUTFIT", Kind.Wardrobe, "outfit"),
			new("CLOAK", Kind.Wardrobe, "cape"),
			new("BOOTS", Kind.Wardrobe, "boots"),
			new("GLOVES", Kind.Wardrobe, "gloves"),
			new("BELT", Kind.Wardrobe, "belt"),
			new("LEGS", Kind.Part, "legs"),
			new("PANTS", Kind.PartColour, "legs"),
		};

		/// <summary>The raw LPC layers, for fitting a look by hand.</summary>
		public static readonly Row[] Tailor =
		{
			new("TOP", Kind.Part, "clothes"),
			new("LEGS", Kind.Part, "legs"),
			new("SHOES", Kind.Part, "shoes"),
			new("SHIRT", Kind.PartColour, "clothes"),
			new("PANTS", Kind.PartColour, "legs"),
			new("SHOE COLOUR", Kind.PartColour, "shoes"),
		};

		public static Row[] RowsOf(int tab, HeroLook look) => tab switch
		{
			0 => VisibleLookRows(look),
			1 when Wardrobe.Available => Clothes,
			1 => Clothes.Where(r => r.Kind != Kind.Wardrobe).ToArray(),
			_ => Tailor,
		};

		public static Row[] VisibleLookRows(HeroLook look) =>
			Look.Where(r => !(r.Key == "beard" && look.BodyType != "male")
				&& !(r.Key == "expression" && !FacesFit(look))).ToArray();

		/// <summary>Faces are drawn for human heads only.</summary>
		public static bool FacesFit(HeroLook look)
		{
			var sample = LpcCatalog.Of("expression", look.BodyType).FirstOrDefault();
			return sample != null && sample.Fits(LpcLook.Item(look, "head"));
		}
	}
}
