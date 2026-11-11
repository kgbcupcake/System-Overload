
namespace Maries_Ui
{
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
		private static int _depth;      // Enable() calls not yet disposed
		private static int _suspended;  // Suspend() calls not yet disposed (e.g. typing a name)
		private static bool _active;    // what the terminal was last told
		private static bool _exitHooked;

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
			TerminalInput.Discard();
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
				if (OperatingSystem.IsWindows()) WindowsConsoleMode.EnableVtInput();
				Console.Write("\x1b[?1000h\x1b[?1006h");
			}
			else
			{
				Console.Write("\x1b[?1006l\x1b[?1000l");
				if (OperatingSystem.IsWindows()) WindowsConsoleMode.Restore();
			}
		}

		// ---------------------------------------------------------------- click zones

		/// <summary>Forget every click zone. Screens call this before drawing their own.</summary>
		public static void ClearZones() => ClickZones.Clear();

		/// <summary>A menu row: clicking it selects <paramref name="index"/> and acts like Enter.</summary>
		public static void AddZone(int y, int x, int width, int index) => ClickZones.AddRow(y, x, width, index);

		/// <summary>
		/// A button: clicking it acts like pressing <paramref name="key"/>. With
		/// <paramref name="index"/> it also selects that menu row first (e.g. a [+] button on a row).
		/// </summary>
		public static void AddKeyZone(int y, int x, int width, ConsoleKey key, int? index = null) => ClickZones.AddKey(y, x, width, key, index);

		// ---------------------------------------------------------------- reading

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
						var zone = ClickZones.At(m.X, m.Y);
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
				if (m.Action == MouseAction.LeftClick && ClickZones.At(m.X, m.Y)?.Key is { } key) return KeyInfo(key);
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
		public static bool InputAvailable => TerminalInput.Available;

		/// <summary>Blocks until a key press or a mouse press (button release and motion are skipped).</summary>
		public static InputEvent Read() => TerminalInput.Read();

		private static void HookExit()
		{
			if (_exitHooked) return;
			_exitHooked = true;
			// Never leave the terminal in mouse mode if the game exits or crashes on a mouse screen.
			AppDomain.CurrentDomain.ProcessExit += (_, _) => Disable();
			Console.CancelKeyPress += (_, _) => Disable();
		}
	}
}
