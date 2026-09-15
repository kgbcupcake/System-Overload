using Pastel;
using System.Text;

using static System.Console;

namespace Maries_Ui
{
	public static class Transitions
	{
		public static void ShowSaveLoadedIcon(string playerName, int health, int hitPoints, int coins, bool isDevMode, string devIteration, string buildVersion)
		{
			Console.Clear();
			// V2 Integration
			HudChrome.TitleBar(true, playerName, health, hitPoints, coins, isDevMode);
			HudChrome.DisplayFooter(isDevMode, devIteration, buildVersion);

			int centerX = Console.WindowWidth / 2;
			int startY = 5;

			// --- SIDE DATA PILLARS (Dynamic Width) ---
			string[] sideDecor = { " [CON-4] ", " [TRN-R] ", " [LOG-V] ", " [SYS-8] " };
			for (int i = 0; i < sideDecor.Length; i++)
			{
				// Use Console.WindowWidth - 10 to keep them relative to the screen edge
				Console.SetCursorPosition(2, 7 + (i * 3));
				Console.Write(sideDecor[i].Pastel("#125874"));

				Console.SetCursorPosition(Console.WindowWidth - 11, 7 + (i * 3));
				Console.Write(sideDecor[i].Pastel("#125874"));
			}

			// 1. TOP ACCENTS
			string topAccent = "◢◤".Pastel("#00FF00") + " —————————————————————————————————————— " + "◥◣".Pastel("#00FF00");
			Console.SetCursorPosition(Math.Max(0, centerX - (UiEngine.StripAnsi(topAccent).Length / 2)), startY - 2);
			Console.Write(topAccent);

			// 2. 3D ICON
			string icon = @"
      _______________________
     /                      /|
    /      RECONSTRUCT     / |
   /        COMPLETE      /  |
  /______________________/   |
  |                      |   |
  |   [ VESSEL ONLINE ]  |   |
  |______________________|  /";

			string[] lines = icon.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			int currentY = startY;
			foreach (string line in lines)
			{
				Console.SetCursorPosition(centerX - 13, currentY++);
				string color = (currentY % 2 == 0) ? "#00FF00" : "#008800";
				Console.Write(line.Pastel(color));
			}

			// 3. PLAYER DATA BLOCK
			currentY += 1;
			string bracketL = ">> [".Pastel("#00FFFF");
			string bracketR = "] <<".Pastel("#00FFFF");
			string nameDisp = playerName.ToUpper().Pastel("#FFD700");
			string fullRow = $"{bracketL} {nameDisp} {bracketR}";

			// Using your new UiEngine helper to calculate centering
			int textLen = UiEngine.StripAnsi(fullRow).Length;
			Console.SetCursorPosition(centerX - (textLen / 2), currentY);
			Console.WriteLine(fullRow);

			// 4. METRICS
			currentY += 3;
			string[] metrics = { "INTEGRITY: 100%", "BIO-SYNC: STABLE", "CHRONO-LINK: ACTIVE" };
			foreach (var m in metrics)
			{
				Console.SetCursorPosition(centerX - (m.Length / 2), currentY++);
				Console.Write(m.Pastel("#333333"));
			}

			// 5. THE PULSE PROMPT
			string prompt = "— PRESS ANY KEY TO ENTER THE VOID —";
			int promptY = 20;

			PlatformConsole.PlayBeepSequence((600, 50), (900, 50), (1200, 100));

			while (!Console.KeyAvailable)
			{
				UiEngine.DrawCentered(prompt.Pastel("#FFFFFF"), promptY, 50);
				Thread.Sleep(500);
				if (Console.KeyAvailable) break;
				UiEngine.DrawCentered(prompt.Pastel("#444444"), promptY, 50);
				Thread.Sleep(250);
			}

			Console.ReadKey(true);
			Clear();
		}

