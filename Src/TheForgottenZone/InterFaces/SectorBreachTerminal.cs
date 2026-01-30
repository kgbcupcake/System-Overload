using System_Overload.GameEngine.Systems.Loaders;
using System_Overload.Src.GameData.Entities;
using System_Overload.Src.GameEngine;
using System_Overload.Src.GameEngine.Flow;
using System_Overload.Src.GameEngine.Systems;
using System_Overload.Src.Utilities.UI;
using Pastel;
using static System.Console;

namespace Src.TownSquare.InterFaces
{
	public static class SectorBreachTerminal
	{
		public static void Open(PlayerData.loadPlayer player)
		{
			bool inMenu = true;
			int selectedIndex = 0;
			int scrollOffset = 0;
			const int maxVisibleItems = 12;

			// Add this line to force a fresh scan of the folders
			//SectorLoader.ClearCache();
			var sectors = SectorLoader.LoadAll();

			const int boxWidth = 92;

			while (inMenu)
			{
				CursorVisible = false;
				UiFunctions.TitleBar();
				UiFunctions.DisplayFooter();

				// Build total list including EXIT
				var allItems = sectors.Select(s => FormatSectorLine(s)).ToList();
				allItems.Add("EXIT TERMINAL".PadRight(boxWidth - 2));

				// Calculate visible window
				var visibleLines = allItems.Skip(scrollOffset).Take(maxVisibleItems).ToList();

				UiEngine.DrawDynamicFrame(
					title: $"SECTOR BREACH TERMINAL ({sectors.Count} NODES DETECTED)",
					lines: visibleLines,
					hint: "Select a Sector to begin Breach Protocol | [ESC] Back",
					boxWidth: boxWidth,
					startY: 5,
					selectedIndex: selectedIndex - scrollOffset
				);

				var key = ReadKey(true).Key;

				if (key == ConsoleKey.UpArrow)
				{
					selectedIndex--;
					if (selectedIndex < 0) selectedIndex = allItems.Count - 1;
					if (selectedIndex < scrollOffset) scrollOffset = selectedIndex;
					if (selectedIndex == allItems.Count - 1) scrollOffset = Math.Max(0, allItems.Count - maxVisibleItems);
				}
				else if (key == ConsoleKey.DownArrow)
				{
					selectedIndex++;
					if (selectedIndex >= allItems.Count) selectedIndex = 0;
					if (selectedIndex >= scrollOffset + maxVisibleItems) scrollOffset = selectedIndex - maxVisibleItems + 1;
					if (selectedIndex == 0) scrollOffset = 0;
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
						var selectedSector = sectors[selectedIndex];
						ProcessSectorBreach(selectedSector, player);
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

		private static string FormatSectorLine(SectorData sector)
		{
			var log = PlayerQuestLog.Instance;
			string status = "[STABLE   ]";
			if (log.IsQuestCompleted(sector.Id)) status = "[DELETED  ]".Pastel("#555555");
			else if (log.IsQuestAccepted(sector.Id)) status = "[BREACHING]".Pastel("#00FF00");

			// Use the new Title property
			string name = sector.Title.Length > 50 ? sector.Title.Substring(0, 47) + "..." : sector.Title;
			return $"{status} {name} | {sector.Difficulty}".PadRight(90);
		}

		private static void ProcessSectorBreach(SectorData sector, PlayerData.loadPlayer player)
		{
			var log = PlayerQuestLog.Instance;

			if (log.IsQuestCompleted(sector.Id))
			{
				UiFunctions.QuickAlert("This sector has already been purged.");
				return;
			}

			// Accept if not already accepted (Logic remains the same, just simulation themed)
			if (!log.IsQuestAccepted(sector.Id))
			{
				log.AcceptQuest(sector.Id);
				UiFunctions.QuickAlert($"Breach Initialized: {sector.Title}");
			}

			// 1. Run the Sector directly (No AdventureSelector needed!)
			// Note: You will need to update AdventureRunner.Run to accept SectorData instead of AdventureData
			bool isVictory = SimulationExecutor.Run(sector, player);

			// 2. Handle the Outcome
			if (isVictory && player.Health > 0)
			{
				log.CompleteQuest(sector.Id);
				// Rewarding credits based on the new JSON property
				player.Coins += sector.CompletionCredits;
				UiFunctions.QuickAlert($"BREACH SUCCESSFUL: {sector.Title}".Pastel("#00FF00"));
				WriteLine($"System rewards distributed: {sector.CompletionCredits} Credits.".Pastel("#FFD700"));
			}
			else
			{
				UiFunctions.QuickAlert($"BREACH FAILED: Connection Terminated!".Pastel("#FF0000"));
			}

			SaveGame.Save();

			WriteLine("\nPress any key to return to Terminal...");
			ReadKey(true);
			Clear();
		}
	}
}