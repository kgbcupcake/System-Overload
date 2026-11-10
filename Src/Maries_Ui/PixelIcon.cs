using System.Buffers.Binary;
using System.IO.Compression;
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
				if (File.Exists(path) && Png.Decode(File.ReadAllBytes(path)) is { } img
					&& img.Width <= MaxSize && img.Height <= MaxSize)
					rows = Render(img);
			}
			catch (Exception)
			{
				rows = null; // a broken PNG shouldn't take the screen down
			}

			_cache[path] = rows;
			return rows;
		}

		private static string[] Render(Png img)
		{
			int w = img.Width, h = img.Height;
			var rows = new string[(h + 1) / 2];

			for (int y = 0; y < h; y += 2)
			{
				var sb = new StringBuilder();
				for (int x = 0; x < w; x++)
				{
					uint top = img[x, y];
					uint bottom = y + 1 < h ? img[x, y + 1] : 0;
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

		/// <summary>
		/// A minimal PNG reader for small icons: 8-bit greyscale, RGB, palette, grey+alpha and RGBA,
		/// non-interlaced. Anything else decodes to null. Built on the runtime's zlib, so no image library.
		/// </summary>
		private sealed class Png
		{
			public int Width { get; private init; }
			public int Height { get; private init; }
			private uint[] _pixels = Array.Empty<uint>(); // 0xRRGGBBAA

			public uint this[int x, int y] => _pixels[y * Width + x];

			private static ReadOnlySpan<byte> Signature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

			public static Png? Decode(byte[] data)
			{
				if (data.Length < 8 || !data.AsSpan(0, 8).SequenceEqual(Signature)) return null;

				int width = 0, height = 0, colorType = -1;
				byte[] palette = Array.Empty<byte>(), alpha = Array.Empty<byte>();
				using var idat = new MemoryStream();

				for (int pos = 8; pos + 8 <= data.Length;)
				{
					int len = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(pos));
					string type = Encoding.ASCII.GetString(data, pos + 4, 4);
					if (len < 0 || pos + 12 + len > data.Length) return null;
					var body = data.AsSpan(pos + 8, len);

					switch (type)
					{
						case "IHDR":
							width = BinaryPrimitives.ReadInt32BigEndian(body);
							height = BinaryPrimitives.ReadInt32BigEndian(body[4..]);
							int bitDepth = body[8];
							colorType = body[9];
							int interlace = body[12];
							if (bitDepth != 8 || interlace != 0 || width <= 0 || height <= 0 || width > 1024 || height > 1024)
								return null;
							break;
						case "PLTE": palette = body.ToArray(); break;
						case "tRNS": alpha = body.ToArray(); break;
						case "IDAT": idat.Write(body); break;
						case "IEND": pos = data.Length; continue;
					}
					pos += 12 + len;
				}

				int channels = colorType switch { 0 => 1, 2 => 3, 3 => 1, 4 => 2, 6 => 4, _ => 0 };
				if (channels == 0 || width == 0) return null;

				int stride = width * channels;
				var raw = new byte[height * (stride + 1)];
				idat.Position = 0;
				using (var z = new ZLibStream(idat, CompressionMode.Decompress))
					z.ReadExactly(raw);

				var pixels = new uint[width * height];
				var prev = new byte[stride];
				var line = new byte[stride];
				for (int y = 0; y < height; y++)
				{
					int filter = raw[y * (stride + 1)];
					Array.Copy(raw, y * (stride + 1) + 1, line, 0, stride);
					Unfilter(filter, line, prev, channels);

					for (int x = 0; x < width; x++)
					{
						int i = x * channels;
						byte r, g, b, a = 255;
						switch (colorType)
						{
							case 0: r = g = b = line[i]; break;
							case 2: r = line[i]; g = line[i + 1]; b = line[i + 2]; break;
							case 3:
								int p = line[i];
								if (p * 3 + 2 >= palette.Length) return null;
								r = palette[p * 3]; g = palette[p * 3 + 1]; b = palette[p * 3 + 2];
								if (p < alpha.Length) a = alpha[p];
								break;
							case 4: r = g = b = line[i]; a = line[i + 1]; break;
							default: r = line[i]; g = line[i + 1]; b = line[i + 2]; a = line[i + 3]; break;
						}
						pixels[y * width + x] = (uint)(r << 24 | g << 16 | b << 8 | a);
					}
					(prev, line) = (line, prev);
				}

				return new Png { Width = width, Height = height, _pixels = pixels };
			}

			private static void Unfilter(int filter, byte[] line, byte[] prev, int bpp)
			{
				for (int i = 0; i < line.Length; i++)
				{
					int left = i >= bpp ? line[i - bpp] : 0;
					int up = prev[i];
					int upLeft = i >= bpp ? prev[i - bpp] : 0;
					line[i] = (byte)(line[i] + filter switch
					{
						1 => left,
						2 => up,
						3 => (left + up) / 2,
						4 => Paeth(left, up, upLeft),
						_ => 0,
					});
				}
			}

			private static int Paeth(int a, int b, int c)
			{
				int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
				return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
			}
		}
	}
}
