using System.Text.Json;
using Maries_DataModels;
using Maries_Ui;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>One Raven clothing piece: its icon, and the LPC item and colour that draw it on the hero.</summary>
	public sealed record WardrobePiece(string Id, string Name, string Slot, string Icon,
		Dictionary<string, string> Lpc, Dictionary<string, string> Colours, Dictionary<string, string>? ColoursMale)
	{
		public string LpcFor(string bodyType) => Lpc.TryGetValue(bodyType, out var id) ? id : Lpc.Values.First();

		public Dictionary<string, string> ColoursFor(string bodyType) =>
			bodyType == "male" && ColoursMale != null ? ColoursMale : Colours;
	}

	/// <summary>
	/// The Clockwork Raven clothing pack (clothes, capes, boots, gloves, belts) as things the hero can wear,
	/// read from GameData/Templates/wardrobe/wardrobe.json (made by Src/Dev/make-wardrobe.py). As in MariesRPG,
	/// each piece is a Raven icon paired with an LPC layer, so picking it really changes the sprite.
	/// The icons are drawn from assets/icons/clothing (Src/Dev/copy-clothing.sh); without them the pieces
	/// still work, just with no picture.
	/// </summary>
	public static class Wardrobe
	{
		/// <summary>Slot -> the LPC part types it owns (an outfit is a shirt or a dress, a belt a belt or a sash).</summary>
		public static readonly Dictionary<string, string[]> SlotParts = new()
		{
			["outfit"] = new[] { "clothes", "dress" },
			["cape"] = new[] { "cape" },
			["boots"] = new[] { "shoes" },
			["gloves"] = new[] { "gloves" },
			["belt"] = new[] { "belt", "sash" },
		};

		public static readonly List<WardrobePiece> Pieces = new();
		private static readonly Dictionary<string, WardrobePiece> ById = new();

		public static bool Available => Pieces.Count > 0;

		static Wardrobe()
		{
			string? file = new[]
			{
				Path.Combine(AppContext.BaseDirectory, "GameData", "Templates", "wardrobe", "wardrobe.json"),
				Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "GameData", "Templates", "wardrobe", "wardrobe.json"),
			}.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
			if (file == null) return;
			try
			{
				using var doc = JsonDocument.Parse(File.ReadAllText(file));
				foreach (var p in doc.RootElement.GetProperty("pieces").EnumerateArray())
				{
					var piece = new WardrobePiece(
						p.GetProperty("id").GetString()!, p.GetProperty("name").GetString()!,
						p.GetProperty("slot").GetString()!, p.GetProperty("icon").GetString()!,
						Map(p.GetProperty("lpc")), Map(p.GetProperty("colours")),
						p.TryGetProperty("colours_male", out var cm) ? Map(cm) : null);
					// A piece whose LPC art isn't installed can't be worn.
					if (!piece.Lpc.Values.All(LpcCatalog.Items.ContainsKey)) continue;
					Pieces.Add(piece);
					ById[piece.Id] = piece;
				}
			}
			catch (Exception)
			{
				Pieces.Clear();
				ById.Clear();
			}
		}

		private static Dictionary<string, string> Map(JsonElement e) =>
			e.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetString()!);

		public static List<WardrobePiece> Of(string slot) => Pieces.Where(p => p.Slot == slot).ToList();

		public static WardrobePiece? Get(string id) => ById.GetValueOrDefault(id);

		/// <summary>Puts a piece on (null takes the slot off), replacing whatever the slot's part types held.</summary>
		public static void Wear(HeroLook look, string slot, WardrobePiece? piece)
		{
			foreach (string type in SlotParts[slot]) look.Parts[type] = LpcMeta.None;
			look.Wardrobe.Remove(slot);
			if (piece == null) return;

			string id = piece.LpcFor(look.BodyType);
			string type0 = LpcCatalog.Items[id].Type;
			look.Parts[type0] = id;
			look.PartColours[type0] = new Dictionary<string, string>(piece.ColoursFor(look.BodyType));
			look.Wardrobe[slot] = piece.Id;
		}

		/// <summary>
		/// The piece worn in a slot: the one recorded, as long as the look still draws it (the Tailor tab can
		/// change the layers under it); null when the slot is empty or tailored by hand.
		/// </summary>
		public static WardrobePiece? Worn(HeroLook look, string slot)
		{
			if (!look.Wardrobe.TryGetValue(slot, out var id) || Get(id) is not { } piece) return null;
			string lpc = piece.LpcFor(look.BodyType);
			string type = LpcCatalog.Items.TryGetValue(lpc, out var item) ? item.Type : "";
			if (LpcLook.Part(look, type) != lpc) return null;
			return piece.ColoursFor(look.BodyType).All(c => LpcLook.ColourOf(look, type, c.Key) == c.Value) ? piece : null;
		}

		/// <summary>True when nothing the slot owns is on the hero.</summary>
		public static bool Empty(HeroLook look, string slot) =>
			SlotParts[slot].All(t => LpcLook.Part(look, t) == LpcMeta.None);

		/// <summary>The pieces worn right now, by slot (to put back on in the other body's cut).</summary>
		public static Dictionary<string, WardrobePiece> AllWorn(HeroLook look) =>
			SlotParts.Keys.Select(slot => (slot, piece: Worn(look, slot)))
				.Where(x => x.piece != null).ToDictionary(x => x.slot, x => x.piece!);

		/// <summary>The piece's icon as terminal rows (16x8 cells), or null when the icon isn't installed.</summary>
		public static string[]? Icon(WardrobePiece piece) =>
			PixelIcon.LoadFit(Path.Combine(PixelIcon.Folder, "clothing", piece.Icon));
	}
}
