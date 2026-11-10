using System.Runtime.InteropServices;
using System.Text;

namespace Maries_Ui
{
	public enum MouseAction { LeftClick, RightClick, MiddleClick, WheelUp, WheelDown }

	/// <summary>A mouse press at a 0-based screen column (X) and row (Y).</summary>
	public readonly record struct MouseEvent(MouseAction Action, int X, int Y);

	/// <summary>One piece of input: either a key or a mouse event.</summary>
	public readonly record struct InputEvent(ConsoleKeyInfo? Key, MouseEvent? Mouse);

	/// <summary>
	/// Mouse support for console screens. <see cref="Enable"/> asks the terminal to report clicks
	/// (xterm SGR mouse mode, supported by Konsole, GNOME Terminal, kitty, Windows Terminal...),
	/// and <see cref="Read"/> returns keys and clicks. Clicks arrive as escape codes like
	/// ESC [ &lt; 0 ; 12 ; 3 M, which Console.ReadKey hands over one character at a time; this class
	/// puts them back together. While mouse mode is on, plain click-and-drag no longer selects text
	/// (hold Shift to select in most terminals).
	///
	/// Menus: screens register click zones while drawing (<see cref="AddZone"/> for a menu row,
	/// <see cref="AddKeyZone"/> for a button that acts like a key) and read input with
	/// <see cref="ReadMenuKey"/> or <see cref="ReadKey"/>. "Press any key" prompts use
	/// <see cref="WaitForAnyInput"/>, where any click also counts.
	/// </summary>
	public static class MouseInput
	{
		private static readonly Queue<ConsoleKeyInfo> _pending = new();
		private static int _depth;      // Enable() calls not yet disposed
		private static int _suspended;  // Suspend() calls not yet disposed (e.g. typing a name)
		private static bool _active;    // what the terminal was last told
		private static bool _exitHooked;
		private static uint? _savedWindowsMode;

		/// <summary>
		/// Turns mouse reporting on. Calls nest: mouse mode stays on until every Enable() has been
		/// disposed, so a screen can enable it even if the whole game already has.
		/// </summary>
		public static IDisposable Enable()
		{
			_depth++;
			HookExit();
			Apply();
			return new Scope(() => { _depth = Math.Max(0, _depth - 1); Apply(); });
		}

		/// <summary>
		/// Turns mouse reporting off until disposed, for text input (Console.ReadLine would
		/// otherwise receive click codes as typed characters).
		/// </summary>
		public static IDisposable Suspend()
		{
			_suspended++;
			Apply();
			// Throw away anything still waiting (e.g. a late button release) so the text prompt
			// starts clean.
			Thread.Sleep(30);
			while (Console.KeyAvailable) Console.ReadKey(true);
			_pending.Clear();
			return new Scope(() => { _suspended = Math.Max(0, _suspended - 1); Apply(); });
		}

		/// <summary>Forces mouse reporting off (used on exit).</summary>
		public static void Disable()
		{
			_depth = 0;
			Apply();
		}

		private static void Apply()
		{
			bool want = _depth > 0 && _suspended == 0;
			if (want == _active) return;
			_active = want;
			if (want)
			{
				if (OperatingSystem.IsWindows()) EnableWindowsVtInput();
				Console.Write("\x1b[?1000h\x1b[?1006h");
			}
			else
			{
				Console.Write("\x1b[?1006l\x1b[?1000l");
				if (OperatingSystem.IsWindows()) RestoreWindowsMode();
			}
		}

		// ---------------------------------------------------------------- click zones

		/// <summary>A clickable span on one screen row: either a menu row (Index) or a key button (Key).</summary>
		private sealed record Zone(int Y, int X1, int X2, int? Index, ConsoleKey? Key);
		private static readonly List<Zone> _zones = new();

		/// <summary>Forget every click zone. Screens call this before drawing their own.</summary>
		public static void ClearZones() => _zones.Clear();

		/// <summary>A menu row: clicking it selects <paramref name="index"/> and acts like Enter.</summary>
		public static void AddZone(int y, int x, int width, int index) =>
			_zones.Add(new Zone(y, x, x + Math.Max(1, width) - 1, index, null));

		/// <summary>
		/// A button: clicking it acts like pressing <paramref name="key"/>. With
		/// <paramref name="index"/> it also selects that menu row first (e.g. a [+] button on a row).
		/// </summary>
		public static void AddKeyZone(int y, int x, int width, ConsoleKey key, int? index = null) =>
			_zones.Add(new Zone(y, x, x + Math.Max(1, width) - 1, index, key));

		private static Zone? ZoneAt(int x, int y) =>
			_zones.LastOrDefault(z => z.Y == y && x >= z.X1 && x <= z.X2);

