using System.Text;
using System.Text.RegularExpressions;

namespace Maries_Ui
{
	/// <summary>
	/// Draws small pixel-art PNGs (item icons) in the terminal. Each character cell holds two pixels,
	/// one above the other, using the half-block "▀"/"▄" with 24-bit foreground and background colours,
	/// so a 16x16 icon takes 16 columns by 8 rows. Transparent pixels show the terminal background.
	///
	/// Icons live in assets/icons next to the game and are named after the item:
	/// "Rogue's Dagger" -> assets/icons/rogues_dagger.png. A missing file just means no icon.
	/// </summary>
	public static class PixelIcon
	{
		/// <summary>Largest icon side in pixels. Bigger images are skipped rather than overflowing the panel.</summary>
		public const int MaxSize = 16;

		private const byte AlphaCutoff = 128;
		private static readonly Dictionary<string, string[]?> _cache = new(StringComparer.OrdinalIgnoreCase);
		private static string? _folder;

		/// <summary>The icons folder: beside the built game, or in the repo when running from source.</summary>
		public static string Folder => _folder ??= FindFolder();

		/// <summary>False when output is redirected or NO_COLOR is set; callers then lay out without icons.</summary>
		public static bool Enabled =>
			!Console.IsOutputRedirected && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

		/// <summary>
		/// The icon for an item as terminal rows (each exactly as wide as the image), or null when
		/// there is no icon file, icons are off, or the image can't be read.
		/// </summary>
		public static string[]? ForItem(string itemName) => Enabled ? Load(Path.Combine(Folder, Slug(itemName) + ".png")) : null;

		/// <summary>Visible width in columns of rows returned by <see cref="ForItem"/>.</summary>
		public static int Width(string[] rows) => rows.Length == 0 ? 0 : UiEngine.StripAnsi(rows[0]).Length;

		/// <summary>"Rogue's Dagger" -> "rogues_dagger".</summary>
		public static string Slug(string name) =>
			Regex.Replace(name.ToLowerInvariant().Replace("'", ""), "[^a-z0-9]+", "_").Trim('_');

		public static string[]? Load(string path)
		{
			if (_cache.TryGetValue(path, out var cached)) return cached;

			string[]? rows = null;
			try
			{
				if (Png.Load(path) is { } img && img.Width <= MaxSize && img.Height <= MaxSize)
					rows = Render(img.Pixels, img.Width, img.Height);
			}
			catch (Exception)
			{
				rows = null; // a broken PNG shouldn't take the screen down
			}

			_cache[path] = rows;
			return rows;
		}

		/// <summary>
		/// Turns pixels (0xRRGGBBAA, row by row) into terminal rows: two pixels per character cell, using
		/// "▀"/"▄" with 24-bit colours. Each row is exactly <paramref name="w"/> columns wide.
		/// </summary>
		public static string[] Render(uint[] px, int w, int h)
		{
			var rows = new string[(h + 1) / 2];

			for (int y = 0; y < h; y += 2)
			{
				var sb = new StringBuilder();
				for (int x = 0; x < w; x++)
				{
					uint top = px[y * w + x];
					uint bottom = y + 1 < h ? px[(y + 1) * w + x] : 0;
					bool hasTop = (top & 0xFF) >= AlphaCutoff, hasBottom = (bottom & 0xFF) >= AlphaCutoff;

					if (hasTop && hasBottom)
						sb.Append(Fg(top)).Append(Bg(bottom)).Append('▀');
					else if (hasTop)
						sb.Append(Fg(top)).Append("\x1b[49m▀");
					else if (hasBottom)
						sb.Append(Fg(bottom)).Append("\x1b[49m▄");
					else
						sb.Append("\x1b[0m ");
				}
				sb.Append("\x1b[0m");
				rows[y / 2] = sb.ToString();
			}
			return rows;
		}

		private static string Fg(uint rgba) => $"\x1b[38;2;{rgba >> 24};{(rgba >> 16) & 0xFF};{(rgba >> 8) & 0xFF}m";
		private static string Bg(uint rgba) => $"\x1b[48;2;{rgba >> 24};{(rgba >> 16) & 0xFF};{(rgba >> 8) & 0xFF}m";

		private static string FindFolder()
		{
			string beside = Path.Combine(AppContext.BaseDirectory, "assets", "icons");
			if (Directory.Exists(beside)) return beside;
			// Running from bin/Debug/net10.0 inside the repo.
			return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "assets", "icons"));
		}
	}
}
