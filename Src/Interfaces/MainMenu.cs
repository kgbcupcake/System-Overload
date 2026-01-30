using System_Overload.Src.Game.Interfaces;
using System_Overload.Src.GameData.Components;
using System_Overload.Src.Utilities.GameArt;
using System_Overload.Src.Utilities.UI;
using Pastel;
using Src.TownSquare;
using System.Runtime.InteropServices;
using static System.Console;

namespace System_Overload.Src.GameEngine.Interfaces
{
	internal class MainMenu
	{

		public static void Show()
		{
			GameState.EnsureDirectories();
			var conductor = new Conductor();
			bool isMirrorHealthy = SaveGame.RunSanityCheck();
			CursorVisible = false;

			bool isRunning = true;
			int selectedIndex = 0;
			string[] options = { "START NEW GAME", "LOAD SAVE", "CREDITS", "EXIT" };
			OutputEncoding = System.Text.Encoding.UTF8;

			// --- INTEGRATED STANDARDIZED TITLE BAR ---
			Clear();
			UiFunctions.TitleBar();

			// --- ATMOSPHERIC BACKGROUND TEXT ---
			// Shifted slightly to ensure the new icon-based TitleBar at Row 0 doesn't overlap
			UiEngine.DrawCentered("--------------------------------------------".Pastel("#4A0000"), 3);
			UiEngine.DrawCentered("SYSTEM // CORRUPTED // REALITY // FRAGMENTED".Pastel("#4A0000"), 4);
			UiEngine.DrawCentered("--------------------------------------------".Pastel("#4A0000"), 18);
			UiEngine.DrawCentered("ERROR // SECTOR // UNSTABLE // ENTITY".Pastel("#4A0000"), 19);
			UiEngine.DrawCentered("--------------------------------------------".Pastel("#4A0000"), 20);
			MainMenuArt.DrawMainHeader();
			

			while (isRunning)
			{
			RedrawMenu:
				bool saveExists = CheckForSaves();

		
				RenderMenu(options, selectedIndex, saveExists);

				while (!KeyAvailable)
				{
					UpdateIdleAnimation(saveExists);
					Thread.Sleep(50);
				}

				ConsoleKeyInfo keyInfo = ReadKey(true);


				switch (keyInfo.Key)
				{
					case ConsoleKey.UpArrow:
						if (selectedIndex > 0)
						{
							selectedIndex--;
						}
						break;
					case ConsoleKey.DownArrow:
						if (selectedIndex < options.Length - 1)
						{
							selectedIndex++;
						}
						break;
					case ConsoleKey.Enter:
						isRunning = ExecuteSelection(options[selectedIndex], saveExists);
						if (!isRunning) return;

						Write("\x1b[2J\x1b[3J\x1b[H");
						break;
				}
				RenderMenu(options, selectedIndex, saveExists);
			}
		}

		private static void RenderMenu(string[] options, int selectedIndex, bool saveExists)
		{
			const int boxWidth = 42;
			const int startY = 12;

			var menuItems = new List<string>();

			for (int i = 0; i < options.Length; i++)
			{
				menuItems.Add(BuildMenuItem(i, selectedIndex, saveExists, options));
			}

			UiEngine.DrawDynamicFrame("MAIN MENU", menuItems, "Use Arrows & Enter", boxWidth: boxWidth, startY: startY);
		}

		private static string BuildMenuItem(int index, int selectedIndex, bool saveExists, string[] options)
		{
			if (index >= options.Length) return "";

			string option = options[index];
			bool isSelected = (index == selectedIndex);

			if (option.Trim().ToUpper() == "LOAD SAVE" && !saveExists)
			{
				return UiEngine.PadAnsiStringWithCenter("🔒 LOAD SAVE 🔒".Pastel("#8B0000"), 40);
			}

			if (isSelected)
			{
				string selectedColor = ((int)(DateTime.Now.TimeOfDay.TotalMilliseconds / 250) % 2 == 0) ? "#FFD700" : "#FFA500";
				return UiEngine.PadAnsiStringWithCenter($"> {option} <".Pastel(selectedColor), 40);
			}

			return UiEngine.PadAnsiStringWithCenter(option.Pastel("#4A0000"), 40);
		}

		private static void UpdateIdleAnimation(bool saveExists)
		{
			string status = saveExists ? "[ SAVES DETECTED ]" : "[ NO SAVES FOUND ]";
			bool statusVisible = ((int)(DateTime.Now.TimeOfDay.TotalSeconds * 2.0) % 2 == 0);
			UiEngine.DrawCentered(statusVisible ? status.Pastel(saveExists ? "#00FF00" : "#FF0000") : "                     ", 22);
		}

		private static bool CheckForSaves()
		{
			string activeFolder = GameState.GetActiveProfileFolder();
			if (Directory.Exists(activeFolder))
			{
				return Directory.GetFiles(activeFolder, "*.json").Length > 0;
			}
			return false;
		}

		private static bool ExecuteSelection(string selection, bool saveExists)
		{
			switch (selection)
			{
				case "START NEW GAME":
					Clear();
					CharacterCreation.Start();
					if (GameState.CurrentPlayer != null)
					{
						GameState.Sync(); // Ensure character is written to profiles immediately
						UiFunctions.TitleBar();
						TheForgottenZone.MainTownSquare();
					}
					return false;

				case "LOAD SAVE":
					if (saveExists)
					{
						Clear();
						if (CharacterLoadScreen.ShowLoadMenu())
						{
							UiFunctions.TitleBar();
							return false;
						}
					}
					return true;

				case "CREDITS":
					ShowCredits();
					return true;

				case "EXIT":
					UiFunctions.ShutdownSequence();
					return false;

				default:
					return true;
			}
		}

		private static void ShowCredits()
		{
			Clear();
			UiEngine.DrawCentered("CREATED BY: YOUR NAME".Pastel("#FFAB00"), 10);
			UiEngine.DrawCentered("V2 ENGINE POWERED BY C# & IMGUI", 12);
			UiEngine.DrawCentered("Press any key to return...", 16);
			ReadKey(true);
			Clear();
		}
	}
}