using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// Stone steps coming down to the right from a dark doorway in the top-left corner. Holds the ground
	/// up while an actor is on it; <see cref="EndX"/> is the column the last step ends at.
	/// </summary>
	public sealed class Stair(int steps = 7, int stepW = 6, int rise = 3) : Prop
	{
		public int EndX => steps * stepW;
		private int DoorW => stepW * 3;

		public override int? GroundAt(Room room, int x) =>
			x < EndX ? room.FeetLine - (steps - Math.Max(0, x) / stepW) * rise : null;

		public override IEnumerable<(int, int)> Spans(Room room) => new[] { (0, EndX) };

		public override void PaintBack(Room room, Canvas back)
		{
			uint stone = Shade(room.Style.Wall, 1.15);

			// The dark way the stair comes from, framed in stone.
			// (Wall effects such as lava keep a column clear beyond the frame too.)
			for (int x = 0; x < DoorW + 4 && x < room.W; x++)
				for (int y = 0; y <= GroundAt(room, x); y++)
				{
					if (x < DoorW + 2) back[x, y] = x >= DoorW ? Shade(stone, x == DoorW ? 1.2 : 0.7) : Rgb(8 + x, 7 + x, 9 + x);
					room.Covered[y * room.W + x] = true;
				}

			for (int x = 0; x < EndX && x < room.W; x++)
			{
				int tread = GroundAt(room, x)!.Value + 1;
				bool riser = x % stepW == stepW - 1;
				for (int y = Math.Max(0, tread); y < room.H; y++)
				{
					double k = 0.62 + (Noise(x / 3, y / 2, 61) - 0.5) * 0.16 - 0.3 * (y - tread) / room.H;
					if (y == tread) k = 1.55;                       // the worn tread edge
					else if (y == tread + 1) k = 1.15;
					if (riser && y < tread + rise + 1) k *= 0.45;   // the face of the next step down
					if (y > tread && (y - tread) % 6 == 0) k *= 0.7; // block joints
					back[x, y] = Shade(stone, k * (0.8 + 0.2 * x / EndX));
					room.Covered[y * room.W + x] = true;
				}
			}
		}
	}
}
