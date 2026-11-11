using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>A wall torch: an iron bracket and a flickering flame in the room style's colours.</summary>
	public sealed class Torch(double at) : Prop
	{
		public int X { get; private set; }
		public int Y { get; private set; }

		public override void Plan(Room room)
		{
			X = (int)(room.W * at);
			Y = Math.Max(6, room.FloorTop / 2 - 2);
		}

		public override IEnumerable<(double, double, double)> Lights(Room room) => new[] { ((double)X, (double)Y - 2, 1.25) };

		public override void PaintBack(Room room, Canvas back)
		{
			for (int y = Y; y < Y + 4; y++)
			{
				back[X, y] = Rgb(40, 34, 30);
				back[X + 1, y] = Rgb(28, 24, 22);
			}
		}

		public override void Animate(Room room, Canvas frame, int t)
		{
			var s = room.Style;
			int lean = (int)Math.Round((Noise(X, t / 2, 51) - 0.5) * 2);
			int height = 4 + (Noise(X, t, 52) > 0.5 ? 1 : 0);
			for (int i = 0; i < height; i++)
			{
				int y = Y - 1 - i, x = X + (i >= 2 ? lean : 0);
				frame[x, y] = i == 0 ? s.FlameMid : i == 1 ? s.FlameCore : i < height - 1 ? s.FlameMid : s.FlameOuter;
				if (i < height - 2) frame[x + 1, y] = i == 0 ? s.FlameOuter : s.FlameMid;
			}
		}
	}
}
