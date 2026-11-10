// Ignore Spelling: Tle

using Maries_Loaders;
using Pastel;
using static System.Console;

namespace Maries_Ui
{
	public static class HudChrome
	{
		public static void TitleBar(bool hasPlayer, string playerName, int health, int hitPoints, int coins, bool isDevMode)
		{
			// 1. FORCE UTF-8: Essential for Ubuntu/Linux to render ⚔️
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			// We save the cursor position first so we don't disrupt the menu
			int currentCursorLeft = Console.CursorLeft;
			int currentCursorTop = Console.CursorTop;

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
				Console.SetCursorPosition(currentCursorLeft, currentCursorTop);
			}
		}

		public static void DisplayFooter(bool isDevMode, string devIteration, string buildVersion, bool isVisible = true)
		{
			// CHANGE 1: Get the real width and height of the current window
			int width = Console.WindowWidth;
			int height = Console.WindowHeight;
			int footerRow = height - 1;

			int origLeft = CursorLeft;
			int origTop = CursorTop;

			try
			{
				string modeLabel = isDevMode ? " DEV " : " BUILD ";
				string modeStatus = isDevMode ? " F8 FOR GUI " : " STABLE ";
				string versionText = isDevMode ? $"ITER: {devIteration}" : $"V {buildVersion}";
				string accentColor = isDevMode ? "#FF4500" : "#125874";

				string leftBadge = modeLabel.PastelBg(accentColor).Pastel("#FFFFFF") +
								  modeStatus.PastelBg("#333333").Pastel(accentColor);

				string rightInfo = $" {versionText} ".Pastel("#888888").PastelBg("#1A1A1A");

				// CHANGE 2: The raw length calculation is correct,
				// but paddingSize will now be based on the REAL width
				int rawLength = modeLabel.Length + modeStatus.Length + versionText.Length + 2;
				int paddingSize = width - rawLength;
				string padding = new string(' ', Math.Max(0, paddingSize)).PastelBg("#1A1A1A");

				SetCursorPosition(0, footerRow);

				string fullFooter = leftBadge + padding + rightInfo;

				// CHANGE 3: Use the dynamic width variable here
				Write(fullFooter.PadRight(94));

				SetCursorPosition(origLeft, origTop);
			}
			catch { /* Resize Safety */ }
		}
	}
}
