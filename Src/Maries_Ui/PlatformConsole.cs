namespace Maries_Ui
{
	public static class PlatformConsole
	{
		public static void TryResizeConsole(int width, int height)
		{
			try
			{
				if (OperatingSystem.IsWindows())
				{
					// Windows-specific buffer and window sizing
					Console.SetWindowSize(Math.Min(width, Console.LargestWindowWidth), Math.Min(height, Console.LargestWindowHeight));
					Console.SetBufferSize(width, height);
				}
				else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
				{
					// Linux/Unix use ANSI escape sequences to suggest terminal resizing
					// \e[8;height;widtht is the standard sequence
					Console.Write($"[8;{height};{width}t");
				}
			}
			catch { /* Resize Safety */ }
		}

		public static void PlayBeepSequence(params (int Frequency, int DurationMs)[] notes)
		{
			if (!OperatingSystem.IsWindows()) return;

			foreach (var note in notes)
			{
				Console.Beep(note.Frequency, note.DurationMs);
			}
		}
	}
}
