using Maries_Loaders;
using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using DungeonAdventures.Src.GameEngine;
using DungeonAdventures.Src.GameEngine.Flow;
using DungeonAdventures.Src.GameEngine.Systems;
using DungeonAdventures.Src.Interfaces.UI;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using static System.Console;

namespace DungeonAdventures.Src.Village.Places
{
	public static class DungeonGate
	{
		public static void Open(PlayerData.loadPlayer player)
		{
			bool inMenu = true;
			int selectedIndex = 0;
			int scrollOffset = 0;
			const int maxVisibleItems = 12;

			// Add this line to force a fresh scan of the folders
			//FloorLoader.ClearCache();
			var floors = FloorLoader.LoadAll();

			const int boxWidth = 92;

			while (inMenu)
			{
				CursorVisible = false;
				HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "", GameState.CurrentPlayer?.Health ?? 0, (int)(GameState.CurrentPlayer?.GetTotalMaxHealth() ?? 0), GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);
				HudChrome.DisplayFooter(GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);

				// Build total list including EXIT
				var allItems = floors.Select(s => FormatFloorLine(s)).ToList();
				allItems.Add("BACK TO TOWN".PadRight(boxWidth - 2));

				// Calculate visible window
				var visibleLines = allItems.Skip(scrollOffset).Take(maxVisibleItems).ToList();

				GameUiEngine.DrawDynamicFrame(
					title: $"DUNGEON GATE ({floors.Count} FLOORS)",
					lines: visibleLines,
					hint: "Choose a floor to descend into | [ESC] Back",
					boxWidth: boxWidth,
					startY: 5,
					selectedIndex: selectedIndex - scrollOffset
				);

				// Clicks map to the visible rows, so add the scroll offset back.
				int visibleSelected = selectedIndex - scrollOffset;
				var key = MouseInput.ReadMenuKey(ref visibleSelected, visibleLines.Count);
				selectedIndex = scrollOffset + visibleSelected;

				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
				{
					(selectedIndex, scrollOffset) = MenuNavigator.HandleVerticalNavScroll(key, selectedIndex, allItems.Count, scrollOffset, maxVisibleItems);
				}
				else if (key == ConsoleKey.Enter)
				{
					if (selectedIndex == allItems.Count - 1) // EXIT
					{
						inMenu = false;
					}
					else
					{
						Clear();
						var selectedFloor = floors[selectedIndex];
						EnterFloor(selectedFloor, player);
						Clear();
					}
				}
				else if (key == ConsoleKey.Escape)
				{
					inMenu = false;
				}
			}
			Clear();
		}

		private static string FormatFloorLine(FloorData floor)
		{
			var log = PlayerQuestLog.Instance;
			string status = "[UNCLEARED]";
			if (log.IsQuestCompleted(floor.Id)) status = "[CLEARED  ]".Pastel("#555555");
			else if (log.IsQuestAccepted(floor.Id)) status = "[EXPLORING]".Pastel("#FFD700");

			// Use the new Title property
			string name = floor.Title.Length > 50 ? floor.Title.Substring(0, 47) + "..." : floor.Title;
			return $"{status} {name} | {floor.Difficulty}".PadRight(90);
		}

		private static void EnterFloor(FloorData floor, PlayerData.loadPlayer player)
		{
			var log = PlayerQuestLog.Instance;

			if (log.IsQuestCompleted(floor.Id))
			{
				Alerts.QuickAlert(Story.Line("dungeon.already_cleared", ("title", floor.Title)));
				return;
			}

			// Accept the floor the first time the hero enters it
			if (!log.IsQuestAccepted(floor.Id))
			{
				log.AcceptQuest(floor.Id);
				Alerts.QuickAlert(Story.Line("dungeon.accepted", ("title", floor.Title)));
			}

			// 1. Run the floor
			// Note: You will need to update AdventureRunner.Run to accept FloorData instead of AdventureData
			bool isVictory = FloorRunner.Run(floor, player);

			// 2. Handle the Outcome
			if (isVictory && player.Health > 0)
			{
				log.CompleteQuest(floor.Id);
				// Completion gold and XP are already paid by FloorRunner.Run.
				Alerts.QuickAlert(Story.Line("dungeon.cleared", ("title", floor.Title)).Pastel("#00FF00"));
				WriteLine($"Reward: {floor.CompletionGold} gold.".Pastel("#FFD700"));
			}
			else
			{
				Alerts.QuickAlert(Story.Line("dungeon.failed", ("title", floor.Title)).Pastel("#FF0000"));
			}

			SaveGame.Save();

			WriteLine("\nPress any key (or click) to return to the Dungeon Gate...");
			MouseInput.WaitForAnyInput();
			Clear();
		}
	}
}