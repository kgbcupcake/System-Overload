using System_Overload.Src.GameData.Components;
using System_Overload.Src.Utilities.UI;
using Pastel;
using static System.Console;
using System.Collections.Generic;

namespace System_Overload.Src.Game.MainInterfaces
{
	internal class CharSubMenus
	{
		public static void ChooseSubClass()
		{
			var player = GameState.CurrentPlayer;
			string[] subClasses;
			string[] descriptions;
			string flavorColor;

			// Logic: Map the new Neural Archetypes to specialized sub-routines
			switch (player.PlayerClass)
			{
				case "Aegis-Protocol":
					subClasses = new string[] { "Titan-Shell", "Bastion-Core", "Sentinel-v2" };
					flavorColor = "#00FFFF"; // Cyan
					descriptions = new string[] { "Heavy kinetic plating. Unmatched integrity.", "Energy shielding and recursive repair.", "Area denial and crowd suppression." };
					break;
				case "Cipher-Blade":
					subClasses = new string[] { "Ghost-Code", "Neural-Reaper", "Phase-Shift" };
					flavorColor = "#00FF00"; // Neon Green
					descriptions = new string[] { "Stealth-based infiltration. Critical strikes.", "Rapid multi-target data-corruption.", "High mobility and evasion modules." };
					break;
				case "System-Architect":
					subClasses = new string[] { "Logic-Bombard", "Overclocker", "Root-Admin" };
					flavorColor = "#BC1DBC"; // Purple
					descriptions = new string[] { "High-output energy discharge attacks.", "Sacrifice stability for massive burst damage.", "Manipulate simulation laws and loot drops." };
					break;
				default: return;
			}

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
				UiEngine.DrawDynamicFrame($"SPEC-EVOLUTION: {player.PlayerName.ToUpper()}",
					evolutionOptions,
					"[UP/DOWN] to browse | [ENTER] to confirm specialization",
					boxWidth: 94,
					selectedIndex: selected);

				UiEngine.DrawCentered($"Select Neural Specialization for {player.PlayerClass}".Pastel(flavorColor), 6);

				int contentBaseY = 20;
				UiEngine.DrawCentered(" SUB-ROUTINE SPECS ".Pastel("#000000").PastelBg(flavorColor), contentBaseY);
				UiEngine.DrawCentered(descriptions[selected].Pastel("#DCDCDC"), contentBaseY + 2);

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow) selected = (selected == 0) ? subClasses.Length - 1 : selected - 1;
				else if (key == ConsoleKey.DownArrow) selected = (selected == subClasses.Length - 1) ? 0 : selected + 1;
				else if (key == ConsoleKey.Enter)
				{
					player.PlayerClass = subClasses[selected];
					break;
				}
			}
		}
	}
}