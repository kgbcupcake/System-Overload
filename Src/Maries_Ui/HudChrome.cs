// Ignore Spelling: Tle

using Maries_Loaders;
using Pastel;
using static System.Console;

namespace Maries_Ui
{
	public static class HudChrome
	{
		/// <summary>
		/// Remembers the cursor position. On Linux/macOS this uses the terminal's own save code
		/// (ESC 7) instead of Console.CursorLeft/Top: those ask the terminal for the position and read
		/// the answer from the same input stream mouse clicks arrive on, so a click at the wrong
		/// moment produced garbage positions and crashed. Windows reads the position from its API.
		/// </summary>
		private static (int Left, int Top)? SaveCursor()
		{
			if (OperatingSystem.IsWindows()) return (Console.CursorLeft, Console.CursorTop);
			Console.Write("\u001b7"); // not "\x1b7": C# reads that as one char, U+01B7
			return null;
		}

		private static void RestoreCursor((int Left, int Top)? saved)
		{
			if (saved is { } pos) Console.SetCursorPosition(pos.Left, pos.Top);
			else Console.Write("\u001b8");
		}

		public static void TitleBar(bool hasPlayer, string playerName, int health, int hitPoints, int coins, bool isDevMode)
		{
			// 1. FORCE UTF-8: Essential for Ubuntu/Linux to render ⚔️
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			// We save the cursor position first so we don't disrupt the menu
			var saved = SaveCursor();

			try
			{
				Console.SetCursorPosition(0, 0);
				// Wipe the top line with spaces to act as a physical eraser
				Console.Write(new string(' ', 84));

				// Check if player is established (e.g., after character creation or loading a game)
				if (hasPlayer)
				{
					// --- FIX: DRAW THE STATS TO THE CONSOLE SCREEN ---
					Console.SetCursorPosition(0, 0);
					// This pulls your LIVE Health and Coins
					Console.Write($" 👤 {playerName}".Pastel("#FFD700"));
					Console.Write($" | ❤️ HP: {health}/{hitPoints}".Pastel("#FF5555"));
					Console.Write($" | 💰 {coins}".Pastel("#FFD700"));

					// 2. SET THE TAB TITLE: Use the property for better cross-platform support
					string modeLabel = isDevMode ? "DEVELOPER" : "STABLE";
					Console.Title = $"{Story.Line("game.title")} [{modeLabel}]";
				}
				else
				{
					// --- No hero yet: show the game's name and a line of story ---
					Console.Title = Story.Line("game.title");
					UiEngine.DrawCentered(Story.Pick("title.banner").Pastel("#B8860B"), 0);
				}
			}
			finally
			{
				// Always restore the cursor position
				RestoreCursor(saved);
			}
		}

		public static void DisplayFooter(bool isDevMode, string devIteration, string buildVersion, bool isVisible = true)
		{
			// CHANGE 1: Get the real width and height of the current window
			int width = Console.WindowWidth;
			int height = Console.WindowHeight;
			int footerRow = height - 1;

			var saved = SaveCursor();

			try
			{
				string versionText = isDevMode ? $"ITER: {devIteration}" : $"V {buildVersion}";
				string rightInfo = $" {versionText} ".Pastel("#888888").PastelBg("#1A1A1A");

				// The footer is just a dark bar with the version on the right.
				int rawLength = versionText.Length + 2;
				int paddingSize = width - rawLength;
				string padding = new string(' ', Math.Max(0, paddingSize)).PastelBg("#1A1A1A");

				SetCursorPosition(0, footerRow);

				string fullFooter = padding + rightInfo;

				// CHANGE 3: Use the dynamic width variable here
				Write(fullFooter.PadRight(94));

				RestoreCursor(saved);
			}
			catch { /* Resize Safety */ }
		}
	}
}
