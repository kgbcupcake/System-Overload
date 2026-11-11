using Maries_DataModels;
using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine.Combat;
using Maries_Ui;
using Maries_Loaders; // Make sure this namespace is included
using static System.Console;
using Pastel;

namespace DungeonAdventures.Src.GameEngine.Flow
{
	public static class FloorRunner
	{
		private static void DrawTitleBar()
		{
			HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "", GameState.CurrentPlayer?.Health ?? 0, (int)(GameState.CurrentPlayer?.GetTotalMaxHealth() ?? 0), GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);
		}

		// Runs one dungeon floor stage by stage
		public static bool Run(FloorData floor, PlayerData.loadPlayer player)
		{
			// The hero comes down the stairs into the floor; the plain heading when there's no art to draw.
			var look = player.Look ?? (DungeonAdventures.Src.Lpc.LpcMeta.Available ? DungeonAdventures.Src.Lpc.LpcLook.Default() : null);
			string heading = Story.Line("dungeon.enter", ("title", floor.Title));
			if (!StageScene.Intro(floor, heading, look, DrawTitleBar))
			{
				Clear();
				DrawTitleBar();
				WriteLine(heading.Pastel("#FFAB00"));
				WriteLine(floor.Description);
				WriteLine($"DANGER: {floor.Difficulty}".Pastel("#FFD700"));
				MouseInput.WaitForAnyInput();
			}

			// 1. Walk through each stage of the floor
			foreach (var stage in floor.Stages ?? Enumerable.Empty<FloorStage>())
			{
				Clear();
				DrawTitleBar();

				// Apply the stage color if provided in the JSON
				string narrative = string.IsNullOrEmpty(stage.HexColor)
					? stage.StoryText
					: stage.StoryText.Pastel(stage.HexColor);

				// The room and the hero walking in; the plain line when there's no art to draw.
				if (!StageScene.Play(floor, stage, look, DrawTitleBar))
				{
					Clear();
					DrawTitleBar();
					WriteLine(narrative);
					MouseInput.WaitForAnyInput();
				}

				if (!string.IsNullOrWhiteSpace(stage.EnemyId))
				{
					var playerCombatant = EnemyFactory.CreatePlayer(player);
					var enemyCombatant = EnemyFactory.CreateEnemy(stage.EnemyId, player.DifficultyRating);

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

			// 2. SUCCESS REWARDS (Using the new FloorData property names)
			player.AddXP(floor.CompletionExp);
			player.AddGold(floor.CompletionGold); 

			Clear();
			DrawTitleBar();
			WriteLine("=====================================".Pastel("#FFD700"));
			string done = string.IsNullOrWhiteSpace(floor.CompletionText)
				? Story.Line("dungeon.complete", ("title", floor.Title))
				: floor.CompletionText;
			WriteLine($"  {done}");
			WriteLine("=====================================".Pastel("#FFD700"));
			WriteLine($"REWARD: {floor.CompletionGold} gold and {floor.CompletionExp} XP");
			MouseInput.WaitForAnyInput();

			return true;
		}
	}
}
