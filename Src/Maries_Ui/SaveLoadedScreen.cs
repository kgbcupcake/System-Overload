using Maries_Loaders;
using Pastel;
using System.Text;

using static System.Console;

namespace Maries_Ui
{
	/// <summary>The "save loaded" card shown after picking a save: the hero's name, health and coins.</summary>
	public static class SaveLoadedScreen
	{
		public static void Show(string playerName, int health, int hitPoints, int coins, bool isDevMode, string devIteration, string buildVersion)
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

			while (!MouseInput.InputAvailable)
			{
				UiEngine.DrawCentered(prompt.Pastel("#FFFFFF"), promptY, 50);
				Thread.Sleep(500);
				if (MouseInput.InputAvailable) break;
				UiEngine.DrawCentered(prompt.Pastel("#444444"), promptY, 50);
				Thread.Sleep(250);
			}

			MouseInput.WaitForAnyInput();
			Clear();
		}
	}
}
