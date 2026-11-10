using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Maries_Ui
{
	/// <summary>
	/// A small PNG reader for pixel art: greyscale, RGB, palette (1, 2, 4 or 8 bits), grey+alpha and
	/// RGBA, 8 bits per channel, non-interlaced, with tRNS transparency. Anything else decodes to null.
	/// Built on the runtime's zlib, so the game needs no image library.
	/// </summary>
	public sealed class Png
	{
		public int Width { get; private init; }
		public int Height { get; private init; }

		/// <summary>Pixels row by row, each packed as 0xRRGGBBAA.</summary>
		public uint[] Pixels { get; private init; } = Array.Empty<uint>();

		public uint this[int x, int y] => Pixels[y * Width + x];

		private static ReadOnlySpan<byte> Signature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

		public static Png? Load(string path)
		{
			try { return File.Exists(path) ? Decode(File.ReadAllBytes(path)) : null; }
			catch (Exception) { return null; }
		}

		public static Png? Decode(byte[] data)
		{
			if (data.Length < 8 || !data.AsSpan(0, 8).SequenceEqual(Signature)) return null;

			int width = 0, height = 0, colorType = -1, bitDepth = 0;
			byte[] palette = Array.Empty<byte>(), trns = Array.Empty<byte>();
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
						bitDepth = body[8];
						colorType = body[9];
						int interlace = body[12];
						bool depthOk = colorType is 0 or 3 ? bitDepth is 1 or 2 or 4 or 8 : bitDepth == 8;
						if (!depthOk || interlace != 0 || width <= 0 || height <= 0 || width > 8192 || height > 8192)
							return null;
						break;
					case "PLTE": palette = body.ToArray(); break;
					case "tRNS": trns = body.ToArray(); break;
					case "IDAT": idat.Write(body); break;
					case "IEND": pos = data.Length; continue;
				}
				pos += 12 + len;
			}

			int channels = colorType switch { 0 => 1, 2 => 3, 3 => 1, 4 => 2, 6 => 4, _ => 0 };
			if (channels == 0 || width == 0) return null;

			int bitsPerPixel = channels * bitDepth;
			int stride = (width * bitsPerPixel + 7) / 8;
			int bpp = Math.Max(1, bitsPerPixel / 8); // filter step in bytes
			var raw = new byte[height * (stride + 1)];
			idat.Position = 0;
			using (var z = new ZLibStream(idat, CompressionMode.Decompress))
				z.ReadExactly(raw);

			// A greyscale or RGB tRNS names one colour that is fully transparent.
			int keyGrey = colorType == 0 && trns.Length >= 2 ? BinaryPrimitives.ReadUInt16BigEndian(trns) : -1;
			uint keyRgb = colorType == 2 && trns.Length >= 6
				? (uint)(trns[1] << 16 | trns[3] << 8 | trns[5]) : 0xFFFFFFFF;
			int maxSample = (1 << bitDepth) - 1;

			var pixels = new uint[width * height];
			var prev = new byte[stride];
			var line = new byte[stride];
			for (int y = 0; y < height; y++)
			{
				int filter = raw[y * (stride + 1)];
				Array.Copy(raw, y * (stride + 1) + 1, line, 0, stride);
				Unfilter(filter, line, prev, bpp);

				for (int x = 0; x < width; x++)
				{
					byte r, g, b, a = 255;
					switch (colorType)
					{
						case 0:
						{
							int v = Sample(line, x, bitDepth);
							r = g = b = (byte)(v * 255 / maxSample);
							if (v == keyGrey) a = 0;
							break;
						}
						case 3:
						{
							int p = Sample(line, x, bitDepth);
							if (p * 3 + 2 >= palette.Length) return null;
							r = palette[p * 3]; g = palette[p * 3 + 1]; b = palette[p * 3 + 2];
							if (p < trns.Length) a = trns[p];
							break;
						}
						case 2:
							r = line[x * 3]; g = line[x * 3 + 1]; b = line[x * 3 + 2];
							if ((uint)(r << 16 | g << 8 | b) == keyRgb) a = 0;
							break;
						case 4:
							r = g = b = line[x * 2]; a = line[x * 2 + 1];
							break;
						default:
							r = line[x * 4]; g = line[x * 4 + 1]; b = line[x * 4 + 2]; a = line[x * 4 + 3];
							break;
					}
					pixels[y * width + x] = (uint)(r << 24 | g << 16 | b << 8 | a);
				}
				(prev, line) = (line, prev);
			}

			return new Png { Width = width, Height = height, Pixels = pixels };
		}

		/// <summary>The x-th sample of a row packed at 1, 2, 4 or 8 bits, high bits first.</summary>
		private static int Sample(byte[] line, int x, int bitDepth)
		{
			if (bitDepth == 8) return line[x];
			int bit = x * bitDepth;
			int shift = 8 - bitDepth - (bit & 7);
			return (line[bit >> 3] >> shift) & ((1 << bitDepth) - 1);
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
