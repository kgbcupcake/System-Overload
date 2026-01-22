using System_Overload.Src.GameData.Components;
using System_Overload.Src.Utilities.UI;
using Src.TownSquare;
using System.Text.Json;
using static System_Overload.Src.GameData.Entities.PlayerData;
using static System.Console;
using System.Text.RegularExpressions;

namespace System_Overload.Src.GameEngine.Interfaces
{
	public class CharacterLoadScreen
	{
		private static readonly Regex AnsiRegex =
			new(@"\x1B\[[^@-~]*[@-~]", RegexOptions.Compiled);

		private static string PadAnsi(string input, int width)
		{
			string clean = System.Text.RegularExpressions.Regex
				.Replace(input, @"\x1B\[[^@-~]*[@-~]", "");

			int pad = Math.Max(0, width - clean.Length);
			return input + new string(' ', pad);
		}


		public static bool ShowLoadMenu()
		{
			string activeFolder = GameState.GetActiveProfileFolder();
			if (!Directory.Exists(activeFolder)) Directory.CreateDirectory(activeFolder);

			string[] files = Directory.GetFiles(activeFolder, "*.json");

			List<string> displayNames = files
				.Select(f => Path.GetFileNameWithoutExtension(f)
				.Replace("_", " ")
				.ToUpper())
				.ToList();

			int selectedIndex = 0;

			while (true)
			{
				loadPlayer preview = new loadPlayer();

				if (files.Length > 0)
				{
					try
					{
						string json = File.ReadAllText(files[selectedIndex]);
						preview = JsonSerializer.Deserialize<loadPlayer>(
							json,
							new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
						) ?? new loadPlayer();
					}
					catch
					{
						preview.PlayerName = "CORRUPT DATA";
					}
				}

				string title = displayNames.Count > 0
					? $"HERO: {preview.PlayerName.ToUpper()} | LVL: {preview.Level} | XP: {preview.Experience}/{preview.ExperienceToLevel}"
					: "CHARACTER SELECTION: EMPTY";

				RenderCharacterMenu(title, displayNames, selectedIndex, preview);

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.Escape) return false;
				if (displayNames.Count == 0) continue;

				switch (key)
				{
					case ConsoleKey.UpArrow:
						selectedIndex = selectedIndex == 0 ? displayNames.Count - 1 : selectedIndex - 1;
						break;

					case ConsoleKey.DownArrow:
						selectedIndex = selectedIndex == displayNames.Count - 1 ? 0 : selectedIndex + 1;
						break;

					case ConsoleKey.Enter:
						GameState.CurrentPlayer = preview;
						UiFunctions.LoadSaveProgress();
						UiFunctions.ShowSaveLoadedIcon(preview.PlayerName);
						Thread.Sleep(800);
						TheForgottenZone.MainTownSquare();
						return true;
				}
			}
		}

		private static void RenderCharacterMenu(
	string title,
	List<string> displayNames,
	int selectedIndex,
	loadPlayer preview)
		{
			string cleanTitle = System.Text.RegularExpressions.Regex
				.Replace(title, @"\x1B\[[^@-~]*[@-~]", "");

			const int boxWidth = 70;
			int innerWidth = boxWidth - 2;

			// Normalize lines for THIS screen ONLY
			List<string> safeLines = displayNames.Count > 0
				? displayNames
					.Select(name => PadAnsi(name, innerWidth))
					.ToList()
				: new List<string> { PadAnsi("NO SAVES FOUND", innerWidth) };

			Clear();

			UiEngine.DrawDynamicFrame(
				cleanTitle,
				safeLines,
				"ARROWS to move | ENTER to select | ESC to cancel",
				boxWidth: boxWidth,
				startY: 5,
				selectedIndex: selectedIndex,
				previewHp: preview.Health,
				previewCoins: preview.Coins
			);
		}

	}
}
