using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>The village well: a stone ring, two posts and a little roof.</summary>
	public sealed class Well(int x) : Prop
	{
		public override void PaintBack(Room room, Canvas back)
		{
			int baseY = room.FloorTop + 2;
			uint stone = Rgb(130, 126, 120), wood = Rgb(96, 64, 40), roof = Rgb(110, 46, 36);
			for (int y = baseY - 5; y < baseY; y++)
				for (int i = 0; i < 14; i++)
				{
					int row = (y - baseY + 5) / 2, off = row % 2 * 2;
					bool gap = (y - baseY + 5) % 2 == 1 && (i + off) % 4 == 3;
					back[x + i, y] = Shade(stone, gap ? 0.6 : 0.8 + Noise(x + i, y, 104) * 0.3 - (i > 10 ? 0.15 : 0));
				}
			for (int y = baseY - 14; y < baseY - 5; y++)
			{
				back[x + 1, y] = wood;
				back[x + 12, y] = wood;
			}
			for (int i = 3; i < 11; i++) back[x + i, baseY - 10] = Rgb(70, 50, 34); // the windlass
			back[x + 7, baseY - 9] = Rgb(110, 100, 90);
			back[x + 7, baseY - 8] = Rgb(110, 100, 90); // the rope
			for (int r = 0; r < 4; r++)
				for (int i = -1 + r; i < 15 - r; i++)
					back[x + i, baseY - 15 - (3 - r)] = Shade(roof, r == 0 ? 0.7 : 1.0 - r * 0.05);
		}
	}
}
