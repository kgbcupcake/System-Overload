using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>Two rows of dark niches in the wall, a skull in each, clear of torches and other props.</summary>
	public sealed class SkullNiches((double From, double To)[] keepClear) : Prop
	{
		public override void PaintBack(Room room, Canvas back)
		{
			uint dark = Rgb(18, 18, 16), skull = Rgb(200, 196, 178), eye = Rgb(30, 28, 26);
			var torches = room.Props.OfType<Torch>().Select(t => t.X).ToList();
			var taken = room.TakenSpans().ToList();
			for (int row = 0; row < 2; row++)
			{
				int y0 = 4 + row * (room.FloorTop / 2);
				for (int x0 = 4; x0 + 10 < room.W; x0 += 16)
				{
					int x1 = x0 + 10;
					if (torches.Any(t => Math.Abs(t - x0 - 5) < 6)) continue;
					if (taken.Any(s => x1 > s.From && x0 < s.To)) continue;
					if (keepClear.Any(k => x1 > room.W * k.From && x0 < room.W * k.To)) continue;
					for (int y = y0; y < y0 + 6; y++)
						for (int x = x0; x < x1; x++) back[x, y] = dark;
					int sx = x0 + 3, sy = y0 + 2;
					for (int y = sy; y < sy + 3; y++)
						for (int x = sx; x < sx + 4; x++) back[x, y] = skull;
					back[sx, sy + 1] = eye;
					back[sx + 3, sy + 1] = eye;
				}
			}
		}
	}
}
