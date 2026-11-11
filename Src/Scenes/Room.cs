using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// Where a scene's room leaves space, as fractions of its width: torch spots, banner spots, and stretches
	/// of back wall to keep plain (behind where the hero stands). A style's decor reads it.
	/// </summary>
	public sealed record RoomLayout(double[] Torches, double[] Banners, (double From, double To)[] KeepClear)
	{
		public static readonly RoomLayout Default = new(new[] { 0.08, 0.68, 0.94 }, new[] { 0.50, 0.78 }, Array.Empty<(double, double)>());
	}

	/// <summary>
	/// The look of a kind of room: wall and floor colours, the light's colour, the torch flame, whether the
	/// wall is laid in bricks, and the decor (props) it's dressed with. Add a style to <see cref="RoomStyles"/>
	/// and any scene can use it. An outdoor style has no wall: it leaves the space above the ground clear for
	/// the room's <see cref="Room.Backdrop"/> (sky), with a grass verge and a cobbled street below.
	/// </summary>
	public sealed record RoomStyle(
		string Name, string[] Keywords,
		uint Wall, uint Floor, uint Light, double Mortar, bool Bricks,
		uint FlameCore, uint FlameMid, uint FlameOuter,
		Action<Room, RoomLayout> Decor, bool Outdoor = false);

	/// <summary>The room styles the game knows, found by name or guessed from words (a floor's id and title).</summary>
	public static class RoomStyles
	{
		private static readonly uint WarmCore = Rgb(255, 236, 150), WarmMid = Rgb(255, 150, 40), WarmOuter = Rgb(200, 60, 20);

		private static void Torches(Room room, RoomLayout layout)
		{
			foreach (double at in layout.Torches) room.Add(new Torch(at));
		}

		public static readonly RoomStyle Crypt = new("crypt", new[] { "crypt", "tomb", "vault" },
			Rgb(78, 88, 82), Rgb(56, 60, 56), Rgb(255, 170, 90), 0.55, true, WarmCore, WarmMid, WarmOuter,
			(room, layout) => { Torches(room, layout); room.Add(new Bones()); });

		public static readonly RoomStyle Ember = new("ember", new[] { "ember", "cavern", "lava", "fire", "volcan" },
			Rgb(86, 44, 34), Rgb(52, 30, 24), Rgb(255, 120, 40), 0.5, false, WarmCore, WarmMid, WarmOuter,
			(room, _) => room.Add(new LavaVeins())); // lit by its lava, no torches

		public static readonly RoomStyle Catacombs = new("catacombs", new[] { "catacomb", "lich", "ossuary" },
			Rgb(86, 82, 72), Rgb(54, 52, 46), Rgb(110, 255, 140), 0.55, true, Rgb(200, 255, 210), Rgb(80, 230, 120), Rgb(30, 140, 70),
			(room, layout) => { Torches(room, layout); room.Add(new SkullNiches(layout.KeepClear)); });

		public static readonly RoomStyle Throne = new("throne", new[] { "throne", "dread", "king", "hall" },
			Rgb(44, 38, 48), Rgb(32, 28, 36), Rgb(255, 160, 80), 0.6, true, WarmCore, WarmMid, WarmOuter,
			(room, layout) => { Torches(room, layout); room.Add(new Banners(layout.Banners)); });

		/// <summary>A village street at dusk: grass verge, cobbles, no walls (the scene adds the buildings).</summary>
		public static readonly RoomStyle Village = new("village", new[] { "village", "town", "street" },
			Rgb(70, 110, 60), Rgb(112, 104, 96), Rgb(255, 190, 120), 0.6, false, WarmCore, WarmMid, WarmOuter,
			(_, _) => { }, Outdoor: true);

		public static IReadOnlyList<RoomStyle> All { get; } = new[] { Crypt, Ember, Catacombs, Throne, Village };

		/// <summary>The style with this name, or null.</summary>
		public static RoomStyle? Named(string? name) =>
			string.IsNullOrWhiteSpace(name) ? null : All.FirstOrDefault(s => s.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

		/// <summary>The first style whose keyword appears in the text; the crypt when none does.</summary>
		public static RoomStyle Guess(string text)
		{
			string t = text.ToLowerInvariant();
			return All.FirstOrDefault(s => s.Keywords.Any(t.Contains)) ?? Crypt;
		}
	}

	/// <summary>
	/// A painted room: back wall and floor in a style, lit by its props' lights, with props painted behind the
	/// actors (the back) and in front of them (the front, with see-through cracks). Painted once; only the
	/// props' moving bits are redrawn per frame.
	///
	/// The floor starts 9 pixels from the bottom; actors stand with their feet on <see cref="FeetLine"/>
	/// unless a prop (a stair) holds the ground higher.
	/// </summary>
	public sealed class Room
	{
		public RoomStyle Style { get; }
		public int W { get; }
		public int H { get; }
		public int FloorTop => H - 9;
		public int FeetLine => FloorTop + 4;

		public List<Prop> Props { get; } = new();

		/// <summary>Drawn behind the room where it's clear (an outdoor sky); null indoors.</summary>
		public Backdrop? Backdrop { get; set; }

		/// <summary>The wall, floor and back props.</summary>
		public Canvas Back { get; }
		/// <summary>Props in front of the actors (0 where there are none).</summary>
		public Canvas Front { get; }
		/// <summary>Front pixels that actors show through, darkened (a crack, a gap).</summary>
		public bool[] SeeThrough { get; }
		/// <summary>Back pixels a prop stands in front of the wall on (a stair): wall effects stay off them.</summary>
		public bool[] Covered { get; }

		public Room(RoomStyle style, int w, int h)
		{
			Style = style;
			W = w;
			H = h;
			Back = new Canvas(w, h);
			Front = new Canvas(w, h);
			SeeThrough = new bool[w * h];
			Covered = new bool[w * h];
		}

		public Room Add(Prop prop)
		{
			Props.Add(prop);
			return this;
		}

		/// <summary>Dresses the room with its style's decor.</summary>
		public Room Dress(RoomLayout layout)
		{
			Style.Decor(this, layout);
			return this;
		}

		/// <summary>The y an actor's feet rest on at column x.</summary>
		public int GroundAt(int x)
		{
			foreach (var p in Props)
				if (p.GroundAt(this, x) is { } g) return g;
			return FeetLine;
		}

		/// <summary>Columns of back wall props already take (wall decor keeps clear of them).</summary>
		public IEnumerable<(int From, int To)> TakenSpans() => Props.SelectMany(p => p.Spans(this));

		/// <summary>Paints the room once its props are added.</summary>
		public void Paint()
		{
			foreach (var p in Props) p.Plan(this);
			if (Style.Outdoor)
			{
				for (int y = 0; y < H; y++)
					for (int x = 0; x < W; x++)
						Back.Px[y * W + x] = OutdoorPixel(x, y);
				foreach (var p in Props) p.PaintBack(this, Back);
				foreach (var p in Props) p.PaintFront(this, Front, SeeThrough);
				return;
			}
			var lights = Props.SelectMany(p => p.Lights(this)).ToList();

			for (int y = 0; y < H; y++)
				for (int x = 0; x < W; x++)
				{
					double light = 0;
					foreach (var (lx, ly, power) in lights)
					{
						double dx = x - lx, dy = (y - ly) * 1.6;
						light += power / (1 + (dx * dx + dy * dy) / 260.0);
					}
					light = Math.Min(1.1, light);
					double ambient = 0.22 + 0.20 * y / H;
					double edge = Math.Min(1, Math.Min(x, W - 1 - x) / 10.0) * 0.25 + 0.75; // darker at the sides
					uint c = y < FloorTop ? WallPixel(x, y) : FloorPixel(x, y);
					Back.Px[y * W + x] = Tint(c, (ambient + light * 0.85) * edge, Style.Light, light * 0.18);
				}

			foreach (var p in Props) p.PaintBack(this, Back);
			foreach (var p in Props) p.PaintFront(this, Front, SeeThrough);
		}

		/// <summary>The props' moving bits (flames, glowing lava) for frame t.</summary>
		public void Animate(Canvas frame, int t)
		{
			foreach (var p in Props) p.Animate(this, frame, t);
		}

		private uint WallPixel(int x, int y)
		{
			if (!Style.Bricks)
				return Shade(Style.Wall, Noise(x / 4, y / 3, 21) * 0.5 + Noise(x, y, 22) * 0.2 + 0.55); // rough rock
			int row = y / 6, offset = row % 2 * 7;
			int bx = (x + offset) / 14;
			bool mortar = y % 6 == 5 || (x + offset) % 14 == 13;
			double brick = 0.78 + Noise(bx, row, 3) * 0.32 + (Noise(x, y, 4) - 0.5) * 0.10;
			if (y % 6 == 0) brick += 0.08; // a lit top edge on each course
			return Shade(Style.Wall, mortar ? Style.Mortar : brick);
		}

		/// <summary>Clear sky above the verge, grass on it, cobbles below.</summary>
		private uint OutdoorPixel(int x, int y)
		{
			int d = y - FloorTop;
			if (d < -1) return 0;
			if (d <= 0) return Shade(Style.Wall, 0.8 + Noise(x, y, 71) * 0.35 + (d == 0 ? -0.15 : 0)); // the verge
			int row = (d - 1) / 3, offset = row % 2 * 3;
			bool gap = (d - 1) % 3 == 2 || (x + offset) % 6 == 5;
			double k = gap ? 0.55 : 0.8 + Noise((x + offset) / 6, row, 72) * 0.3 + (Noise(x, y, 73) - 0.5) * 0.08;
			return Shade(Style.Floor, k * (0.75 + 0.25 * d / (H - FloorTop))); // a touch lighter toward you
		}

		private uint FloorPixel(int x, int y)
		{
			int d = y - FloorTop;
			if (d == 0) return Shade(Style.Floor, 1.35); // the lip where wall meets floor
			int band = d < 4 ? 0 : 1;
			int span = band == 0 ? 12 : 16, offset = band * 6;
			bool seam = d == 4 || (x + offset + d / 2) % span == 0;
			double n = 0.85 + Noise((x + offset) / span, band, 5) * 0.25 + (Noise(x, y, 6) - 0.5) * 0.12;
			return Shade(Style.Floor, seam ? 0.6 : n);
		}
	}
}
