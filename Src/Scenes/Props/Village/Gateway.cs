using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// An old stone archway over a road that leads off into the dark, with a red glow far down it and a
	/// signpost beside it. The village's road to the dungeon.
	/// </summary>
	public sealed class Gateway(int x) : Prop
	{
		public const int Width = 24;
		private const int Pillar = 5;

		public string Id { get; init; } = "";
		public string Name { get; init; } = "";
		public int X => x;
		public int CenterX => x + Width / 2;
		public bool Contains(int wx) => wx >= x - 8 && wx < x + Width + 2;

		private int _base, _top;

		public override void Plan(Room room)
		{
			_base = room.FloorTop + 2;
			_top = Math.Max(1, _base - 27);
		}

		public override void PaintBack(Room room, Canvas back)
		{
			uint stone = Rgb(112, 108, 112);
			int archY = _top + Width / 2; // where the round top meets the pillars
			double r = Width / 2.0;
			for (int y = _top; y < _base; y++)
				for (int i = 0; i < Width; i++)
				{
					double dx = i + 0.5 - r, dy = archY - y;
					bool outside = y < archY && dx * dx + dy * dy > r * r;
					if (outside) continue;
					bool inner = i >= Pillar && i < Width - Pillar && (y >= archY || dx * dx + dy * dy < (r - Pillar) * (r - Pillar));
					if (inner)
					{
						// The passage: darker the deeper it goes.
						double depth = (double)(y - _top) / (_base - _top);
						back[x + i, y] = Rgb(14 + depth * 10, 10, 16);
						continue;
					}
					int row = (y - _top) / 3, off = row % 2 * 3;
					bool joint = (y - _top) % 3 == 2 || (i + off) % 5 == 4;
					double k = joint ? 0.55 : 0.75 + Noise(x + i, y, 111) * 0.3;
					if (i >= Width - 2) k *= 0.7;
					if (Noise(x + i, y, 112) > 0.85 && y > archY) k *= 0.8; // weathering
					back[x + i, y] = Tint(Shade(stone, k), 1.0, Rgb(60, 90, 50), Noise(x + i, y, 113) > 0.8 ? 0.4 : 0); // moss
				}

			// The signpost.
			int sx = x - 7, sy = _base - 14;
			for (int y = sy; y < _base; y++) back[sx, y] = Rgb(80, 56, 36);
			for (int i = -3; i <= 4; i++)
				for (int y = 0; y < 3; y++)
					back[sx + i, sy + 1 + y] = y == 1 && i >= -1 && i <= 2 ? Rgb(60, 36, 24) : Rgb(150, 110, 64);
			back[sx + 3, sy + 2] = Rgb(60, 36, 24); // the arrow's head
			back[sx + 5, sy + 2] = Rgb(150, 110, 64);
		}

		/// <summary>A dull red glow breathing far down the road.</summary>
		public override void Animate(Room room, Canvas frame, int t)
		{
			double glow = 0.5 + 0.5 * Math.Sin(t * 0.08);
			for (int i = Pillar + 2; i < Width - Pillar - 2; i++)
				for (int y = _base - 5; y < _base; y++)
				{
					double k = glow * (1 - Math.Abs(i - Width / 2.0) / (Width / 2.0 - Pillar)) * (y - _base + 6) / 6.0;
					frame[x + i, y] = Rgb(30 + 120 * k, 10 + 20 * k, 14);
				}
		}
	}
}
