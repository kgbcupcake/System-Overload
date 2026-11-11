using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>Glowing lava veins running down the wall; they light the room and pulse.</summary>
	public sealed class LavaVeins(int count = 7) : Prop
	{
		private readonly List<(int I, double Phase)> _veins = new();

		public override void Plan(Room room)
		{
			var cells = new HashSet<int>();
			for (int v = 0; v < count; v++)
			{
				int x = (int)(room.W * (0.06 + v * 0.84 / Math.Max(1, count - 1))) + (int)(Noise(v, 0, 7) * 6);
				int y0 = (int)(Noise(v, 1, 7) * room.FloorTop * 0.5);
				for (int y = y0; y < room.FloorTop; y++)
				{
					x += (int)Math.Round((Noise(v, y, 9) - 0.5) * 2.2);
					if (x < 1 || x >= room.W - 1) break;
					cells.Add(y * room.W + x);
					if (Noise(x, y, 11) > 0.7) cells.Add(y * room.W + x + 1);
				}
			}
			_veins.Clear();
			foreach (int i in cells) _veins.Add((i, Noise(i, 3, 5) * Math.PI * 2));
		}

		public override IEnumerable<(double, double, double)> Lights(Room room) =>
			_veins.Where(v => v.I % 9 == 0).Select(v => ((double)(v.I % room.W), (double)(v.I / room.W), 0.05));

		public override void PaintBack(Room room, Canvas back)
		{
			foreach (var (i, _) in _veins)
				if (!room.Covered[i]) back.Px[i] = Rgb(200, 70, 20); // not over a stair painted before us
		}

		public override void Animate(Room room, Canvas frame, int t)
		{
			foreach (var (i, phase) in _veins)
			{
				if (room.Covered[i]) continue;
				double glow = 0.5 + 0.5 * Math.Sin(t * 0.25 + phase + (i / room.W) * 0.3);
				frame.Px[i] = Rgb(200 + 55 * glow, 60 + 120 * glow, 10 + 40 * glow);
			}
		}
	}
}
