using System_Overload.Src.GameData.Components;
using System_Overload.Src.GameEngine.Interfaces;
using System_Overload.Src.Interfaces.UI;
using Maries_Ui;
using Pastel;
using Src.TownSquare.InterFaces;
using System.Numerics;
using static System.Console;

namespace Src.TownSquare
{
	internal class TheForgottenZone
	{

		public static void MainTownSquare()
		{
			bool isRunning = true;
			int selectedIndex = 0;

			string[] options =
			{
				"DATA EXCHANGE",      // Was STORE
				"BREACH TERMINAL",    // Was BOUNTY BOARD
				"SIMULATION GATE",    // Was DUNGEON
				"HARDWARE TECH",      // Was BLACK SMITH
				"INTEGRITY REPAIR",   // Was MED CLINIC
				"BIO-SIGNATURE",      // Was PLAYER STATS
				"MEMORY DECK",
				"DISCONNECT"
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
					title: "THE FORGOTTEN OUTPOST",
					lines: paddedOptions,
					hint: "Use arrow keys to navigate",
					boxWidth: boxWidth,
					selectedIndex: selectedIndex
				);

				ConsoleKey key = ReadKey(true).Key;

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
				case "DATA EXCHANGE":
					Clear();
					Store.LoadShop();
					break;

				case "BREACH TERMINAL":
					if (GameState.CurrentPlayer == null)
					{
						Alerts.QuickAlert("IDENTITY ERROR: No active bio-signature found.");
						break;
					}
					Clear();
					SectorBreachTerminal.Open(GameState.CurrentPlayer);
					Clear();
					break;

				case "HARDWARE TECH":
					Clear();
					HardwareTech.Open();
					break;

				case "BIO-SIGNATURE":
					Clear();
					Stats.PlayerStats();
					break;

				case "MEMORY DECK":
					Clear();
					MemoryDeck.Open(GameState.CurrentPlayer);
					break;




				case "DISCONNECT":
					Transitions.ShutdownSequence();
					//isRunning = false;
					break;

				default:
					Alerts.QuickAlert($"{selection} is currently unstable in this reality.");
					break;
			}
		}

		private static void LocalHeader()
		{
			var p = GameState.CurrentPlayer;
			Console.SetCursorPosition(0, 0);

			// .Pastel sets the text color, .PastelBg sets the background color
			// This creates the white text on a dark red bar from your screenshots
			string loc = " THE FORGOTTEN ZONE ".Pastel("#FFFFFF").PastelBg("#8B0000");

			// This creates the yellow/red icons on a transparent background
			string stats = $" 👤 {p.PlayerName} | ❤️ HP: {p.Health}/{p.HitPoints} | 💰 {p.Coins} "
							.Pastel("#FFD700");

			// Clear the line first to prevent old text bleeding through
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, 0);

			// Print the combined string
			Console.Write(loc + " " + stats);
		}






	}
}
