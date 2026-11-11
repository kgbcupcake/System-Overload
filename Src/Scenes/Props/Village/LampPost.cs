using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>An iron street lamp with a flickering flame.</summary>
	public sealed class LampPost(int x) : Prop
	{
		private int _top;

		public override void PaintBack(Room room, Canvas back)
		{
			int baseY = room.FloorTop + 2;
			_top = baseY - 16;
			for (int y = _top; y < baseY; y++) back[x, y] = Rgb(34, 30, 34);
			back[x - 1, baseY - 1] = Rgb(34, 30, 34);
			back[x + 1, baseY - 1] = Rgb(34, 30, 34);
			for (int i = -1; i <= 1; i++)
			{
				back[x + i, _top - 4] = Rgb(30, 26, 30);
				back[x + i, _top] = Rgb(30, 26, 30);
			}
		}

		public override void Animate(Room room, Canvas frame, int t)
		{
			bool bright = Noise(x, t / 2, 103) > 0.3;
			for (int y = _top - 3; y < _top; y++)
				for (int i = -1; i <= 1; i++)
					frame[x + i, y] = i == 0 && y == _top - 2 ? Rgb(255, 240, 180) : bright ? Rgb(255, 190, 90) : Rgb(230, 150, 60);
		}
	}
}
