using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Interfaces.UI;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using static System.Console;
using System.Collections.Generic;

namespace DungeonAdventures.Src.Game.MainInterfaces
{
	internal class CharSubMenus
	{
		public static void ChooseSubClass()
		{
			var player = GameState.CurrentPlayer;
			string[] subClasses;
			string[] descriptions;
			string flavorColor;

			// Each class branches into three paths. Their descriptions live in the story data
			// under "class.<path>" (for example "class.knight").
			switch (player.PlayerClass)
			{
				case "Warrior":
					subClasses = new string[] { "Knight", "Paladin", "Berserker" };
					flavorColor = "#FF4500"; // Ember
					break;
				case "Rogue":
					subClasses = new string[] { "Assassin", "Shadowblade", "Ranger" };
					flavorColor = "#9370DB"; // Dusk purple
					break;
				case "Mage":
					subClasses = new string[] { "Wizard", "Warlock", "Battlemage" };
					flavorColor = "#00BFFF"; // Arcane blue
					break;
				default: return;
			}
			descriptions = subClasses.Select(s => Story.Line($"class.{s.ToLower()}")).ToArray();

			int selected = 0;
			while (true)
			{
				Clear();

				List<string> evolutionOptions = new List<string>();
				for (int i = 0; i < subClasses.Length; i++)
				{
					// Center the text manually to prevent ANSI color drift from breaking the box borders
					int visualLength = subClasses[i].Length;
					int leftSpaceCount = (92 / 2) - (visualLength / 2);
					int rightSpaceCount = 92 - visualLength - leftSpaceCount;

					string centeredOption = new string(' ', leftSpaceCount) + subClasses[i] + new string(' ', rightSpaceCount);
					evolutionOptions.Add(centeredOption);
				}

				// Draw the UI frame using the standard Width (94) for consistency
				GameUiEngine.DrawDynamicFrame($"CHOOSE YOUR PATH: {player.PlayerName.ToUpper()}",
					evolutionOptions,
					"[UP/DOWN] to browse | [ENTER] or click to choose",
					boxWidth: 94,
					selectedIndex: selected);

				UiEngine.DrawCentered(Story.Line("creation.subclass_prompt", ("class", player.PlayerClass)).Pastel(flavorColor), 6);

				int contentBaseY = 20;
				UiEngine.DrawCentered(" THE PATH ".Pastel("#000000").PastelBg(flavorColor), contentBaseY);
				UiEngine.DrawCentered(descriptions[selected].Pastel("#DCDCDC"), contentBaseY + 2);

				var key = MouseInput.ReadMenuKey(ref selected, subClasses.Length);
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow) selected = MenuNavigator.HandleVerticalNav(key, selected, subClasses.Length);
				else if (key == ConsoleKey.Enter)
				{
					player.PlayerClass = subClasses[selected];
					break;
				}
			}
		}
	}
}