using System.Text.Json;
using System.Text.RegularExpressions;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>
	/// Where the Universal LPC Spritesheet Character Generator lives: a clone of its repo in assets/LPC
	/// (git-ignored, about 500 MB, read from disk), beside the built game, in the repo when running from
	/// source, or wherever DUNGEON_LPC points. Frames are 64x64 with the feet on y = 62; each animation is
	/// a PNG of its own in each layer's folder, one row per direction: up, left, down, right.
	/// Ported from MariesRPG's scenes/animations/player/frames/lpc/.
	/// </summary>
	public static class LpcMeta
	{
		public const int Cell = 64;
		public const string None = "none";
		public static readonly string[] BodyTypes = { "female", "male" };
		public static readonly string[] Directions = { "up", "left", "down", "right" };

		private static string? _root;

		/// <summary>The catalog's folder ("" when it isn't on disk).</summary>
		public static string Root => _root ??= FindRoot();

		public static string Sheets => Path.Combine(Root, "spritesheets");

		/// <summary>True when the catalog is on disk.</summary>
		public static bool Available => Root != "";

		private static string FindRoot()
		{
			var candidates = new List<string>();
			if (Environment.GetEnvironmentVariable("DUNGEON_LPC") is { Length: > 0 } env) candidates.Add(env);
			candidates.Add(Path.Combine(AppContext.BaseDirectory, "assets", "LPC"));
			candidates.Add(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "assets", "LPC"));
			foreach (var c in candidates)
			{
				string full = Path.GetFullPath(c);
				if (Directory.Exists(Path.Combine(full, "sheet_definitions")) && Directory.Exists(Path.Combine(full, "spritesheets")))
					return full;
			}
			return "";
		}
	}

	/// <summary>One layer of an item: drawn at height Z, its folder under spritesheets/ per body type.</summary>
	public record LpcLayer(int Z, Dictionary<string, string> Paths, bool Oversize);

	/// <summary>One catalog item (a hairstyle, a head, a shirt...).</summary>
	public record LpcItem(string Id, string Name, string Type, List<LpcLayer> Layers, List<string> Recolors,
		List<string> Variants, List<string> Tags, List<string> Animations, List<string> RequiredTags,
		Dictionary<string, Dictionary<string, string>> Replace)
	{
		/// <summary>True when the item can go with `other` (a face needs a human head).</summary>
		public bool Fits(LpcItem? other) => RequiredTags.All(tag => other != null && other.Tags.Contains(tag));

		public bool HasArtFor(string bodyType) => Layers.Count > 0 && Layers[0].Paths.ContainsKey(bodyType);

		/// <summary>The ramp its art is painted in when that isn't the material's usual base (a wolf head in fur_brown).</summary>
		public string RecolorBase { get; init; } = "";
	}

	/// <summary>The generator's sheet_definitions/*.json, read once into items by id and by type.</summary>
	public static class LpcCatalog
	{
		public static readonly Dictionary<string, LpcItem> Items = new();
		public static readonly Dictionary<string, List<LpcItem>> ByType = new();
		private static readonly string[] DefaultAnims = { "spellcast", "thrust", "walk", "slash", "shoot", "hurt" };

		static LpcCatalog()
		{
			if (!LpcMeta.Available) return;
			var files = Directory.GetFiles(Path.Combine(LpcMeta.Root, "sheet_definitions"), "*.json", SearchOption.AllDirectories);
			Array.Sort(files, StringComparer.Ordinal);
			foreach (string file in files)
			{
				string id = Path.GetFileNameWithoutExtension(file);
				if (id.StartsWith("meta_") || id == "meta") continue;
				LpcItem? item;
				try { item = Parse(id, file); }
				catch (Exception) { continue; }
				if (item == null) continue;
				Items[id] = item;
				if (!ByType.TryGetValue(item.Type, out var list)) ByType[item.Type] = list = new List<LpcItem>();
				list.Add(item);
			}
		}

		private static LpcItem? Parse(string id, string file)
		{
			using var doc = JsonDocument.Parse(File.ReadAllText(file));
			var root = doc.RootElement;
			if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("type_name", out var type)) return null;

			var layers = new List<LpcLayer>();
			for (int n = 1; n <= 9; n++)
			{
				if (!root.TryGetProperty("layer_" + n, out var layer)) break;
				var paths = new Dictionary<string, string>();
				foreach (var body in layer.EnumerateObject())
					if (body.Value.ValueKind == JsonValueKind.String && body.Name != "custom_animation")
						paths[body.Name] = body.Value.GetString()!;
				if (paths.Count == 0) continue;
				int z = layer.TryGetProperty("zPos", out var zp) && zp.ValueKind == JsonValueKind.Number ? zp.GetInt32() : 100;
				layers.Add(new LpcLayer(z, paths, layer.TryGetProperty("custom_animation", out _)));
			}
			if (layers.Count == 0) return null;

			var recolors = new List<string>();
			string recolorBase = "";
			if (root.TryGetProperty("recolors", out var rec) && rec.ValueKind == JsonValueKind.Object)
			{
				// "ulpc.fur_brown" -> "fur_brown"; other palette sets (lpcr.*) aren't loaded, so they keep the usual base.
				if (rec.TryGetProperty("base", out var b) && b.ValueKind == JsonValueKind.String)
				{
					string painted = b.GetString()!;
					recolorBase = painted.StartsWith("ulpc.") ? painted[5..] : painted.Contains('.') ? "" : painted;
				}
				if (rec.TryGetProperty("material", out var m)) recolors.Add(m.GetString()!);
				else
					foreach (var slot in rec.EnumerateObject())
						if (slot.Value.ValueKind == JsonValueKind.Object && slot.Value.TryGetProperty("material", out var sm))
							recolors.Add(sm.GetString()!);
			}

			string name = root.TryGetProperty("name", out var nm) && nm.ValueKind == JsonValueKind.String ? nm.GetString()! : Words(id);
			var anims = Strings(root, "animations");
			if (anims.Count == 0) anims.AddRange(DefaultAnims);

			var replace = new Dictionary<string, Dictionary<string, string>>();
			if (root.TryGetProperty("replace_in_path", out var rip) && rip.ValueKind == JsonValueKind.Object)
				foreach (var slot in rip.EnumerateObject())
				{
					var table = new Dictionary<string, string>();
					foreach (var pair in slot.Value.EnumerateObject())
						if (pair.Value.ValueKind == JsonValueKind.String) table[pair.Name] = pair.Value.GetString()!;
					replace[slot.Name] = table;
				}

			return new LpcItem(id, name, type.GetString()!, layers, recolors, Strings(root, "variants"), Strings(root, "tags"),
				anims, Strings(root, "required_tags"), replace) { RecolorBase = recolorBase };
		}

		private static List<string> Strings(JsonElement root, string key)
		{
			var list = new List<string>();
			if (root.TryGetProperty(key, out var arr) && arr.ValueKind == JsonValueKind.Array)
				foreach (var e in arr.EnumerateArray())
					if (e.ValueKind == JsonValueKind.String) list.Add(e.GetString()!);
			return list;
		}

		/// <summary>The items of a type that have art for the body type, with standard-size walk art.</summary>
		public static List<LpcItem> Of(string type, string bodyType) =>
			ByType.TryGetValue(type, out var all)
				? all.Where(i => i.HasArtFor(bodyType) && i.Animations.Contains("walk")).ToList()
				: new List<LpcItem>();

		/// <summary>The display name of an item id ("None" for none).</summary>
		public static string NameOf(string id) =>
			id == LpcMeta.None || id == "" ? "None" : Items.TryGetValue(id, out var item) ? item.Name : Words(id);

		/// <summary>"hair_braid2" -> "Hair Braid2".</summary>
		public static string Words(string id) =>
			string.Join(' ', Regex.Split(id, "[_ ]+").Where(w => w.Length > 0).Select(w => char.ToUpper(w[0]) + w[1..]));
	}

	/// <summary>
	/// The generator's palettes: a material (body, hair, cloth, eye...) has named colour ramps, and every
	/// PNG is painted in the material's base ramp. Recolouring swaps each base colour for the chosen ramp's.
	/// </summary>
	public static class LpcPalette
	{
		private static readonly Dictionary<string, Dictionary<string, List<string>>> Ramps = new();
		private static readonly Dictionary<string, string> Bases = new();
		private static readonly Dictionary<string, List<string>> NameLists = new();
		private static readonly Dictionary<string, Dictionary<uint, uint>> Maps = new();

		public static readonly string[] Materials = { "body", "hair", "cloth", "eye", "metal", "wood" };

		static LpcPalette()
		{
			if (!LpcMeta.Available) return;
			foreach (string material in Materials)
			{
				string folder = Path.Combine(LpcMeta.Root, "palette_definitions", material);
				string metaFile = Path.Combine(folder, "meta_" + material + ".json");
				string rampFile = Path.Combine(folder, material + "_ulpc.json");
				if (!File.Exists(metaFile) || !File.Exists(rampFile)) continue;
				using (var meta = JsonDocument.Parse(File.ReadAllText(metaFile)))
					Bases[material] = meta.RootElement.GetProperty("base").GetString()!;
				var ramps = new Dictionary<string, List<string>>();
				var names = new List<string>();
				using (var doc = JsonDocument.Parse(File.ReadAllText(rampFile)))
					foreach (var ramp in doc.RootElement.EnumerateObject())
					{
						ramps[ramp.Name] = ramp.Value.EnumerateArray().Select(c => c.GetString()!).ToList();
						names.Add(ramp.Name);
					}
				Ramps[material] = ramps;
				NameLists[material] = names;
			}
		}

		public static List<string> Names(string material) => NameLists.TryGetValue(material, out var n) ? n : new List<string>();
		public static string Base(string material) => Bases.TryGetValue(material, out var b) ? b : "";
		public static bool Has(string material, string ramp) => Own(material, ramp) || CrossCloth(material, ramp);

		private static bool Own(string material, string ramp) => Ramps.TryGetValue(material, out var r) && r.ContainsKey(ramp);

		/// <summary>Metal pieces (gloves, sabatons) can also be dyed in a cloth ramp, as the generator allows.</summary>
		private static bool CrossCloth(string material, string ramp) => material == "metal" && Own("cloth", ramp);

		/// <summary>The middle shade of a ramp, as "#rrggbb", for a swatch.</summary>
		public static string Swatch(string material, string ramp) =>
			Ramps.TryGetValue(material, out var r) && r.TryGetValue(ramp, out var c) && c.Count > 0 ? c[c.Count / 2] : "#808080";

		/// <summary>
		/// Base colour (0xRRGGBB) -> ramp colour for a material; empty when the ramp is the base. `from` is the ramp
		/// the art is painted in when it isn't the material's base (see <see cref="LpcItem.RecolorBase"/>).
		/// </summary>
		public static Dictionary<uint, uint> Map(string material, string ramp, string from = "")
		{
			string paintedIn = from != "" && Ramps.TryGetValue(material, out var own) && own.ContainsKey(from) ? from : Base(material);
			string key = material + ":" + paintedIn + ">" + ramp;
			if (Maps.TryGetValue(key, out var map)) return map;
			map = new Dictionary<uint, uint>();
			List<string>? to = null;
			if (Ramps.TryGetValue(material, out var ramps) && !ramps.TryGetValue(ramp, out to) && CrossCloth(material, ramp))
				to = Ramps["cloth"][ramp];
			if (ramps != null && ramps.TryGetValue(paintedIn, out var baseRamp) && to != null && ramp != paintedIn)
				for (int i = 0; i < baseRamp.Count && i < to.Count; i++)
					map[Pack(baseRamp[i])] = Pack(to[i]);
			Maps[key] = map;
			return map;
		}

		public static uint Pack(string hex) => Convert.ToUInt32(hex.TrimStart('#')[..6], 16);
	}
}
