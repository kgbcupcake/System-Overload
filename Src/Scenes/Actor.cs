using DungeonAdventures.Src.Lpc;
using Maries_DataModels;
using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// An LPC character in a scene: where it stands, which way it faces, which walk frame it's on, and how far
	/// it has sunk through the floor. Its feet rest on the room's ground at its centre column (so it follows
	/// a stair down). It hides behind front props and shows, darkened, through their see-through parts.
	/// </summary>
	public sealed class Actor
	{
		public const int Up = 0, Left = 1, Down = 2, Right = 3;

		public HeroLook Look { get; }
		/// <summary>Terminal rows tall (two pixels a row).</summary>
		public int Rows { get; }
		public int Width { get; }
		public int Height => Rows * 2;

		/// <summary>Left edge, in pixels.</summary>
		public int X { get; set; }
		public int Dir { get; set; } = Right;
		/// <summary>0 standing, 1-8 the walk cycle.</summary>
		public int Frame { get; set; }
		/// <summary>Pixels sunk below the ground; the part below the ground isn't drawn.</summary>
		public int Sink { get; set; }
		public bool Visible { get; set; } = true;
		/// <summary>Pixels nearer (+) or farther (-) than the ground line; nearer actors are drawn over farther ones.</summary>
		public int Lane { get; set; }
		/// <summary>Anything the game wants to keep with the actor (a villager's data).</summary>
		public object? Tag { get; set; }
		/// <summary>A small pointer drawn over the head in this colour (0 for none), to pick out the hero.</summary>
		public uint Marker { get; set; }
		/// <summary>A soft shadow on the ground under the feet.</summary>
		public bool Shadow { get; set; } = true;

		public int CenterX => X + Width / 2;

		private readonly Dictionary<(int Dir, int Frame), uint[]> _frames = new();

		public Actor(HeroLook look, int rows)
		{
			Look = look;
			Rows = rows;
			Width = HeroSprite.WidthFor(rows);
		}

		public void Draw(Room room, Canvas frame)
		{
			if (!Visible) return;
			var px = _frames.TryGetValue((Dir, Frame), out var cached)
				? cached
				: _frames[(Dir, Frame)] = HeroSprite.Pixels(Look, Dir, Frame, Rows, out _);

			int ground = room.GroundAt(CenterX) + Lane;
			int top = ground + 1 - Height + Sink;
			if (Shadow && Sink == 0) DrawShadow(room, frame, ground);
			if (Marker != 0 && Sink == 0) DrawMarker(frame, top);
			for (int y = 0; y < Height; y++)
			{
				int cy = top + y;
				if (cy < 0 || cy >= room.H) continue;
				if (Sink > 0 && cy >= ground) continue; // dropped through the floor
				for (int x = 0; x < Width; x++)
				{
					int cx = X + x;
					if (cx < 0 || cx >= room.W) continue;
					uint src = px[y * Width + x];
					int a = (int)(src & 0xFF);
					if (a == 0) continue;
					int i = cy * room.W + cx;
					if (room.Front.Px[i] != 0)
					{
						if (!room.SeeThrough[i]) continue; // behind the prop
						src = Shade(src, 0.55);
					}
					frame.Px[i] = Blend(src, frame.Px[i], a);
				}
			}
		}

		private void DrawShadow(Room room, Canvas frame, int ground)
		{
			int half = Width / 3;
			for (int dx = -half; dx <= half; dx++)
				for (int dy = 0; dy <= 1; dy++)
				{
					int x = CenterX + dx, y = ground + dy;
					if (!frame.Inside(x, y) || dy == 1 && Math.Abs(dx) > half - 2) continue;
					if (room.Front.Px[y * room.W + x] != 0) continue;
					frame[x, y] = Shade(frame[x, y], 0.6);
				}
		}

		/// <summary>A little downward pointer just above the head.</summary>
		private void DrawMarker(Canvas frame, int top)
		{
			int y = Math.Max(1, top - 1);
			for (int dx = -2; dx <= 2; dx++) frame[CenterX + dx, y - 2] = Marker;
			for (int dx = -1; dx <= 1; dx++) frame[CenterX + dx, y - 1] = Shade(Marker, 0.85);
			frame[CenterX, y] = Shade(Marker, 0.7);
		}
	}
}
