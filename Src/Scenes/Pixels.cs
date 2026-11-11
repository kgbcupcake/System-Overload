namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// Colour maths on 0xRRGGBBAA pixels (the format the LPC composer and PixelIcon use), and a steady noise
	/// so a painted room comes out the same every time it's shown.
	/// </summary>
	public static class Pixels
	{
		/// <summary>An opaque colour; each channel is clamped to 0-255.</summary>
		public static uint Rgb(double r, double g, double b) =>
			(uint)Math.Clamp(r, 0, 255) << 24 | (uint)Math.Clamp(g, 0, 255) << 16 | (uint)Math.Clamp(b, 0, 255) << 8 | 0xFF;

		/// <summary>The colour scaled by k (below 1 darker, above 1 lighter).</summary>
		public static uint Shade(uint c, double k) =>
			Rgb((c >> 24) * k, ((c >> 16) & 0xFF) * k, ((c >> 8) & 0xFF) * k);

		/// <summary>The colour scaled by k with `amount` of a light's colour added.</summary>
		public static uint Tint(uint c, double k, uint light, double amount) =>
			Rgb((c >> 24) * k + (light >> 24) * amount, ((c >> 16) & 0xFF) * k + ((light >> 16) & 0xFF) * amount,
				((c >> 8) & 0xFF) * k + ((light >> 8) & 0xFF) * amount);

		/// <summary>`src` laid over `dst` at the given alpha (0-255).</summary>
		public static uint Blend(uint src, uint dst, int alpha)
		{
			if (alpha >= 250) return src | 0xFF;
			int ia = 255 - alpha;
			return Rgb(((src >> 24) * alpha + (dst >> 24) * ia) / 255.0,
				(((src >> 16) & 0xFF) * alpha + ((dst >> 16) & 0xFF) * ia) / 255.0,
				(((src >> 8) & 0xFF) * alpha + ((dst >> 8) & 0xFF) * ia) / 255.0);
		}

		/// <summary>A steady 0-1 value per (x, y, seed).</summary>
		public static double Noise(int x, int y, int seed)
		{
			unchecked
			{
				uint h = (uint)(x * 374761393 + y * 668265263 + seed * 2147483647);
				h = (h ^ (h >> 13)) * 1274126177;
				h ^= h >> 16;
				return (h & 0xFFFFFF) / (double)0x1000000;
			}
		}
	}
}
