using Maries_Loaders;
using Pastel;
using System.Text;

using static System.Console;

namespace Maries_Ui
{
	/// <summary>The loading screen when the game starts.</summary>
	public static class GameStartLoading
	{
		public static void Play()
		{
			// --- STEP 1: DIMENSIONS & SCROLLBAR KILLER ---
			const int width = 94;
			const int height = 24;

			PlatformConsole.TryResizeConsole(width, height);

			Console.Clear();
			Console.CursorVisible = false;

			// FIX: Clean Title Bar (Removes the ←[38;2;... artifacts from screenshots)
			Console.Title = Story.Line("game.title");

			int centerX = width / 2;
			int centerY = height / 2;
			int barWidth = 40;
			Random rnd = new Random();

			// --- STEP 2: INITIAL BURST ---
			for (int j = 0; j < 5; j++)
			{
				Console.SetCursorPosition(0, 0);
				StringBuilder burst = new StringBuilder();
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						if (rnd.Next(0, 100) < 15)
						{
							burst.Append((rnd.Next(2) == 0 ? "*" : "·").Pastel(rnd.Next(2) == 0 ? "#FF4500" : "#FFAB00"));
						}
						else burst.Append(" ");
					}
				}
				Console.Write(burst.ToString());
				Thread.Sleep(300);
			}

			// --- STEP 3: THE MAIN LOOP (Embers + story banners) ---
			var scaryLabels = Story.Lines("loading.labels");
			string[] scaryColors = { "#FF0000", "#8B0000", "#4B0082", "#2F4F4F" };
			int labelIndex = 0;

			for (int i = 0; i <= 100; i++)
			{
				// --- THE EMBER ENGINE (Background) ---
				Console.SetCursorPosition(0, 0);
				StringBuilder rainFrame = new StringBuilder();
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						if (rnd.Next(0, 100) < 10)
						{
							rainFrame.Append((rnd.Next(3) == 0 ? "*" : "·").Pastel("#4A2A0A"));
						}
						else rainFrame.Append(" ");
					}
				}
				Console.Write(rainFrame.ToString());

				// --- THE UI OVERLAY ---
				// 1. TOP BANNER
				string topContent = Story.Line("loading.top_banner");
				string bannerRed = rnd.Next(0, 10) > 8 ? "#4A0000" : "#FF0000";
				UiEngine.DrawCentered(topContent.Pastel(bannerRed), centerY - 5);

				// 2. DYNAMIC LABEL
				if (i % 15 == 0 && scaryLabels.Count > 0) labelIndex = rnd.Next(scaryLabels.Count);
				if (scaryLabels.Count > 0) UiEngine.DrawCentered(scaryLabels[labelIndex].Pastel(scaryColors[rnd.Next(scaryColors.Length)]), centerY - 2);

				// 3. PROGRESS BAR
				int barLeft = centerX - (barWidth / 2);
				ProgressBarRenderer.Draw(barLeft, centerY, i / 100.0, barWidth, "#5A057A", "#333333");

				// 4. BOTTOM BANNER
				string bottomContent = Story.Line("loading.bottom_banner");
				UiEngine.DrawCentered(bottomContent.Pastel(scaryColors[rnd.Next(scaryColors.Length)]), centerY + 3);

				// --- SPEED CONTROL ---
				int baseDelay = 120;
				if (i < 30) Thread.Sleep(baseDelay + 100);
				else if (i < 80) Thread.Sleep(baseDelay);
				else Thread.Sleep(baseDelay + 150);

				if (rnd.Next(0, 70) == 0 && i < 90) i += rnd.Next(2, 8);
			}

			// --- STEP 4: FINAL MESSAGE ---
			Console.Clear();
			UiEngine.DrawCentered(Story.Line("loading.final").Pastel("#FFAB00"), centerY);

			// FIX: Ensure the loading screen stays until the user is ready for the menu
			UiEngine.DrawCentered("» PRESS ANY KEY «".Pastel("#444444"), centerY + 2);
			Thread.Sleep(1000);
			MouseInput.WaitForAnyInput();

			Console.ResetColor();
			Console.Clear();
		}
	}
}
