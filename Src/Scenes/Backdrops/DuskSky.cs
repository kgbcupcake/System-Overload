using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// A dusk sky over a village: night blue at the top fading to ember orange at the horizon, a few early
	/// stars, a low sun, drifting clouds, and two rows of hills that scroll slower than the street.
	/// </summary>
	public sealed class DuskSky(int horizon) : Backdrop
	{
		private static readonly uint Top = Rgb(28, 26, 64), Mid = Rgb(110, 62, 104), Low = Rgb(236, 142, 86);
		private static readonly uint FarHills = Rgb(84, 56, 86), NearHills = Rgb(46, 38, 58), Cloud = Rgb(246, 176, 150);
		private static readonly uint Sun = Rgb(255, 214, 150), Star = Rgb(220, 220, 255);

		public override void Paint(Canvas view, int cameraX, int roomWidth, int t)
		{
			int w = view.W, h = view.H;
			int sunX = (int)(w * 0.72 - cameraX * 0.05), sunY = horizon - 5;
			int drift = t / 10;

			for (int y = 0; y < h; y++)
			{
				double p = Math.Clamp((double)y / Math.Max(1, horizon), 0, 1);
				uint sky = p < 0.55 ? Mix(Top, Mid, p / 0.55) : Mix(Mid, Low, (p - 0.55) / 0.45);
				for (int x = 0; x < w; x++)
				{
					uint c = sky;
					if (y < horizon / 3 && Noise(x + cameraX / 10, y, 81) > 0.985) c = Star;

					double ds = Math.Sqrt((x - sunX) * (x - sunX) + (y - sunY) * (y - sunY) * 2.0);
					if (ds < 4) c = Sun;
					else if (ds < 9) c = Mix(c, Sun, (9 - ds) / 12);

					if (IsCloud(x + cameraX / 7 - drift, y)) c = Mix(c, Cloud, 0.3);

					int far = horizon - 5 - (int)(3 * Math.Sin((x + cameraX * 0.25) * 0.045) + 2 * Noise((x + cameraX / 4) / 6, 0, 82));
					int near = horizon - 2 - (int)(2.5 * Math.Abs(Math.Sin((x + cameraX * 0.5) * 0.19)) + 2 * Noise((x + cameraX / 2) / 3, 0, 83));
					if (y >= near) c = NearHills;
					else if (y >= far) c = FarHills;

					view.Px[y * w + x] = c;
				}
			}
		}

		/// <summary>Soft streaks of cloud: a few long ellipses spread along the sky.</summary>
		private bool IsCloud(int x, int y)
		{
			int m = (x % 200 + 200) % 200; // the clouds repeat every 200 columns
			for (int i = 0; i < 4; i++)
			{
				int cx = (i * 53 + 20) % 200, cy = 3 + i * 3 % Math.Max(1, horizon / 2);
				double dx = (m - cx) / (12.0 + i * 4), dy = (y - cy) / 1.2;
				if (dx * dx + dy * dy < 1) return true;
			}
			return false;
		}

		private static uint Mix(uint a, uint b, double k)
		{
			k = Math.Clamp(k, 0, 1);
			return Rgb((a >> 24) + (((double)(b >> 24)) - (a >> 24)) * k,
				((a >> 16) & 0xFF) + ((double)((b >> 16) & 0xFF) - ((a >> 16) & 0xFF)) * k,
				((a >> 8) & 0xFF) + ((double)((b >> 8) & 0xFF) - ((a >> 8) & 0xFF)) * k);
		}
	}
}
