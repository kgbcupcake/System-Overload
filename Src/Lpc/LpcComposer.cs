using Maries_DataModels;
using Maries_Ui;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>Helpers over a <see cref="HeroLook"/>: which item is on a part, which ramp a material is in.</summary>
	public static class LpcLook
	{
		/// <summary>Draw order of the part types for ties in height.</summary>
		public static readonly string[] Order =
			{ "body", "wings", "tail", "legs", "shoes", "clothes", "armour", "dress", "belt", "sash", "gloves", "cape", "head", "ears", "expression", "beard", "hair", "hat", "shield", "weapon" };

		/// <summary>The look a brand-new hero starts from.</summary>
		public static HeroLook Default() => new()
		{
			BodyType = "female",
			Parts = new()
			{
				["head"] = "heads_human_female", ["expression"] = "face_neutral", ["hair"] = "hair_long",
				["beard"] = LpcMeta.None, ["clothes"] = "torso_clothes_longsleeve2_scoop", ["legs"] = "legs_pants",
				["shoes"] = "feet_shoes_basic", ["wings"] = LpcMeta.None,
			},
			Colours = new() { ["body"] = "light", ["hair"] = "chestnut", ["eye"] = "blue" },
			PartColours = new()
			{
				["clothes"] = new() { ["cloth"] = "blue" },
				["legs"] = new() { ["cloth"] = "brown" },
				["shoes"] = new() { ["cloth"] = "black" },
			},
		};

		/// <summary>The item on a part. The body is the plain "body" unless the look swaps it (a skeleton's, a zombie's).</summary>
		public static string Part(HeroLook look, string type) =>
			look.Parts.TryGetValue(type, out var id) && id != "" ? id : type == "body" ? "body" : LpcMeta.None;

		public static LpcItem? Item(HeroLook look, string type) =>
			LpcCatalog.Items.TryGetValue(Part(look, type), out var item) ? item : null;

		/// <summary>The ramp a material is drawn in (the material's base when none is chosen).</summary>
		public static string ColourOf(HeroLook look, string material) =>
			look.Colours.TryGetValue(material, out var ramp) && LpcPalette.Has(material, ramp) ? ramp : LpcPalette.Base(material);

		/// <summary>The ramp a material is drawn in on one part: the part's own colour when it has one.</summary>
		public static string ColourOf(HeroLook look, string type, string material) =>
			look.PartColours.TryGetValue(type, out var own) && own.TryGetValue(material, out var ramp) && LpcPalette.Has(material, ramp)
				? ramp : ColourOf(look, material);

		/// <summary>A string that is the same for two looks that draw the same thing.</summary>
		public static string Key(HeroLook look) =>
			look.BodyType + "|" + string.Join(";", Order.Select(t => t + "=" + Part(look, t)))
			+ "|" + string.Join(";", LpcPalette.Materials.Select(m => m + "=" + ColourOf(look, m)))
			+ "|" + string.Join(";", look.PartColours.OrderBy(p => p.Key).Select(p => p.Key + ":" + string.Join(",", p.Value.Select(v => v.Key + "=" + v.Value))));
	}

	/// <summary>
	/// Draws one frame of a look: every layer of every chosen item, recoloured through the chosen palette
	/// ramps, stacked from the lowest height up, as the generator does. Standard 64 px cells only (the
	/// creator shows walking and standing, never a weapon swing).
	/// </summary>
	public static class LpcComposer
	{
		private const int S = LpcMeta.Cell;

		public record Layer(int Z, int Order, string Folder, string Variant, Dictionary<uint, uint> Map, string MapKey);

		private static readonly Dictionary<string, uint[]?> Sheets = new();
		private static readonly Dictionary<string, (int W, int H)> Sizes = new();
		private static readonly Dictionary<string, uint[]> Frames = new();

		/// <summary>The layers a look needs, back to front.</summary>
		public static List<Layer> Layers(HeroLook look)
		{
			var layers = new List<Layer>();
			var head = LpcLook.Item(look, "head");
			for (int order = 0; order < LpcLook.Order.Length; order++)
			{
				string type = LpcLook.Order[order];
				if (!LpcCatalog.Items.TryGetValue(LpcLook.Part(look, type), out var item)) continue;
				if (!item.Fits(head)) continue;

				var map = new Dictionary<uint, uint>();
				var key = new System.Text.StringBuilder();
				foreach (string material in item.Recolors)
				{
					string ramp = LpcLook.ColourOf(look, type, material);
					foreach (var pair in LpcPalette.Map(material, ramp, item.RecolorBase)) map[pair.Key] = pair.Value;
					key.Append(material).Append('=').Append(ramp).Append(';');
				}

				// Items sold as one file per colour (no recolours) take the file the part names as its "variant"
				// (a foe's wolf tail in fur_grey), else the one named like the part's cloth colour.
				string variant = "";
				if (item.Variants.Count > 0 && item.Recolors.Count == 0)
				{
					string cloth = LpcLook.ColourOf(look, type, "cloth");
					variant = look.PartColours.TryGetValue(type, out var own) && own.TryGetValue("variant", out var named) && item.Variants.Contains(named)
						? named
						: item.Variants.FirstOrDefault(v => v.Replace(' ', '_') == cloth.Replace(' ', '_')) ?? item.Variants[0];
				}

				foreach (var layer in item.Layers)
				{
					if (layer.Oversize || !layer.Paths.TryGetValue(look.BodyType, out string? folder)) continue;
					if (!Fill(ref folder, item, look)) continue;
					layers.Add(new Layer(layer.Z, order, folder, variant.Replace(' ', '_'), map, key.ToString()));
				}
			}
			layers.Sort((a, b) => a.Z != b.Z ? a.Z.CompareTo(b.Z) : a.Order.CompareTo(b.Order));
			return layers;
		}

		/// <summary>Fills a ${head}-style template in a folder from the item's replace table and the look.</summary>
		private static bool Fill(ref string folder, LpcItem item, HeroLook look)
		{
			foreach (var (slot, table) in item.Replace)
			{
				string token = "${" + slot + "}";
				if (!folder.Contains(token)) continue;
				var chosen = LpcLook.Item(look, slot);
				if (chosen == null || !table.TryGetValue(chosen.Name.Replace(' ', '_'), out string? value)) return false;
				folder = folder.Replace(token, value);
			}
			return !folder.Contains("${");
		}

		/// <summary>
		/// Frame `column` of the walk ("walk") or idle ("idle") animation facing `dir`, every layer stacked:
		/// 64x64 pixels, 0xRRGGBBAA. Cached per look.
		/// </summary>
		public static uint[] Frame(HeroLook look, string anim, string dir, int column)
		{
			string cacheKey = LpcLook.Key(look) + "#" + anim + dir + column;
			if (Frames.TryGetValue(cacheKey, out var cached)) return cached;
			if (Frames.Count > 512) Frames.Clear();

			var buffer = new uint[S * S];
			int row = Math.Max(0, Array.IndexOf(LpcMeta.Directions, dir));
			foreach (var layer in Layers(look))
			{
				string suffix = layer.Variant != "" ? "/" + layer.Variant : "";
				string path = Path.Combine(LpcMeta.Sheets, layer.Folder + anim + suffix + ".png");
				// Many clothes have no idle art: they stand in the walk's first frame instead.
				if (!File.Exists(path) && anim != "walk")
				{
					path = Path.Combine(LpcMeta.Sheets, layer.Folder + "walk" + suffix + ".png");
					Over(Sheet(path, layer), path, 0, row, buffer);
				}
				else Over(Sheet(path, layer), path, column, row, buffer);
			}
			Frames[cacheKey] = buffer;
			return buffer;
		}

		/// <summary>The PNG at `path`, recoloured through the layer's palette swap (null when missing).</summary>
		private static uint[]? Sheet(string path, Layer layer)
		{
			string key = path + "|" + layer.MapKey;
			if (Sheets.TryGetValue(key, out var px)) return px;
			var png = Png.Load(path);
			if (png == null) { Sheets[key] = null; return null; }
			px = (uint[])png.Pixels.Clone();
			if (layer.Map.Count > 0)
				for (int i = 0; i < px.Length; i++)
				{
					if ((px[i] & 0xFF) == 0) continue;
					if (layer.Map.TryGetValue(px[i] >> 8, out uint to)) px[i] = to << 8 | (px[i] & 0xFF);
				}
			Sheets[key] = px;
			Sizes[path] = (png.Width, png.Height);
			return px;
		}

		/// <summary>Lays frame (column, row) of a sheet over a 64x64 buffer, alpha over.</summary>
		private static void Over(uint[]? sheet, string path, int column, int row, uint[] into)
		{
			if (sheet == null || !Sizes.TryGetValue(path, out var size)) return;
			int x0 = column * S, y0 = row * S;
			if (x0 + S > size.W || y0 + S > size.H) return;
			for (int y = 0; y < S; y++)
				for (int x = 0; x < S; x++)
				{
					uint src = sheet[(y0 + y) * size.W + x0 + x];
					int a = (int)(src & 0xFF);
					if (a == 0) continue;
					ref uint dst = ref into[y * S + x];
					if (a == 255 || (dst & 0xFF) == 0) { dst = src; continue; }
					int ia = 255 - a;
					uint r = (uint)((((src >> 24) & 0xFF) * a + ((dst >> 24) & 0xFF) * ia) / 255);
					uint g = (uint)((((src >> 16) & 0xFF) * a + ((dst >> 16) & 0xFF) * ia) / 255);
					uint b = (uint)((((src >> 8) & 0xFF) * a + ((dst >> 8) & 0xFF) * ia) / 255);
					uint al = (uint)Math.Min(255, a + (int)(dst & 0xFF) * ia / 255);
					dst = r << 24 | g << 16 | b << 8 | al;
				}
		}

		/// <summary>
		/// Shrinks the (cx, cy, cw, ch) part of a 64x64 frame to dw x dh pixels, averaging the covered
		/// source pixels by area and alpha, so a 64 px LPC character fits a terminal panel.
		/// </summary>
		public static uint[] Shrink(uint[] src, int cx, int cy, int cw, int ch, int dw, int dh)
		{
			var dst = new uint[dw * dh];
			double sx = (double)cw / dw, sy = (double)ch / dh;
			for (int y = 0; y < dh; y++)
				for (int x = 0; x < dw; x++)
				{
					double x0 = cx + x * sx, x1 = x0 + sx, y0 = cy + y * sy, y1 = y0 + sy;
					double r = 0, g = 0, b = 0, a = 0, area = 0;
					for (int py = (int)y0; py < Math.Ceiling(y1); py++)
						for (int px = (int)x0; px < Math.Ceiling(x1); px++)
						{
							double w = (Math.Min(x1, px + 1) - Math.Max(x0, px)) * (Math.Min(y1, py + 1) - Math.Max(y0, py));
							if (w <= 0) continue;
							area += w;
							if (px < 0 || py < 0 || px >= S || py >= S) continue;
							uint p = src[py * S + px];
							double pa = (p & 0xFF) / 255.0 * w;
							r += ((p >> 24) & 0xFF) * pa; g += ((p >> 16) & 0xFF) * pa; b += ((p >> 8) & 0xFF) * pa; a += pa;
						}
					if (a <= 0) continue;
					uint ar = (uint)Math.Clamp(Math.Round(a / area * 255), 0, 255);
					dst[y * dw + x] = (uint)Math.Round(r / a) << 24 | (uint)Math.Round(g / a) << 16 | (uint)Math.Round(b / a) << 8 | ar;
				}
			return dst;
		}

		/// <summary>
		/// Like <see cref="Shrink"/> but crisp, for pixel art: each target pixel takes the most common colour among
		/// the source pixels it covers (clear when most of them are clear), so no blended in-between colours.
		/// Dark outline pixels count a little extra, so a character keeps its outline when shrunk.
		/// </summary>
		public static uint[] ShrinkCrisp(uint[] src, int cx, int cy, int cw, int ch, int dw, int dh)
		{
			var dst = new uint[dw * dh];
			double sx = (double)cw / dw, sy = (double)ch / dh;
			var counts = new Dictionary<uint, double>();
			for (int y = 0; y < dh; y++)
				for (int x = 0; x < dw; x++)
				{
					counts.Clear();
					double clear = 0, total = 0;
					int x0 = cx + (int)(x * sx), x1 = cx + Math.Max((int)(x * sx) + 1, (int)((x + 1) * sx));
					int y0 = cy + (int)(y * sy), y1 = cy + Math.Max((int)(y * sy) + 1, (int)((y + 1) * sy));
					for (int py = y0; py < y1; py++)
						for (int px = x0; px < x1; px++)
						{
							total++;
							uint p = px >= 0 && py >= 0 && px < S && py < S ? src[py * S + px] : 0;
							if ((p & 0xFF) < 128) { clear++; continue; }
							uint rgb = p | 0xFF;
							int lum = (int)((p >> 24) + ((p >> 16) & 0xFF) + ((p >> 8) & 0xFF));
							counts[rgb] = counts.GetValueOrDefault(rgb) + (lum < 150 ? 1.6 : 1.0); // keep outlines
						}
					if (counts.Count == 0 || clear > total * 0.6) continue;
					dst[y * dw + x] = counts.MaxBy(c => c.Value).Key;
				}
			return dst;
		}

		/// <summary>Forget cached sheets and frames (they can add up to tens of MB while browsing).</summary>
		public static void ReleaseAll()
		{
			Sheets.Clear();
			Sizes.Clear();
			Frames.Clear();
		}
	}
}
