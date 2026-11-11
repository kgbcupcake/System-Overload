using System.Text;

namespace Maries_Ui
{
	/// <summary>
	/// Reads the terminal's input one event at a time: keys, and mouse presses put back together from the
	/// escape codes Console.ReadKey hands over a character at a time. Button releases and motion are dropped.
	/// </summary>
	internal static class TerminalInput
	{
		private static readonly Queue<ConsoleKeyInfo> _pending = new();

		/// <summary>
		/// True if a key or mouse press is waiting. Use this instead of Console.KeyAvailable in
		/// animation loops: it buffers what's waiting and throws away button releases, which would
		/// otherwise wake the loop and then block it.
		/// </summary>
		public static bool Available
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

		/// <summary>Throws away everything waiting (before a text prompt).</summary>
		public static void Discard()
		{
			while (Console.KeyAvailable) Console.ReadKey(true);
			_pending.Clear();
		}
	}
}
