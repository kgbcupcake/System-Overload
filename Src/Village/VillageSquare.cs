using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine.Interfaces;
using DungeonAdventures.Src.GameEngine.Systems;
using DungeonAdventures.Src.Interfaces.UI;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using DungeonAdventures.Src.Village.Places;
using DungeonAdventures.Src.Game.Interfaces;
using DungeonAdventures.Src.Lpc;
using DungeonAdventures.Src.Scenes;
using DungeonAdventures.Src.Village.Street;
using System.Numerics;
using static System.Console;

namespace DungeonAdventures.Src.Village
{
	internal class VillageSquare
	{
		private const string ReloadContentOption = "RELOAD CONTENT (DEV)";

		// The id quests use for the village itself, e.g. "Return to Ashford".
		private const string VillageLocationId = "ashford";

		// The location id for the hero's house, used by points of interest.
		private const string HomeLocationId = "home";

		private const string WalkOption = "WALK THE STREET";

		/// <summary>
		/// The village. With the LPC art there, it's a street to walk (<see cref="VillageStreet"/>), with Tab to
		/// this list menu and WALK THE STREET back; without it, just the list menu.
		/// </summary>
		public static void MainTownSquare()
		{
			bool isRunning = true;
			var player = GameState.CurrentPlayer;
			var look = player?.Look ?? (LpcMeta.Available ? LpcLook.Default() : null);
			bool street = player != null && Scene.CanCast(look);

			while (isRunning)
			{
				if (street)
				{
					QuestSystem.Notify(player!, "discover_location", VillageLocationId);
					QuestScreens.ShowNotices();
					VillageStreet.Run(player!, look!, LocalHeader, option => ExecuteSelection(option, ref isRunning), VillageLocationId);
				}
				if (!isRunning) break;
				Clear(); // the street's picture shouldn't show behind the list
				ListMenu(ref isRunning, street);
			}
		}

		/// <summary>The village as a list of places. Returns when WALK THE STREET (or Escape) is chosen.</summary>
		private static void ListMenu(ref bool isRunning, bool street)
		{
			int selectedIndex = 0;

			string[] options =
			{
				"HOME",
				"TALK TO VILLAGERS",
				"GENERAL STORE",
				"ROAD TO HOLLOWMERE",
				"BLACKSMITH",
				"HEALER",
				"JOURNAL",
				"CHARACTER",
				"INVENTORY",
				"LEAVE THE REALM"
			};
			// Dev builds get an extra option to pick up content edited in MariesGui without restarting.
			if (GameState.IsDevMode)
				options = options.Append(ReloadContentOption).ToArray();
			if (street)
				options = options.Prepend(WalkOption).ToArray();
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			const int boxWidth = 92;
			int contentWidth = boxWidth - 2;

			while (isRunning)
			{
			RedrawMenu:
				CursorVisible = false;

				// Being in the village counts as reaching it for quests ("discover_location" ashford).
				if (GameState.CurrentPlayer != null)
					QuestSystem.Notify(GameState.CurrentPlayer, "discover_location", VillageLocationId);

				// Quest news from wherever the hero just was (a floor, a shop...) comes first.
				QuestScreens.ShowNotices();

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

					case ConsoleKey.Escape when street:
						Clear();
						return;

					case ConsoleKey.Enter:
						if (options[selectedIndex] == WalkOption)
						{
							Clear();
							return;
						}
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
				case "HOME":
					// Things to examine at the hero's house come from the "points_of_interest" data.
					if (GameState.CurrentPlayer == null ||
						PeopleAndPlacesLoader.PlacesAt(HomeLocationId, f => QuestSystem.HasFlag(GameState.CurrentPlayer, f)).Count == 0)
					{
						Alerts.QuickAlert("The house is quiet. There's nothing to look at here right now.");
						break;
					}
					Clear();
					PeopleAndPlacesScreens.LookAround(HomeLocationId, "HOME");
					break;

				case "TALK TO VILLAGERS":
					Clear();
					PeopleAndPlacesScreens.TalkMenu(VillageLocationId, Story.Line("town.name"));
					break;

				case "GENERAL STORE":
					Clear();
					Store.LoadShop();
					break;

				case "ROAD TO HOLLOWMERE":
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

				case "JOURNAL":
					QuestScreens.Journal();
					break;

				case "CHARACTER":
					Clear();
					Stats.PlayerStats();
					break;

				case "INVENTORY":
					Clear();
					InventoryScreen.Open(GameState.CurrentPlayer);
					break;




				case ReloadContentOption:
					ContentReload.ReloadAll();
					Alerts.QuickAlert("Content reloaded from Realm Data.");
					break;

				case "LEAVE THE REALM":
					ShutdownSequence.Play();
					//isRunning = false;
					break;

				default:
					Alerts.QuickAlert(Story.Pick("town.closed", ("place", selection)));
					break;
			}
		}

		internal static void LocalHeader()
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
