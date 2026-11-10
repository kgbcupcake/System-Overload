using Maries_DataModels;
using System_Overload.Src.GameData.Components;
using System_Overload.Src.GameEngine.Combat;
using Maries_Ui;
using Maries_Loaders; // Make sure this namespace is included
using static System.Console;
using Pastel;

namespace System_Overload.Src.GameEngine.Flow
{
	public static class SimulationExecutor
	{
		private static void DrawTitleBar()
		{
			HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "", GameState.CurrentPlayer?.Health ?? 0, GameState.CurrentPlayer?.HitPoints ?? 0, GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);
		}

		// Changed parameter from AdventureData to SectorData
		public static bool Run(SectorData sector, PlayerData.loadPlayer player)
		{
			Clear();
			DrawTitleBar();

			WriteLine(Story.Line("dungeon.enter", ("title", sector.Title)).Pastel("#FFAB00"));
			WriteLine(sector.Description);
			WriteLine($"DANGER: {sector.Difficulty}".Pastel("#FFD700"));
			ReadKey(true);

			// 1. Walk through each stage of the floor
			foreach (var stage in sector.Stages ?? Enumerable.Empty<SectorStage>())
			{
				Clear();
				DrawTitleBar();

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
					bool playerWon = CombatEncounter.Start(playerCombatant, enemyCombatant, player);

					// Sync health back to the player object immediately
					player.Health = playerCombatant.Health;

					if (!playerWon)
					{
						// The hero fell or fled
						return false;
					}
				}
			}

			// 2. SUCCESS REWARDS (Using the new SectorData property names)
			player.AddXP(sector.CompletionExp);
			player.AddGold(sector.CompletionCredits); // Map credits to your gold function for now

			Clear();
			DrawTitleBar();
			WriteLine("=====================================".Pastel("#FFD700"));
			string done = string.IsNullOrWhiteSpace(sector.CompletionText)
				? Story.Line("dungeon.complete", ("title", sector.Title))
				: sector.CompletionText;
			WriteLine($"  {done}");
			WriteLine("=====================================".Pastel("#FFD700"));
			WriteLine($"REWARD: {sector.CompletionCredits} gold and {sector.CompletionExp} XP");
			ReadKey(true);

			return true;
		}
	}
}
