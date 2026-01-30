using System_Overload.Src.GameData;
using System_Overload.Src.GameData.Entities;
using System_Overload.Src.GameEngine.Combat;
using System_Overload.Src.Utilities.UI;
using System_Overload.GameEngine.Systems.Loaders; // Make sure this namespace is included
using static System.Console;
using Pastel;

namespace System_Overload.Src.GameEngine.Flow
{
	public static class SimulationExecutor
	{
		// Changed parameter from AdventureData to SectorData
		public static bool Run(SectorData sector, PlayerData.loadPlayer player)
		{
			Clear();
			UiFunctions.TitleBar();

			// Using simulation terminology
			WriteLine($"INITIALIZING BREACH: {sector.Title}".Pastel("#00FFFF"));
			WriteLine(sector.Description);
			WriteLine($"SECURITY LEVEL: {sector.Difficulty}".Pastel("#FFD700"));
			ReadKey(true);

			// 1. BREACH LOOP (Iterating through Sector Stages)
			foreach (var stage in sector.Stages ?? Enumerable.Empty<SectorStage>())
			{
				Clear();
				UiFunctions.TitleBar();

				// Apply the stage color if provided in the JSON
				string narrative = string.IsNullOrEmpty(stage.HexColor)
					? stage.StoryText
					: stage.StoryText.Pastel(stage.HexColor);

				WriteLine(narrative);
				ReadKey(true);

				if (!string.IsNullOrWhiteSpace(stage.EnemyId))
				{
					var playerCombatant = EnemyFactory.CreatePlayer(player);
					var enemyCombatant = EnemyFactory.CreateEnemy(stage.EnemyId);

					// CombatEncounter.Start returns false if the player dies OR runs
					bool playerWon = CombatEncounter.Start(playerCombatant, enemyCombatant);

					// Sync health back to the player object immediately
					player.Health = playerCombatant.Health;

					if (!playerWon)
					{
						// Breach failed
						return false;
					}
				}
			}

			// 2. SUCCESS REWARDS (Using the new SectorData property names)
			player.AddXP(sector.CompletionExp);
			player.AddGold(sector.CompletionCredits); // Map credits to your gold function for now

			Clear();
			UiFunctions.TitleBar();
			WriteLine("=====================================".Pastel("#00FF00"));
			WriteLine("         SECTOR PURGE COMPLETE!      ");
			WriteLine("=====================================".Pastel("#00FF00"));
			WriteLine($"DATA RECOVERED: {sector.CompletionCredits} Credits and {sector.CompletionExp} XP");
			ReadKey(true);

			return true;
		}
	}
}