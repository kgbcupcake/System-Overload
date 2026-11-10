using Maries_Loaders;
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
			string[] sideDecor = { " † ", " * ", " † ", " * " };
			for (int i = 0; i < sideDecor.Length; i++)
			{
				// Use Console.WindowWidth - 10 to keep them relative to the screen edge
				Console.SetCursorPosition(2, 7 + (i * 3));
				Console.Write(sideDecor[i].Pastel("#8B5A2B"));

				Console.SetCursorPosition(Console.WindowWidth - 11, 7 + (i * 3));
				Console.Write(sideDecor[i].Pastel("#8B5A2B"));
			}

			// 1. TOP ACCENTS
			string topAccent = "◢◤".Pastel("#FFAB00") + " —————————————————————————————————————— " + "◥◣".Pastel("#FFAB00");
			Console.SetCursorPosition(Math.Max(0, centerX - (UiEngine.StripAnsi(topAccent).Length / 2)), startY - 2);
			Console.Write(topAccent);

			// 2. 3D ICON (the words inside come from the "save_loaded.box" story lines)
			var boxText = Story.Lines("save_loaded.box");
			string BoxLine(int i) => UiEngine.PadAnsiStringWithCenter(i < boxText.Count ? boxText[i] : "", 22);
			string icon =
				"      _______________________\n" +
				"     /                      /|\n" +
				$"    /{BoxLine(0)}/ |\n" +
				$"   /{BoxLine(1)}/  |\n" +
				"  /______________________/   |\n" +
				"  |                      |   |\n" +
				$"  |{BoxLine(2)}|   |\n" +
				"  |______________________|  /";

			string[] lines = icon.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			int currentY = startY;
			foreach (string line in lines)
			{
				Console.SetCursorPosition(centerX - 13, currentY++);
				string color = (currentY % 2 == 0) ? "#FFAB00" : "#B8860B";
				Console.Write(line.Pastel(color));
			}

			// 3. PLAYER DATA BLOCK
			currentY += 1;
			string bracketL = ">> [".Pastel("#B8860B");
			string bracketR = "] <<".Pastel("#B8860B");
			string nameDisp = playerName.ToUpper().Pastel("#FFD700");
			string fullRow = $"{bracketL} {nameDisp} {bracketR}";

			// Using your new UiEngine helper to calculate centering
			int textLen = UiEngine.StripAnsi(fullRow).Length;
			Console.SetCursorPosition(centerX - (textLen / 2), currentY);
			Console.WriteLine(fullRow);

			// 4. METRICS
			currentY += 3;
			var metrics = Story.Lines("save_loaded.status");
			foreach (var m in metrics)
			{
				Console.SetCursorPosition(centerX - (m.Length / 2), currentY++);
				Console.Write(m.Pastel("#333333"));
			}

			// 5. THE PULSE PROMPT
			string prompt = Story.Line("save_loaded.prompt");
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
				string noise = Story.Pick("load_save.scan", ("n", rnd.Next(1, 999))).PadRight(40);
				Console.Write(noise.Pastel("#B8860B"));
				Thread.Sleep(90);
			}

			var steps = Story.Lines("load_save.steps");

			// 2. PHASE TWO: THE PROGRESS BAR + RAIN
			for (int i = 0; i < steps.Count; i++)
			{
				Console.SetCursorPosition(centerX - 25, centerY - 2);
				Console.Write(new string(' ', 50));
				Console.SetCursorPosition(centerX - (steps[i].Length / 2), centerY - 2);
				Console.Write(steps[i].Pastel("#FFAB00"));

				for (int p = 0; p <= 10; p++)
				{
					// --- UPDATE FALLING EMBERS ---
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
						string bit = rnd.Next(2) == 0 ? "*" : "·";
						// Alternate between bright and dim embers for depth
						string color = rnd.Next(3) == 0 ? "#FF8C00" : "#5A2A00";
						Console.Write(bit.Pastel(color));
					}

					// --- UPDATE PROGRESS BAR ---
					float totalPercent = ((i * 10) + p) / (steps.Count * 10f);
					ProgressBarRenderer.Draw(centerX - (barWidth / 2), centerY, totalPercent, barWidth, "#FFAB00", "#222222");

					Thread.Sleep(rnd.Next(30, 80));
				}
			}

			// 3. FINAL LOCK-ON
			Console.SetCursorPosition(centerX - 10, centerY + 2);
			Console.Write(Story.Line("load_save.done").Pastel("#FFD700"));

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
				string glitchRed = rnd.Next(0, 10) > 8 ? "#4A0000" : "#FF0000";
				UiEngine.DrawCentered(topContent.Pastel(glitchRed), centerY - 5);

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
			Console.ReadKey(true);

			Console.ResetColor();
			Console.Clear();
		}

		public static void ShowCreationAnimation(string playerName, string playerClass, int health, int hitPoints, int coins, bool isDevMode, string devIteration, string buildVersion)
		{
			// --- DYNAMIC THEME ---
			string themeColor = playerClass switch
			{
				"Warrior" or "Knight" or "Paladin" or "Berserker" => "#FF4500",
				"Mage" or "Wizard" or "Warlock" or "Battlemage" => "#00BFFF",
				"Rogue" or "Assassin" or "Shadowblade" or "Ranger" => "#9370DB",
				_ => "#00FF00"
			};

			var steps = Story.Lines("creation.steps", ("name", playerName), ("class", playerClass));

			Console.Clear();
			HudChrome.TitleBar(true, playerName, health, hitPoints, coins, isDevMode);
			UiEngine.DrawCentered(Story.Line("creation.header").Pastel(themeColor), 4);
			HudChrome.DisplayFooter(isDevMode, devIteration, buildVersion);

			int startY = 7;
			int centerX = (Console.WindowWidth / 2) - 20;

			for (int i = 0; i < steps.Count; i++)
			{
				Console.SetCursorPosition(centerX, startY + i);
				Console.Write("[ ".Pastel("#555555") + " .. ".Pastel("#FFD700") + " ] ".Pastel("#555555") + steps[i]);

				// We removed the SaveData logic from here.
				// The data is already synced by GameState.Sync() before this starts.

				Thread.Sleep(new Random().Next(300, 600));

				Console.SetCursorPosition(centerX, startY + i);
				Console.Write("[ ".Pastel("#555555") + " ok ".Pastel("#00FF00") + " ] ".Pastel("#555555") + steps[i]);
			}

			// --- FINAL FEEDBACK ---
			var footer = Story.Lines("creation.footer", ("name", playerName), ("class", playerClass));
			for (int f = 0; f < footer.Count; f++)
			{
				Console.SetCursorPosition(centerX, startY + steps.Count + 1 + f);
				Console.WriteLine($"» {footer[f]}".Pastel(f == footer.Count - 1 ? themeColor : "#555555"));
			}

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

			// 1. Farewell lines
			Clear();
			var tasks = Story.Lines("shutdown.tasks");
			string pending = Story.Line("shutdown.task_pending");
			string done = Story.Line("shutdown.task_done");
			string suffix = Story.Line("shutdown.task_suffix");

			for (int i = 0; i < tasks.Count; i++)
			{
				SetCursorPosition(4, 5 + i);
				Write($"{pending} {tasks[i]}".Pastel("#555555"));
				Thread.Sleep(1000);
				SetCursorPosition(4, 5 + i);
				Write($"{done} {tasks[i]}".Pastel("#8B0000"));
				WriteLine($" {suffix}".Pastel("#444444"));
			}
			Thread.Sleep(1000);

			// 2. Falling ash + warnings
			for (int i = 0; i < 15; i++)
			{
				Clear();
				for (int r = 0; r < 20; r++) // Ash background
				{
					string rain = new string(' ', rnd.Next(2, 80)) + (rnd.Next(2) == 0 ? "*" : "·");
					SetCursorPosition(0, r);
					Write(rain.Pastel("#3A2A1A")); // Dim ash
				}

				UiEngine.DrawCentered(Story.Line("shutdown.warning").Pastel("#FF0000"), 8);
				UiEngine.DrawCentered(Story.Line("shutdown.whisper").Pastel("#555555"), 10);
				Thread.Sleep(80);
			}

			// 3. The Purple Loading Bar
			int barWidth = 40;
			for (int p = 0; p <= 100; p += 2)
			{
				ProgressBarRenderer.Draw(27, 12, p / 100.0, barWidth, "#800080", "#333333");

				if (p >= 40)
				{
					UiEngine.DrawCentered(Story.Line("shutdown.doom").Pastel("#FF0000"), 15);
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
				// Skull art from GameArt, with the caption from the story data
				var art = new List<string> { Story.Line("game_over.header") };
				art.AddRange(GameOver.Skull);
				art.Add(Story.Line("game_over.footer"));
				foreach (string line in art)
				{
					UiEngine.DrawCentered(line.Pastel(color), startY++);
				}
				Thread.Sleep(100);
			}

			// 5. Fatal Termination Bar
			Clear();
			SetCursorPosition(0, 11);
			string finalMsg = $" {Story.Line("shutdown.final")} ";
			// Force the red background to fill the 94-width reality anchor
			Write(finalMsg.PadLeft((94 + finalMsg.Length) / 2).PadRight(94).PastelBg("#8B0000").Pastel("#FFFFFF"));

			Thread.Sleep(3500);
			Environment.Exit(0);
		}
	}
}
