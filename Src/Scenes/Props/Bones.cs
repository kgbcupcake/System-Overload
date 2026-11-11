using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>A few old bones scattered on the floor.</summary>
	public sealed class Bones(int count = 7) : Prop
	{
		public override void PaintBack(Room room, Canvas back)
		{
			uint bone = Rgb(196, 190, 170), shade = Rgb(140, 134, 118);
			for (int b = 0; b < count; b++)
			{
				int x = 4 + (int)(Noise(b, 0, 31) * (room.W - 10));
				int y = room.FloorTop + 3 + (int)(Noise(b, 1, 31) * 4);
				if (y + 1 >= room.H) continue;
				for (int i = 0; i < 4; i++) back[x + i, y] = bone;
				back[x - 1, y - 1] = bone; back[x - 1, y + 1] = shade;
				back[x + 4, y - 1] = bone; back[x + 4, y + 1] = shade;
			}
		}
	}
}