		public static void LoadSaveProgress()
		{
			Console.Clear();

			int centerX = 85 / 2;
			int centerY = 12;
			int barWidth = 40;
			Random rnd = new Random();

			// --- RAIN CONFIG ---
			// Columns on the left (5, 10, 15) and right (70, 75, 80)
			int[] rainCols = { 5, 10, 15, 70, 75, 80 };
			int[] rainY = new int[rainCols.Length];
			for (int r = 0; r < rainY.Length; r++) rainY[r] = rnd.Next(2, 22);

			// 1. PHASE ONE: RAPID DATA SCAN
			for (int i = 0; i < 15; i++)
			{
				Console.SetCursorPosition(centerX - 20, centerY);
				string noise = $"SCANNING_SECTOR_{rnd.Next(1000, 9999)} >> 0x{rnd.Next(100, 999)}FF{rnd.Next(10, 99)}";
				Console.Write(noise.Pastel("#ed0410"));
				Thread.Sleep(90);
			}

			string[] steps = {
		"INITIALIZING HANDSHAKE",
		"DECRYPTING SOUL-FILE",
		"STITCHING TEMPORAL VECTORS",
		"RECONSTRUCTING VESSEL"
	};

			// 2. PHASE TWO: THE PROGRESS BAR + RAIN
			for (int i = 0; i < steps.Length; i++)
			{
				Console.SetCursorPosition(centerX - 25, centerY - 2);
				Console.Write(new string(' ', 50));
				Console.SetCursorPosition(centerX - (steps[i].Length / 2), centerY - 2);
				Console.Write(steps[i].Pastel("#00FFFF"));

				for (int p = 0; p <= 10; p++)
				{
					// --- UPDATE BINARY RAIN ---
					for (int r = 0; r < rainCols.Length; r++)
					{
						// Erase old bit
						Console.SetCursorPosition(rainCols[r], rainY[r]);
						Console.Write(" ");

						// Increment and wrap Y
						rainY[r]++;
						if (rainY[r] > 21) rainY[r] = 2;

						// Draw new bit
						Console.SetCursorPosition(rainCols[r], rainY[r]);
						string bit = rnd.Next(2).ToString();
						// Alternate between bright cyan and deep teal for depth
						string color = rnd.Next(3) == 0 ? "#00FFFF" : "#004444";
						Console.Write(bit.Pastel(color));
					}

					// --- UPDATE PROGRESS BAR ---
					float totalPercent = ((i * 10) + p) / 40f;
					ProgressBarRenderer.Draw(centerX - (barWidth / 2), centerY, totalPercent, barWidth, "#00FFFF", "#222222");

					Thread.Sleep(rnd.Next(30, 80));
				}
			}

			// 3. FINAL LOCK-ON
			Console.SetCursorPosition(centerX - 10, centerY + 2);
			Console.Write("SYNC SUCCESSFUL".Pastel("#00FF00"));

			PlatformConsole.PlayBeepSequence((1200, 100), (1500, 150));
			Thread.Sleep(800);
		}

