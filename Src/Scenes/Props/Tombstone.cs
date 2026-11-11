using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// A tall cracked tombstone standing in front of the actors. Actors behind it are hidden except through
	/// the crack, where they show darkened; <see cref="CrackX"/> is where to walk to squeeze through.
	/// </summary>
	public sealed class Tombstone(double at, int height = 32) : Prop
	{
		private const int Width = 34, Corner = 11;

		public int X { get; private set; }
		public int CrackX => X + Width / 2;

		public override void Plan(Room room) => X = (int)(room.W * at);

		public override IEnumerable<(int, int)> Spans(Room room) => new[] { (X, X + Width) };

		public override void PaintFront(Room room, Canvas front, bool[] seeThrough)
		{
			int top = Math.Max(1, room.FeetLine - height), bottom = room.FeetLine, cx = CrackX;
			uint stone = Rgb(124, 128, 120);

			for (int y = top; y < bottom; y++)
				for (int x = X; x < X + Width && x < room.W; x++)
				{
					int dy = top + Corner - y;
					if (dy > 0)
					{
						int dx = x < X + Corner ? X + Corner - x : x >= X + Width - Corner ? x - (X + Width - Corner - 1) : 0;
						if (dx * dx + dy * dy > Corner * Corner) continue; // rounded top corners
					}
					double k = 0.82 + (Noise(x / 2, y / 2, 41) - 0.5) * 0.18 + (Noise(x, y, 42) - 0.5) * 0.08;
					if (x == X || y == top || (dy > 0 && x < cx)) k += 0.14;              // the lit left and top
					if (x >= X + Width - 2) k -= 0.28;                                   // the shadowed right side
					k -= 0.25 * (y - top) / (bottom - top) * (y > bottom - 6 ? 1.6 : 1); // darker low down
					uint c = Shade(stone, k);
					if (y > bottom - 5 && Noise(x, y, 43) > 0.6) c = Rgb(58, 86, 52);   // moss at the foot
					front[x, y] = c;
				}

			// Carved lines either side of the crack.
			foreach (int ly in new[] { top + 9, top + 12, top + 15 })
				for (int x = X + 5; x < X + Width - 5; x++)
					if (Math.Abs(x - cx) > 4 && ly < bottom && front[x, ly] != 0)
						front[x, ly] = Shade(stone, 0.45);

			// The crack: hairline at the top, a body's width at the foot.
			int crackTop = top + 4;
			for (int y = crackTop; y < bottom; y++)
			{
				double p = (double)(y - crackTop) / (bottom - crackTop);
				double half = 0.6 + p * 3.6;
				double mid = cx + Math.Sin(y * 0.55) * 1.4 + (Noise(0, y, 44) - 0.5) * 1.5;
				for (int x = (int)(mid - half - 1); x <= (int)(mid + half + 1); x++)
				{
					if (front[x, y] == 0) continue;
					double d = Math.Abs(x + 0.5 - mid);
					if (d <= half) { front[x, y] = Rgb(10, 9, 12); seeThrough[y * room.W + x] = true; }
					else if (d <= half + 1) front[x, y] = Shade(front[x, y], 0.55); // dark broken edge
				}
			}
		}
	}
}
