using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>The little picture on a building's hanging sign.</summary>
	public enum SignIcon { None, Coin, Anvil, Potion, Cross, House }

	/// <summary>
	/// A village building standing on the street's verge, `x` pixels along the room: plastered and
	/// timber-framed walls (or stone), a tiled hipped roof, a door, lit windows, a hanging sign and a smoking
	/// chimney. A forge has an open front with glowing coals and an anvil throwing sparks instead of a door.
	/// <see cref="Id"/> is what the game opens when the hero goes in.
	/// </summary>
	public sealed class Building(int x, int width, int height, uint wall, uint roof) : Prop
	{
		public string Id { get; init; } = "";
		public string Name { get; init; } = "";
		public SignIcon Sign { get; init; }
		public bool Stone { get; init; }
		public bool Forge { get; init; }
		public bool Chimney { get; init; } = true;

		public int X => x;
		public int Width => width;
		public int CenterX => x + width / 2;
		public bool Contains(int wx) => wx >= x - 2 && wx < x + width + 2;

		private int _base, _wallTop, _roofH, _doorX, _chimX, _chimTop;
		private const int DoorW = 6, OpenW = 14;

		public override void Plan(Room room)
		{
			_base = room.FloorTop + 1;
			_wallTop = _base - height;
			_roofH = Math.Clamp(width / 4, 5, 9);
			_doorX = Forge ? x + 3 : x + width / 2 - DoorW / 2;
			_chimX = x + width * 3 / 4;
			_chimTop = _wallTop - _roofH - 1;
		}

		public override void PaintBack(Room room, Canvas back)
		{
			Walls(back);
			Roof(back);
			if (Forge) ForgeFront(back); else Door(back);
			Windows(back);
			SignBoard(back);
			if (Chimney)
				for (int y = _chimTop; y < _wallTop - _roofH / 2; y++)
					for (int i = 0; i < 3; i++)
						back[_chimX + i, y] = Shade(Rgb(120, 70, 56), (y - _chimTop) % 2 == 0 ? 0.8 : 1.0 - i * 0.1);
		}

		private void Walls(Canvas back)
		{
			uint beam = Rgb(70, 46, 30);
			for (int y = _wallTop; y < _base; y++)
				for (int i = 0; i < width; i++)
				{
					double k = 0.88 + (Noise(x + i, y, 91) - 0.5) * 0.12 - (y == _wallTop ? 0.35 : 0); // eave shadow
					uint c = Shade(wall, k);
					if (Stone)
					{
						int row = (y - _wallTop) / 3, off = row % 2 * 3;
						if ((y - _wallTop) % 3 == 2 || (i + off) % 6 == 5) c = Shade(wall, 0.62);
					}
					else if (i == 0 || i == width - 1 || y == _wallTop + 1 || y == _base - 1 || y == _wallTop + height / 2)
						c = beam;
					if (i >= width - 2) c = Shade(c, 0.75); // the side away from the sunset
					back[x + i, y] = c;
				}
		}

		private void Roof(Canvas back)
		{
			for (int r = 0; r < _roofH; r++)
			{
				int y = _wallTop - _roofH + r;
				double half = width * 0.30 + (width * 0.20 + 2) * (r + 1) / _roofH;
				int from = (int)Math.Round(x + width / 2.0 - half), to = (int)Math.Round(x + width / 2.0 + half);
				for (int i = from; i < to; i++)
				{
					double k = 0.85 + (Noise(i / 3 + r % 2, r, 92) - 0.5) * 0.25;
					if (r % 2 == 1 || (i + r % 2 * 2) % 4 == 0) k *= 0.8;      // courses and tile edges
					if (r == _roofH - 1 || i == from || i == to - 1) k *= 0.7; // the outline
					if (i > x + width / 2 + width / 4) k *= 0.85;               // the shaded side
					back[i, y] = Shade(roof, k);
				}
			}
		}

		private void Door(Canvas back)
		{
			int h = Math.Min(12, height - 3);
			for (int y = _base - h; y < _base; y++)
				for (int i = 0; i < DoorW; i++)
				{
					bool frame = i == 0 || i == DoorW - 1 || y == _base - h;
					back[_doorX + i, y] = frame ? Rgb(52, 34, 22) : Shade(Rgb(110, 70, 40), i % 2 == 0 ? 0.85 : 1.0);
				}
			back[_doorX + DoorW - 2, _base - h / 2] = Rgb(230, 190, 80); // the handle
		}

		private void ForgeFront(Canvas back)
		{
			int h = Math.Min(13, height - 3);
			for (int y = _base - h; y < _base; y++)
				for (int i = 0; i < OpenW; i++)
				{
					bool frame = i == 0 || i == OpenW - 1 || y == _base - h;
					back[_doorX + i, y] = frame ? Rgb(60, 50, 46) : Rgb(24 + (y - _base + h) * 2, 16, 14);
				}
			// The anvil outside the opening.
			int ax = _doorX + OpenW + 2, ay = _base - 4;
			uint iron = Rgb(70, 70, 80), dark = Rgb(40, 40, 48);
			for (int i = 0; i < 7; i++) back[ax + i, ay] = iron;
			for (int i = 1; i < 5; i++) back[ax + i, ay + 1] = dark;
			for (int i = 2; i < 4; i++) back[ax + i, ay + 2] = dark;
			for (int i = 1; i < 6; i++) back[ax + i, ay + 3] = iron;
		}

		private void Windows(Canvas back)
		{
			int wy = _wallTop + 3;
			var spots = Forge ? new[] { x + width - 9 } : new[] { x + 3, x + width - 9 };
			foreach (int wx in spots)
			{
				if (wx + 6 > x + width || (wx < _doorX + DoorW + 1 && wx + 6 > _doorX - 1 && !Forge)) continue;
				for (int y = 0; y < 5; y++)
					for (int i = 0; i < 6; i++)
					{
						bool frame = i == 0 || i == 5 || y == 0 || y == 4, bar = i == 3 || y == 2;
						back[wx + i, wy + y] = frame ? Rgb(60, 40, 28) : bar ? Rgb(110, 70, 40) : y < 2 ? Rgb(255, 210, 120) : Rgb(236, 150, 70);
					}
			}
		}

		private void SignBoard(Canvas back)
		{
			if (Sign == SignIcon.None) return;
			int sx = Forge ? _doorX + OpenW + 1 : _doorX + DoorW + 2, sy = _wallTop + 2;
			if (sx + 7 > x + width) sx = _doorX - 9;
			back[sx + 1, sy - 1] = Rgb(40, 30, 24);
			back[sx + 5, sy - 1] = Rgb(40, 30, 24);
			for (int y = 0; y < 5; y++)
				for (int i = 0; i < 7; i++)
					back[sx + i, sy + y] = y == 0 || y == 4 || i == 0 || i == 6 ? Rgb(96, 64, 36) : Rgb(170, 128, 72);
			var (rows, colours) = Icon(Sign);
			for (int y = 0; y < rows.Length; y++)
				for (int i = 0; i < rows[y].Length; i++)
					if (colours.TryGetValue(rows[y][i], out uint c)) back[sx + 1 + i, sy + 1 + y] = c;
		}

		private static (string[] Rows, Dictionary<char, uint> Colours) Icon(SignIcon icon) => icon switch
		{
			SignIcon.Coin => (new[] { ".ooo.", "ooOoo", ".ooo." }, new() { ['o'] = Rgb(240, 200, 60), ['O'] = Rgb(170, 120, 30) }),
			SignIcon.Anvil => (new[] { "#####", ".###.", "##.##" }, new() { ['#'] = Rgb(52, 52, 62) }),
			SignIcon.Potion => (new[] { "..g..", ".rrr.", ".rrr." }, new() { ['g'] = Rgb(190, 210, 230), ['r'] = Rgb(210, 40, 70) }),
			SignIcon.Cross => (new[] { "..g..", ".ggg.", "..g.." }, new() { ['g'] = Rgb(50, 170, 80) }),
			SignIcon.House => (new[] { "..r..", ".rrr.", ".w.w." }, new() { ['r'] = Rgb(150, 50, 40), ['w'] = Rgb(240, 220, 170) }),
			_ => (Array.Empty<string>(), new()),
		};

		/// <summary>Chimney smoke, the forge's coals and the anvil's sparks.</summary>
		public override void Animate(Room room, Canvas frame, int t)
		{
			if (Chimney)
				for (int i = 0; i < 5; i++)
				{
					int age = (t + i * 9) % 45;
					int sx = _chimX + 1 + (int)Math.Round(Math.Sin(age * 0.25 + i) * 1.2 + age / 9.0);
					int sy = _chimTop - 1 - age / 4;
					uint smoke = Forge ? Rgb(70, 64, 66) : Rgb(150, 140, 150);
					if (sy >= 0) frame[sx, sy] = smoke;
					if (age < 20 && sy >= 0) frame[sx + 1, sy] = smoke;
				}
			if (!Forge) return;

			for (int i = 2; i < OpenW - 2; i++)
			{
				double glow = 0.6 + 0.4 * Math.Sin(t * 0.3 + i * 1.7);
				frame[_doorX + i, _base - 2] = Rgb(255 * glow, 110 * glow, 20);
				frame[_doorX + i, _base - 3] = Rgb(200 * glow, 60 * glow, 10);
				if (Noise(i, t / 2, 93) > 0.7) frame[_doorX + i, _base - 4] = Rgb(255, 170, 40);
			}
			if (t % 30 < 5) // a hammer blow: sparks off the anvil
				for (int s = 0; s < 4; s++)
					frame[_doorX + OpenW + 4 + (int)((Noise(s, t, 94) - 0.5) * 8), _base - 5 - (int)(Noise(s, t, 95) * 4)] = Rgb(255, 230, 120);
		}
	}
}
