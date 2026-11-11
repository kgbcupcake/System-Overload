using Maries_Loaders;
using Pastel;
using System.Text;

using static System.Console;

namespace Maries_Ui
{
	/// <summary>The animation after a new hero is made.</summary>
	public static class CreationAnimation
	{
		public static void Play(string playerName, string playerClass, int health, int hitPoints, int coins, bool isDevMode, string devIteration, string buildVersion)
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
	}
}
