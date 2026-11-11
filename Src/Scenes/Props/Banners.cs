using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>Long red banners with gold trim and swallow-tail ends, hung from the ceiling.</summary>
	public sealed class Banners(double[] at) : Prop
	{
		private const int Width = 12;

		public override IEnumerable<(int, int)> Spans(Room room) => at.Select(f => ((int)(room.W * f), (int)(room.W * f) + Width));

		public override void PaintBack(Room room, Canvas back)
		{
			uint red = Rgb(130, 12, 26), deep = Rgb(84, 6, 16), gold = Rgb(200, 160, 60);
			int len = room.FloorTop - 8;
			foreach (double f in at)
			{
				int x0 = (int)(room.W * f);
				for (int y = 0; y < len; y++)
					for (int x = 0; x < Width; x++)
					{
						int tail = y - (len - 6);
						if (tail > 0 && Math.Abs(x - Width / 2.0 + 0.5) < tail) continue; // the swallow-tail end
						bool trim = x == 0 || x == Width - 1 || y == 1;
						back[x0 + x, y] = trim ? gold : x % 4 == 1 ? deep : red;
					}
			}
		}
	}
}
