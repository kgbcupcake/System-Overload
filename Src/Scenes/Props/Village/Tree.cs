using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>A leafy tree on the verge (or a bare, dead one), `x` pixels along the room.</summary>
	public sealed class Tree(int x, int size = 8, bool dead = false) : Prop
	{
		public override void PaintBack(Room room, Canvas back)
		{
			int baseY = room.FloorTop + 1, trunkTop = baseY - size - 4;
			uint bark = Rgb(66, 44, 32);
			for (int y = trunkTop; y < baseY; y++)
				for (int i = -1; i <= 1; i++)
					back[x + i, y] = Shade(bark, i == 1 ? 0.7 : 1.0);

			if (dead)
			{
				// A few crooked branches.
				for (int b = 0; b < 4; b++)
				{
					int bx = x, by = trunkTop + b * 2, dir = b % 2 == 0 ? 1 : -1;
					for (int s = 0; s < 3 + b % 2 * 2; s++) back[bx += dir, by -= s % 2] = bark;
				}
				return;
			}

			int cy = trunkTop - size / 3;
			uint leaf = Rgb(52, 92, 50);
			for (int y = cy - size; y <= cy + size / 2; y++)
				for (int i = -size - 2; i <= size + 2; i++)
				{
					double dx = i / (size + 1.5), dy = (y - cy) / (double)size;
					if (dx * dx + dy * dy > 1 + (Noise(x + i, y, 101) - 0.5) * 0.4) continue;
					double k = 0.75 + Noise((x + i) / 2, y / 2, 102) * 0.35 - (y - cy) / (double)size * 0.25;
					if (i > size / 3) k *= 0.8;          // the side away from the light
					if (dy < -0.6 && i < 0) k += 0.15;   // sunset on the top left
					back[x + i, y] = Tint(leaf, k, Rgb(255, 150, 90), dy < -0.5 ? 0.06 : 0);
				}
		}
	}
}
