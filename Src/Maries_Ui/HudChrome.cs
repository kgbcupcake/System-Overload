// Ignore Spelling: Tle

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

				Random rnd = new Random();
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
					Console.Title = $"⚔️ {modeLabel} MODE V2 ⚔️";
				}
				else
				{
					// --- Glitchy, Vague Title Bar (Player Not Established) ---
					string[] glitchTitles = {
				"S̷Y̷S̷T̵E̵M̴ ̶E̵R̷R̴O̷R̴",
				"D̷A̷T̵A̷ ̶C̶O̵R̷R̴U̷P̷T̸E̵D̸",
				"U̷N̵K̵N̷O̵W̷N̵ ̶E̵N̵T̸I̵T̸Y̵",
				"P̷R̷O̷T̸O̵C̸O̷L̵ ̶F̷A̷I̸L̴U̷R̷E̵"
			};

					string[] glitchChars = { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "=", "+", "[", "]", "{", "}", ";", ":", "'", "\"", "\\", "|", "<", ">", ",", ".", "/", "?", "`", "~" };
					string[] scaryColors = { "#FF0000", "#8B0000", "#4B0082", "#800000" }; // Red, Dark Red, Indigo, Maroon

					// Flickering Console.Title
					Console.Title = glitchTitles[rnd.Next(glitchTitles.Length)].Pastel(scaryColors[rnd.Next(scaryColors.Length)]);

					// Row 0 Display - Dynamic and Glitchy
					string glitchText = GlitchText.GenerateGlitchString(rnd.Next(15, 30), glitchChars, scaryColors, rnd);
					UiEngine.DrawCentered(glitchText, 0);
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