		private static ConsoleKeyInfo KeyInfo(ConsoleKey key)
		{
			char c = key switch
			{
				ConsoleKey.Enter => '\r',
				ConsoleKey.Escape => '\x1b',
				ConsoleKey.Tab => '\t',
				ConsoleKey.Spacebar => ' ',
				>= ConsoleKey.A and <= ConsoleKey.Z => (char)('a' + (key - ConsoleKey.A)),
				>= ConsoleKey.D0 and <= ConsoleKey.D9 => (char)('0' + (key - ConsoleKey.D0)),
				_ => '\0',
			};
			return new ConsoleKeyInfo(c, key, false, false, false);
		}

		/// <summary>
		/// Reads a key for a menu. A click on a menu row sets <paramref name="selected"/> to that row
		/// and returns Enter; a click on a key button returns its key; the wheel returns Up/Down.
		/// Rows outside 0..count-1 are ignored, so stale zones can never select a missing item.
		/// </summary>
		public static ConsoleKey ReadMenuKey(ref int selected, int count = int.MaxValue)
		{
			while (true)
			{
				var input = Read();
				if (input.Key is { } k) return k.Key;
				if (input.Mouse is not { } m) continue;

				switch (m.Action)
				{
					case MouseAction.WheelUp: return ConsoleKey.UpArrow;
					case MouseAction.WheelDown: return ConsoleKey.DownArrow;
					case MouseAction.LeftClick:
						var zone = ZoneAt(m.X, m.Y);
						if (zone is null) break;
						bool validRow = zone.Index is { } index && index >= 0 && index < count;
						if (validRow) selected = zone.Index!.Value;
						if (zone.Key is { } key) return key;
						if (validRow) return ConsoleKey.Enter;
						break;
				}
			}
		}

		/// <summary>Reads a key; clicks on key buttons count as their key, the wheel as Up/Down.</summary>
		public static ConsoleKeyInfo ReadKey()
		{
			while (true)
			{
				var input = Read();
				if (input.Key is { } k) return k;
				if (input.Mouse is not { } m) continue;
				if (m.Action == MouseAction.WheelUp) return KeyInfo(ConsoleKey.UpArrow);
				if (m.Action == MouseAction.WheelDown) return KeyInfo(ConsoleKey.DownArrow);
				if (m.Action == MouseAction.LeftClick && ZoneAt(m.X, m.Y)?.Key is { } key) return KeyInfo(key);
			}
		}

		/// <summary>Waits for any key or any mouse click ("Press any key to continue").</summary>
		public static void WaitForAnyInput()
		{
			while (true)
			{
				var input = Read();
				if (input.Key is not null) return;
				if (input.Mouse is { Action: MouseAction.LeftClick or MouseAction.RightClick }) return;
			}
		}

		/// <summary>
		/// True if a key or mouse press is waiting. Use this instead of Console.KeyAvailable in
		/// animation loops: it buffers what's waiting and throws away button releases, which would
		/// otherwise wake the loop and then block it.
		/// </summary>
		public static bool InputAvailable
		{
			get
			{
				while (Console.KeyAvailable) _pending.Enqueue(Console.ReadKey(true));
				DropReleases();
				return _pending.Count > 0;
			}
		}

		/// <summary>
		/// After a click, waits briefly for the button's release and throws it away, so the release
		/// can't leak into whatever runs next (a text prompt, a cursor-position query...).
		/// A button held longer than this is fine too: later reads skip stray releases.
		/// </summary>
		private static void AwaitRelease()
		{
			var timer = System.Diagnostics.Stopwatch.StartNew();
			while (timer.ElapsedMilliseconds < 300)
			{
				while (Console.KeyAvailable) _pending.Enqueue(Console.ReadKey(true));
				if (DropReleases() > 0) return;
				Thread.Sleep(5);
			}
		}

		/// <summary>Removes complete mouse-release codes (ESC [ &lt; b ; x ; y m) from the buffer.</summary>
		private static int DropReleases()
		{
			if (_pending.Count == 0) return 0;
			int dropped = 0;
			var keys = _pending.ToList();
			var kept = new List<ConsoleKeyInfo>(keys.Count);
			for (int i = 0; i < keys.Count; i++)
			{
				if (keys[i].Key == ConsoleKey.Escape && i + 2 < keys.Count && keys[i + 1].KeyChar == '[' && keys[i + 2].KeyChar == '<')
				{
					int j = i + 3;
					while (j < keys.Count && (char.IsDigit(keys[j].KeyChar) || keys[j].KeyChar == ';')) j++;
					if (j < keys.Count && keys[j].KeyChar == 'm')
					{
						i = j; // skip the whole release code
						dropped++;
						continue;
					}
				}
				kept.Add(keys[i]);
			}
			_pending.Clear();
			foreach (var k in kept) _pending.Enqueue(k);
			return dropped;
		}