		public static void StartGameLoading()
		{
			// --- STEP 1: DIMENSIONS & SCROLLBAR KILLER ---
			const int width = 94;
			const int height = 24;

			PlatformConsole.TryResizeConsole(width, height);

			Console.Clear();
			Console.CursorVisible = false;

			// FIX: Clean Title Bar (Removes the ←[38;2;... artifacts from screenshots)
			Console.Title = "Dungeon Adventures - Reborn";

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
							burst.Append(GlitchText.NextGlitchChar(rnd, null, new[] { "#FF0000", "#00FF00" }));
						}
						else burst.Append(" ");
					}
				}
				Console.Write(burst.ToString());
				Thread.Sleep(300);
			}

			// --- STEP 3: THE MAIN LOOP (Rain + Skulls + Nukes) ---
			string[] scaryLabels = {
		"SYSTEM INTEGRITY: CRITICAL...",
		"REALITY ANCHORS: DEGRADED...",
		"INITIATING UNSTABLE PROTOCOL...",
		"WARNING: ENTITY DETECTED...",
		"S̴Y̷S̷T̵E̵M̴ ̶C̶O̵R̷R̴U̷P̷T̸E̵D̸...",
		"F̸A̴I̸L̴U̶R̷E̵ ̸T̸O̵ ̷C̸O̷N̵N̷E̵C̷T̵..."
	};
			string[] scaryColors = { "#FF0000", "#8B0000", "#4B0082", "#2F4F4F" };
			int labelIndex = 0;

			for (int i = 0; i <= 100; i++)
			{
				// --- THE RAIN ENGINE (Background) ---
				Console.SetCursorPosition(0, 0);
				StringBuilder rainFrame = new StringBuilder();
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						if (rnd.Next(0, 100) < 10)
						{
							rainFrame.Append(GlitchText.NextGlitchChar(rnd, null, new[] { "#004400" }));
						}
						else rainFrame.Append(" ");
					}
				}
				Console.Write(rainFrame.ToString());

				// --- THE UI OVERLAY ---
				// 1. TOP PATTERN: RED SKULLS + ERROR MESSAGE
				string skullIcon = "☠";
				string skullMsg = " [ FATAL_MEMORY_LEAK ] ";
				string topContent = $"{skullIcon}{skullMsg}{skullIcon}";
				string glitchRed = rnd.Next(0, 10) > 8 ? "#4A0000" : "#FF0000";
				UiEngine.DrawCentered(topContent.Pastel(glitchRed), centerY - 5);

				// 2. DYNAMIC LABEL
				if (i % 15 == 0) labelIndex = rnd.Next(scaryLabels.Length);
				UiEngine.DrawCentered(scaryLabels[labelIndex].Pastel(scaryColors[rnd.Next(scaryColors.Length)]), centerY - 2);

				// 3. PROGRESS BAR
				int barLeft = centerX - (barWidth / 2);
				ProgressBarRenderer.Draw(barLeft, centerY, i / 100.0, barWidth, "#5A057A", "#333333");

				// 4. BOTTOM PATTERN: NUKES + RADIATION WARNING
				string nukeIcon = "☢";
				string nukeMsg = " [ CORE_MELTDOWN_IMMINENT ] ";
				string bottomContent = nukeIcon + nukeMsg + nukeIcon;
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
			UiEngine.DrawCentered("T̷H̷E̵ ̸G̷A̷T̷E̵S̷ ̷A̶R̸E̵ ̷O̷P̷E̶N̸...".Pastel("#FF0000"), centerY);

			// FIX: Ensure the loading screen stays until the user is ready for the menu
			UiEngine.DrawCentered("» PRESS ANY KEY «".Pastel("#444444"), centerY + 2);
			Thread.Sleep(1000);
			Console.ReadKey(true);

			Console.ResetColor();
			Console.Clear();
		}

		public static void ShowCreationAnimation(string playerName, string playerClass, int health, int hitPoints, int coins, bool isDevMode, string devIteration, string buildVersion)
		{
			// --- DYNAMIC THEME ---
			string themeColor = playerClass switch
			{
				"Warrior" or "Paladin" or "Berserker" => "#FF4500",
				"Mage" or "Wizard" or "Warlock" => "#00FFFF",
				"Rogue" or "Assassin" => "#9370DB",
				_ => "#00FF00"
			};

			string[] steps = {
		$"Analyzing {playerName}'s Potential...",
		$"Forging the {playerClass} Soul-Core...",
		"Calibrating Attribute Matrix...",
		"Manifesting Physical Vessel...",
		"Injecting Starter Gear into Pack...",
		"Syncing Chrono-Link to World..."
	};

			Console.Clear();
			HudChrome.TitleBar(true, playerName, health, hitPoints, coins, isDevMode);
			UiEngine.DrawCentered($"SYSTEM: CHARACTER MANIFESTATION".Pastel(themeColor), 4);
			HudChrome.DisplayFooter(isDevMode, devIteration, buildVersion);

			int startY = 7;
			int centerX = (Console.WindowWidth / 2) - 20;

			for (int i = 0; i < steps.Length; i++)
			{
				Console.SetCursorPosition(centerX, startY + i);
				Console.Write("[ ".Pastel("#555555") + "WAIT".Pastel("#FFD700") + " ] ".Pastel("#555555") + steps[i]);

				// We removed the SaveData logic from here.
				// The data is already synced by GameState.Sync() before this starts.

				Thread.Sleep(new Random().Next(300, 600));

				Console.SetCursorPosition(centerX, startY + i);
				Console.Write("[ ".Pastel("#555555") + "DONE".Pastel("#00FF00") + " ] ".Pastel("#555555") + steps[i]);
			}

			// --- FINAL FEEDBACK ---
			string checksum = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
			Console.SetCursorPosition(centerX, startY + steps.Length + 1);
			Console.WriteLine($"» HASH VERIFIED: {checksum}".Pastel("#555555"));

			Console.SetCursorPosition(centerX, startY + steps.Length + 2);
			Console.WriteLine("» UPLINK COMPLETE: LOCAL & MASTER FULLY SYNCED...".Pastel(themeColor));

			Thread.Sleep(1200);
			Clear();
		}

		public static void AreaLoadingScreen(string areaName, string hexColor)
		{
			Console.Clear();
			Console.CursorVisible = false;

			int barWidth = 40; // Slightly narrower for a "smaller" area feel
			int centerX = 94 / 2;
			int centerY = 25 / 2;

			// 1. Label (e.g., "ENTERING THE GENERAL STORE...")
			string plainLabel = $"ENTERING THE {areaName.ToUpper()}...";
			UiEngine.DrawCentered(plainLabel.Pastel(hexColor), centerY - 2);

			// 2. The Bar
			for (int i = 0; i <= 100; i++)
			{
				int barLeft = centerX - (barWidth / 2);
				// Use the hexColor passed into the method for the bar!
				ProgressBarRenderer.Draw(barLeft, centerY, i / 100.0, barWidth, hexColor, "#333333");

				Thread.Sleep(15); // Stores usually load fast!
			}

			Thread.Sleep(400);
			Console.Clear();
			//Store.LoadShop();
			Console.CursorVisible = true;
		}

		public static void ShutdownSequence()
		{
			CursorVisible = false;
			Random rnd = new Random();

			// 1. Cold Asset Liquidation (The Cynical Setup)
			Clear();
			string[] tasks = {
		"VALUATING BIOLOGICAL DEBT...",
		"EXTRACTING NEURAL POTENTIAL...",
		"DEPRECATING PLAYER_IDENTITY..."
	};

			for (int i = 0; i < tasks.Length; i++)
			{
				SetCursorPosition(4, 5 + i);
				Write($"[ ANALYZING ] {tasks[i]}".Pastel("#555555"));
				Thread.Sleep(1000);
				SetCursorPosition(4, 5 + i);
				Write($"[ LIQUIDATED ] {tasks[i]}".Pastel("#8B0000"));
				WriteLine(" [ VALUE: 0.00 ]".Pastel("#444444"));
			}
			Thread.Sleep(1000);

			// 2. Digital Rain / Entity Detection (The Screenshot Phase)
			for (int i = 0; i < 15; i++)
			{
				Clear();
				for (int r = 0; r < 20; r++) // Matrix Background
				{
					string rain = new string(' ', rnd.Next(2, 80)) + (char)rnd.Next(33, 126);
					SetCursorPosition(0, r);
					Write(rain.Pastel("#1A3300")); // Dim green rain
				}

				UiEngine.DrawCentered("☠ [ FATAL_MEMORY_LEAK ] ☠".Pastel("#FF0000"), 8);
				UiEngine.DrawCentered("WARNING: ENTITY DETECTED...".Pastel("#555555"), 10);
				Thread.Sleep(80);
			}

			// 3. The Purple Loading Bar
			int barWidth = 40;
			for (int p = 0; p <= 100; p += 2)
			{
				ProgressBarRenderer.Draw(27, 12, p / 100.0, barWidth, "#800080", "#333333");

				if (p >= 40)
				{
					UiEngine.DrawCentered("☢ [ CORE_MELTDOWN_IMMINENT ] ☢".Pastel("#FF0000"), 15);
				}
				// Stutters more as it reaches 100% to simulate a crash
				Thread.Sleep(p > 80 ? rnd.Next(150, 400) : rnd.Next(30, 100));
			}

			// 4. The Glitching Skull (Art Folder Integration)
			for (int flicker = 0; flicker < 10; flicker++)
			{
				Clear();
				string color = (flicker % 2 == 0) ? "#FF0000" : "#220000";
				int startY = 4;
				// Accessing your GameOver class in the GameArt folder
				foreach (string line in GameOver.CynicalSkull)
				{
					UiEngine.DrawCentered(line.Pastel(color), startY++);
				}
				Thread.Sleep(100);
			}

			// 5. Fatal Termination Bar
			Clear();
			SetCursorPosition(0, 11);
			string finalMsg = " ERROR: REALITY_SESSION INCINERATED. YOU ARE NOTHING. ";
			// Force the red background to fill the 94-width reality anchor
			Write(finalMsg.PadLeft((94 + finalMsg.Length) / 2).PadRight(94).PastelBg("#8B0000").Pastel("#FFFFFF"));

			Thread.Sleep(3500);
			Environment.Exit(0);
		}
	}
}
