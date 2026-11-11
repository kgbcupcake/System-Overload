using DungeonAdventures.Src.Lpc;
using Maries_DataModels;
using Maries_Ui;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// A full-screen pixel scene: title bar on row 0, a painted <see cref="Room"/> with <see cref="Actor"/>s
	/// below it, and a <see cref="StoryBox"/> along the bottom.
	///
	///   row 0           title bar
	///   rows 1..A       the room, two pixels per row, A = height - 5
	///   rows A+1..H-1   the story box
	///
	/// The room can be wider than the screen (a street): <see cref="CameraX"/> picks the part shown, and the
	/// room's <see cref="Room.Backdrop"/> fills whatever the room leaves clear (the sky), scrolled slower.
	///
	/// Two ways to drive it. Scripted: <see cref="Play"/> a script of <see cref="WalkTo"/>, <see cref="SinkOut"/>,
	/// <see cref="Pause"/> and <see cref="Say"/>; any key skips the moving steps. Live: <see cref="Open"/> it,
	/// then each frame move things, <see cref="Render"/>, <see cref="Advance"/> (see the village).
	/// </summary>
	public sealed class Scene
	{
		public const int FrameMs = 60;

		public Room Room { get; }
		public StoryBox Box { get; }
		public List<Actor> Actors { get; } = new();
		/// <summary>Screen columns (and view pixels across).</summary>
		public int Width { get; }
		public int Height { get; }
		/// <summary>The room column shown at the left edge of the screen.</summary>
		public int CameraX { get; set; }
		/// <summary>Frames so far; props animate from it.</summary>
		public int Time => _t;

		private readonly Action _titleBar;
		private readonly CanvasScreen _screen = new(1);
		private readonly Canvas _frame, _view;
		private bool _skipping;
		private int _t;

		/// <summary>True when scenes can be drawn: colour output and the LPC catalog are there.</summary>
		public static bool Available => PixelIcon.Enabled && LpcMeta.Available;

		/// <summary>True when this look can be put in a scene.</summary>
		public static bool CanCast(HeroLook? look) => Available && HeroSprite.CanDraw(look);

		/// <param name="worldWidth">The room's width in pixels when it's wider than the screen.</param>
		public Scene(RoomStyle style, Action titleBar, int? worldWidth = null)
		{
			_titleBar = titleBar;
			Width = Math.Max(80, SafeWidth());
			Height = Math.Max(20, SafeHeight());
			int rows = Height - 1 - StoryBox.Rows;
			Room = new Room(style, Math.Max(Width, worldWidth ?? Width), rows * 2);
			Box = new StoryBox(Width, Height - StoryBox.Rows);
			_frame = new Canvas(Room.W, Room.H);
			_view = new Canvas(Width, Room.H);
		}

		/// <summary>The usual hero size for this screen: as tall as the room allows, up to 15 rows.</summary>
		public int ActorRows => Math.Clamp((Room.H - 8) / 2 * 2, 16, 30) / 2;

		public Scene Add(Prop prop)
		{
			Room.Add(prop);
			return this;
		}

		public Actor Cast(HeroLook look, int x, int? rows = null)
		{
			var actor = new Actor(look, rows ?? ActorRows) { X = x };
			Actors.Add(actor);
			return actor;
		}

		/// <summary>Keeps x in the middle of the screen, stopping at the room's ends.</summary>
		public void Follow(int x) => CameraX = Math.Clamp(x - Width / 2, 0, Room.W - Width);

		/// <summary>Paints the room and draws the whole screen with `firstPage` in the box (without ▼).</summary>
		public void Open(string[] firstPage)
		{
			Room.Paint();
			Redraw(firstPage);
		}

		/// <summary>Clears the screen and draws everything again (after another screen was shown on top).</summary>
		public void Redraw(string[] page, bool more = false)
		{
			Console.Clear();
			_screen.Invalidate();
			_titleBar();
			Box.Draw(page, more);
			Render();
		}

		/// <summary>Opens the scene and runs a script; the cursor is hidden meanwhile.</summary>
		public void Play(string[] firstPage, Action<Scene> script)
		{
			TryCursor(false);
			try
			{
				Open(firstPage);
				script(this);
			}
			finally
			{
				Console.Write("\x1b[0m");
				TryCursor(true);
			}
		}

		// ------------------------------------------------------------------ script steps

		/// <summary>Walks an actor to x (facing the way it goes), then stands it still.</summary>
		public void WalkTo(Actor a, int x, int speed = 2)
		{
			a.Dir = x >= a.X ? Actor.Right : Actor.Left;
			int step = 0;
			while (a.X != x && !_skipping)
			{
				a.Frame = 1 + step++ % HeroSprite.Frames;
				Render();
				if (!Tick()) break;
				a.X = a.X < x ? Math.Min(x, a.X + speed) : Math.Max(x, a.X - speed);
			}
			a.X = x;
			a.Frame = 0;
			Render();
		}

		/// <summary>A beat, then the actor drops through the floor out of sight.</summary>
		public void SinkOut(Actor a)
		{
			Pause(2);
			while (a.Sink <= a.Height && !_skipping)
			{
				a.Sink += a.Sink == 0 ? 1 : Math.Min(4, 1 + a.Sink / 4);
				Render();
				if (!Tick()) break;
			}
			a.Sink = a.Height + 2;
			Render();
		}

		/// <summary>Waits a few frames (the torches keep moving).</summary>
		public void Pause(int frames)
		{
			for (int i = 0; i < frames && !_skipping; i++)
			{
				Render();
				if (!Tick()) break;
			}
		}

		/// <summary>Shows pages in the box, each waiting for a key or click. Ends any skip.</summary>
		public void Say(IEnumerable<string[]> pages)
		{
			_skipping = false;
			foreach (var page in pages)
			{
				Box.Draw(page, more: true);
				while (!MouseInput.InputAvailable)
				{
					Thread.Sleep(FrameMs * 2);
					_t += 2;
					Render();
				}
				MouseInput.WaitForAnyInput();
			}
		}

		// ------------------------------------------------------------------ frames

		/// <summary>One frame on (live scenes call this once per frame).</summary>
		public void Advance() => _t++;

		/// <summary>Draws the current frame (only changed cells reach the terminal).</summary>
		public void Render()
		{
			Room.Back.CopyTo(_frame);
			Room.Animate(_frame, _t);
			var front = Room.Front.Px;
			for (int i = 0; i < front.Length; i++)
				if (front[i] != 0) _frame.Px[i] = front[i];
			foreach (var a in Actors.OrderBy(a => a.Lane)) a.Draw(Room, _frame); // nearer ones over farther

			// The part of the room on screen, over the backdrop where the room is clear.
			if (Room.Backdrop != null) Room.Backdrop.Paint(_view, CameraX, Room.W, _t);
			else Array.Clear(_view.Px);
			for (int y = 0; y < Room.H; y++)
			{
				int from = y * Room.W + CameraX, to = y * Width;
				for (int x = 0; x < Width && CameraX + x < Room.W; x++)
				{
					uint p = _frame.Px[from + x];
					if ((p & 0xFF) != 0) _view.Px[to + x] = p;
				}
			}
			_screen.Present(_view);
		}

		/// <summary>Waits one frame; false (and skipping from now on) when the viewer pressed something.</summary>
		private bool Tick()
		{
			if (MouseInput.InputAvailable)
			{
				MouseInput.WaitForAnyInput();
				_skipping = true;
				return false;
			}
			Thread.Sleep(FrameMs);
			_t++;
			return true;
		}

		public static void TryCursor(bool visible)
		{
			try { Console.CursorVisible = visible; } catch (Exception) { /* not every terminal allows it */ }
		}

		private static int SafeWidth() { try { return Console.WindowWidth; } catch { return 94; } }
		private static int SafeHeight() { try { return Console.WindowHeight; } catch { return 24; } }
	}
}