		/// <summary>Blocks until a key press or a mouse press (button release and motion are skipped).</summary>
		public static InputEvent Read()
		{
			while (true)
			{
				var key = Next();
				if (key.Key != ConsoleKey.Escape || !(Console.KeyAvailable || _pending.Count > 0))
					return new InputEvent(key, null);

				// Escape with more input waiting: maybe an escape code, maybe two quick key presses.
				var second = Next();
				if (second.KeyChar is '[' or 'O')
				{
					var parsed = ParseEscapeCode(second.KeyChar);
					if (parsed is { } result)
					{
						if (result.Mouse is null && result.Key is null) continue; // release, motion, unknown code
						if (result.Mouse is { Action: MouseAction.LeftClick or MouseAction.RightClick or MouseAction.MiddleClick })
							AwaitRelease();
						return result;
					}
				}

				// Not an escape code: hand back Escape now and the other key next time.
				_pending.Enqueue(second);
				return new InputEvent(key, null);
			}
		}

		private static ConsoleKeyInfo Next() =>
			_pending.Count > 0 ? _pending.Dequeue() : Console.ReadKey(true);

		/// <summary>
		/// Reads the rest of an escape code after ESC and its introducer ('[' or 'O').
		/// Returns null when the input isn't an escape code; returns an empty event for codes we ignore.
		/// </summary>
		private static InputEvent? ParseEscapeCode(char introducer)
		{
			var body = new StringBuilder();
			while (Console.KeyAvailable || _pending.Count > 0)
			{
				char c = Next().KeyChar;
				body.Append(c);
				if (c is >= '@' and <= '~' && !(body.Length == 1 && c == '<')) break; // final byte
				if (body.Length > 32) break;
			}
			string code = body.ToString();
			if (code.Length == 0) return null;

			// Arrow keys sent as VT codes (Windows with VT input, or application cursor mode).
			ConsoleKey? arrow = code switch
			{
				"A" => ConsoleKey.UpArrow,
				"B" => ConsoleKey.DownArrow,
				"C" => ConsoleKey.RightArrow,
				"D" => ConsoleKey.LeftArrow,
				_ => null,
			};
			if (arrow is { } a)
				return new InputEvent(new ConsoleKeyInfo('\0', a, false, false, false), null);

			// SGR mouse: "<b;x;yM" (press) or "<b;x;ym" (release).
			if (introducer == '[' && code.StartsWith('<') && (code.EndsWith('M') || code.EndsWith('m')))
			{
				var parts = code[1..^1].Split(';');
				if (parts.Length != 3 || !int.TryParse(parts[0], out int b)
					|| !int.TryParse(parts[1], out int x) || !int.TryParse(parts[2], out int y))
					return new InputEvent(null, null);

				bool press = code.EndsWith('M');
				bool motion = (b & 32) != 0;
				if (!press || motion) return new InputEvent(null, null);

				MouseAction? action = (b & ~(4 | 8 | 16)) switch
				{
					0 => MouseAction.LeftClick,
					1 => MouseAction.MiddleClick,
					2 => MouseAction.RightClick,
					64 => MouseAction.WheelUp,
					65 => MouseAction.WheelDown,
					_ => null,
				};
				return action is { } act
					? new InputEvent(null, new MouseEvent(act, x - 1, y - 1))
					: new InputEvent(null, null);
			}

			return new InputEvent(null, null);
		}

		private static void HookExit()
		{
			if (_exitHooked) return;
			_exitHooked = true;
			// Never leave the terminal in mouse mode if the game exits or crashes on a mouse screen.
			AppDomain.CurrentDomain.ProcessExit += (_, _) => Disable();
			Console.CancelKeyPress += (_, _) => Disable();
		}

		private sealed class Scope : IDisposable
		{
			private Action? _onDispose;
			public Scope(Action onDispose) => _onDispose = onDispose;
			public void Dispose()
			{
				_onDispose?.Invoke();
				_onDispose = null; // disposing twice must not undo someone else's Enable()
			}
		}

		// ---------------------------------------------------------------- Windows console

		private const int StdInputHandle = -10;
		private const uint EnableMouseInput = 0x0010;
		private const uint EnableQuickEditMode = 0x0040;
		private const uint EnableExtendedFlags = 0x0080;
		private const uint EnableVirtualTerminalInput = 0x0200;

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool GetConsoleMode(IntPtr handle, out uint mode);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleMode(IntPtr handle, uint mode);

		/// <summary>On Windows the console only passes mouse codes through with VT input on and Quick Edit off.</summary>
		private static void EnableWindowsVtInput()
		{
			try
			{
				var handle = GetStdHandle(StdInputHandle);
				if (!GetConsoleMode(handle, out uint mode)) return;
				_savedWindowsMode = mode;
				uint wanted = (mode | EnableVirtualTerminalInput | EnableExtendedFlags | EnableMouseInput) & ~EnableQuickEditMode;
				SetConsoleMode(handle, wanted);
			}
			catch { /* Not a real console (redirected input): keyboard still works. */ }
		}

		private static void RestoreWindowsMode()
		{
			if (_savedWindowsMode is not { } mode) return;
			try { SetConsoleMode(GetStdHandle(StdInputHandle), mode); } catch { }
			_savedWindowsMode = null;
		}
	}
}
