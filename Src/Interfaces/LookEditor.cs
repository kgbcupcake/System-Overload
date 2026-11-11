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
	/// <summary>
	/// Changing a look one row at a time: what a row offers, what it's on now, stepping it, keeping the rest
	/// of the look valid afterwards, and rolling a random look.
	/// </summary>
	internal static class LookEditor
	{
		/// <summary>The skin, hair and eye ramps RANDOM picks from.</summary>
		public static readonly Dictionary<string, string[]> NaturalColours = new()
		{
			["body"] = new[] { "light", "amber", "olive", "taupe", "bronze", "brown", "black" },
			["hair"] = new[] { "ash", "platinum", "white", "gray", "blonde", "sandy", "strawberry", "gold", "ginger", "carrot",
				"redhead", "red", "light_brown", "chestnut", "dark_brown", "dark_gray", "black", "raven" },
			["eye"] = new[] { "blue", "green", "brown", "gray" },
		};
		public static readonly Random Rng = new();

		/// <summary>The choices a row offers: catalog item ids (with "none" first when optional) or ramp names.</summary>
		public static List<string> Options(HeroLook look, Row row) => row.Kind switch
		{
			Kind.Body => LpcMeta.BodyTypes.ToList(),
			Kind.Part when row.Key == "head" && RaceLook.Heads(look) is { } heads => heads.ToList(),
			Kind.Part => (row.Optional ? new List<string> { LpcMeta.None } : new List<string>())
				.Concat(LpcCatalog.Of(row.Key, look.BodyType).Select(i => i.Id)).ToList(),
			Kind.Colour when row.Key == "body" && RaceLook.Skins is { } skins => skins.ToList(),
			Kind.Colour => LpcPalette.Names(row.Key),
			Kind.PartColour => LpcPalette.Names("cloth"),
			Kind.Wardrobe => new List<string> { LpcMeta.None }.Concat(Wardrobe.Of(row.Key).Select(p => p.Id)).ToList(),
			_ => new List<string>(),
		};

		/// <summary>Shown for a slot whose layers were changed on the Tailor tab.</summary>
		public const string Tailored = "tailored";

		public static string Current(HeroLook look, Row row) => row.Kind switch
		{
			Kind.Body => look.BodyType,
			Kind.Part => LpcLook.Part(look, row.Key),
			Kind.Colour => LpcLook.ColourOf(look, row.Key),
			Kind.PartColour => LpcLook.ColourOf(look, row.Key, "cloth"),
			Kind.Wardrobe => Wardrobe.Worn(look, row.Key)?.Id ?? (Wardrobe.Empty(look, row.Key) ? LpcMeta.None : Tailored),
			_ => "",
		};

		/// <summary>Moves a row one choice back or on, and keeps the rest of the look valid.</summary>
		public static string Change(HeroLook look, Row row, int step)
		{
			var options = Options(look, row);
			if (options.Count == 0) return "Nothing to choose here.".Pastel(Muted);
			int i = options.IndexOf(Current(look, row));
			string pick = options[((i < 0 ? 0 : i + step) % options.Count + options.Count) % options.Count];
			Set(look, row, pick);
			Tidy(look);
			return "";
		}

		public static void Set(HeroLook look, Row row, string value)
		{
			switch (row.Kind)
			{
				case Kind.Body:
					// Clothing pieces come in a cut per body: put each back on in the new one.
					var worn = Wardrobe.AllWorn(look);
					look.BodyType = value;
					foreach (var (slot, piece) in worn) Wardrobe.Wear(look, slot, piece);
					break;
				case Kind.Part:
					look.Parts[row.Key] = value;
					if (row.Key == "clothes" && value != LpcMeta.None) look.Parts["dress"] = LpcMeta.None; // a top replaces a dress
					break;
				case Kind.Wardrobe:
					Wardrobe.Wear(look, row.Key, Wardrobe.Get(value));
					break;
				case Kind.Colour:
					look.Colours[row.Key] = value;
					break;
				case Kind.PartColour:
					if (!look.PartColours.TryGetValue(row.Key, out var own)) look.PartColours[row.Key] = own = new();
					own["cloth"] = value;
					break;
			}
		}

		/// <summary>
		/// After a change: a human head follows the body type, the race's head, ears and skin stay on (see
		/// <see cref="RaceLook"/>), parts with no art for the body are swapped for the first that has, a woman
		/// has no beard, and a non-human head has no face.
		/// </summary>
		public static void Tidy(HeroLook look)
		{
			string head = LpcLook.Part(look, "head");
			if (head.StartsWith("heads_human_") && (head == "heads_human_female" || head == "heads_human_male"))
				look.Parts["head"] = "heads_human_" + look.BodyType;
			RaceLook.Fit(look);
			foreach (var row in LookRows.Look.Concat(LookRows.Tailor).Where(r => r.Kind == Kind.Part))
			{
				var available = LpcCatalog.Of(row.Key, look.BodyType);
				string part = LpcLook.Part(look, row.Key);
				if (part == LpcMeta.None && row.Optional) continue;
				if (available.All(i => i.Id != part))
					look.Parts[row.Key] = row.Optional ? LpcMeta.None : available.FirstOrDefault()?.Id ?? LpcMeta.None;
			}
			if (look.BodyType != "male") look.Parts["beard"] = LpcMeta.None;
			if (!LookRows.FacesFit(look)) look.Parts["expression"] = LpcMeta.None;
			else if (LpcLook.Part(look, "expression") == LpcMeta.None && !look.Parts.ContainsKey("expression"))
				look.Parts["expression"] = "face_neutral";
		}

		/// <summary>
		/// A new random look: head, face, hair, beard, clothes and every colour. Wings stay a choice. With the
		/// wardrobe installed the hero always gets a Raven outfit and boots, and sometimes a cloak, gloves or belt.
		/// </summary>
		public static void Randomise(HeroLook look)
		{
			T Pick<T>(IList<T> list) => list[Rng.Next(list.Count)];
			foreach (var row in LookRows.Look.Concat(LookRows.Tailor))
			{
				if (row.Kind is Kind.Name or Kind.Body || row.Key == "wings") continue;
				var options = Options(look, row);
				if (row.Kind == Kind.Part && row.Optional && row.Key is "clothes" or "legs" or "shoes" or "hair")
					options.Remove(LpcMeta.None); // a random hero is never bald and never naked
				// Random rolls an ordinary person; fur, green skin, blue hair and the like are there to pick by hand.
				// With a race, its heads and skins are already the natural choices.
				if (row.Key == "head")
					options = options.Where(o => (RaceLook.Heads(look) != null || o.StartsWith("heads_human_" + look.BodyType))
						&& !o.Contains("elderly")).ToList();
				bool raceSkin = row.Key == "body" && RaceLook.Skins != null;
				if (row.Kind == Kind.Colour && !raceSkin && NaturalColours.TryGetValue(row.Key, out var natural))
					options = options.Where(natural.Contains).ToList();
				if (options.Count > 0) Set(look, row, Pick(options));
			}
			if (Rng.Next(3) > 0) look.Parts["beard"] = LpcMeta.None;
			look.Parts["dress"] = LpcMeta.None;
			foreach (string slot in Wardrobe.SlotParts.Keys) Wardrobe.Wear(look, slot, null);
			if (Wardrobe.Available)
			{
				var chance = new Dictionary<string, int> { ["outfit"] = 1, ["boots"] = 1, ["cape"] = 3, ["gloves"] = 4, ["belt"] = 2 };
				foreach (var (slot, oneIn) in chance)
					if (Rng.Next(oneIn) == 0 && Wardrobe.Of(slot) is { Count: > 0 } pieces)
						Wardrobe.Wear(look, slot, Pick(pieces));
			}
			Tidy(look);
		}
	}
}
