using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine.Interfaces;
using DungeonAdventures.Src.Interfaces.UI;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using DungeonAdventures.Src.Village.Places;
using System.Numerics;
using static System.Console;

namespace DungeonAdventures.Src.Village
{
	internal class VillageSquare
	{

		public static void MainTownSquare()
		{
			bool isRunning = true;
			int selectedIndex = 0;

			string[] options =
			{
				"GENERAL STORE",
				"DUNGEON GATE",
				"BLACKSMITH",
				"HEALER",
				"CHARACTER",
				"INVENTORY",
				"LEAVE THE REALM"
			};
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			const int boxWidth = 92;
			int contentWidth = boxWidth - 2;

			while (isRunning)
			{
			RedrawMenu:
				CursorVisible = false;

				LocalHeader();
				HudChrome.DisplayFooter(GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);

				// 🔧 FIX: pad every line to box width
				var paddedOptions = options
					.Select(o => o.PadRight(contentWidth))
					.ToList();

				GameUiEngine.DrawDynamicFrame(
					title: Story.Line("town.title"),
					lines: paddedOptions,
					hint: "Use arrow keys or click to choose",
					boxWidth: boxWidth,
					selectedIndex: selectedIndex
				);

				ConsoleKey key = MouseInput.ReadMenuKey(ref selectedIndex, options.Length);

				switch (key)
				{
					case ConsoleKey.UpArrow:
					case ConsoleKey.DownArrow:
						selectedIndex = MenuNavigator.HandleVerticalNav(key, selectedIndex, options.Length);
						break;

					case ConsoleKey.Enter:
						ExecuteSelection(options[selectedIndex], ref isRunning);
						if (isRunning) goto RedrawMenu;
						break;
				}
			}
		}

		private static void ExecuteSelection(string selection, ref bool isRunning)
		{
			switch (selection)
			{
				case "GENERAL STORE":
					Clear();
					Store.LoadShop();
					break;

				case "DUNGEON GATE":
					if (GameState.CurrentPlayer == null)
					{
						Alerts.QuickAlert("No hero is loaded.");
						break;
					}
					Clear();
					DungeonGate.Open(GameState.CurrentPlayer);
					Clear();
					break;

				case "BLACKSMITH":
					Clear();
					Blacksmith.Open();
					break;

				case "CHARACTER":
					Clear();
					Stats.PlayerStats();
					break;

				case "INVENTORY":
					Clear();
					InventoryScreen.Open(GameState.CurrentPlayer);
					break;




				case "LEAVE THE REALM":
					Transitions.ShutdownSequence();
					//isRunning = false;
					break;

				default:
					Alerts.QuickAlert(Story.Pick("town.closed", ("place", selection)));
					break;
			}
		}

		private static void LocalHeader()
		{
			var p = GameState.CurrentPlayer;
			Console.SetCursorPosition(0, 0);

			// .Pastel sets the text color, .PastelBg sets the background color
			// This creates the white text on a dark red bar from your screenshots
			string loc = $" {Story.Line("town.name")} ".Pastel("#FFFFFF").PastelBg("#8B0000");

			// This creates the yellow/red icons on a transparent background
			string stats = $" 👤 {p.PlayerName} | ❤️ HP: {p.Health}/{(int)p.GetTotalMaxHealth()} | 💰 {p.Coins} "
							.Pastel("#FFD700");

			// Clear the line first to prevent old text bleeding through
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, 0);

			// Print the combined string
			Console.Write(loc + " " + stats);
		}






	}
}
