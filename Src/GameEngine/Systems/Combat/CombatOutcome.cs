using DungeonAdventures.Src.GameEngine; // Added for SaveGame
using DungeonAdventures.Src.GameData.Components;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using Maries_DataModels;
using DungeonAdventures.Src.Village;
using DungeonAdventures.Src.GameEngine.Systems;
using static DungeonAdventures.Src.GameEngine.Combat.DamageCalculator;

namespace DungeonAdventures.Src.GameEngine.Combat
{
	/// <summary>The end of a fight: the victory or defeat screen, rewards and drops, then a save.</summary>
	public static class CombatOutcome
	{
		public static void Show(Combatant player, Combatant enemy, PlayerData.loadPlayer? owner)
		{
			Console.Clear();
			CombatEncounter.DrawTitleBar();

			if (player.Health <= 0)
			{
				Console.WriteLine("\n  " + "--- DEFEAT ---".Pastel("#FF0000"));
				Console.WriteLine("  " + Story.Pick("combat.defeat", ("enemy", enemy.Name)));
			}
			else
			{
				Console.WriteLine("\n  " + "--- VICTORY ---".Pastel("#00FF00"));
				if (enemy.Boss is { } boss && !string.IsNullOrWhiteSpace(boss.OnDeath))
					Console.WriteLine($"\n  {enemy.Name}: \"{boss.OnDeath}\"".Pastel(boss.HexColor));
				Console.WriteLine("\n  " + Story.Pick("combat.victory", ("enemy", enemy.Name)));

				if (owner != null)
				{
					GrantRewards(owner, enemy);
					QuestSystem.Notify(owner, "defeat_enemy", enemy.Name);
				}
			}

			if (owner != null)
				owner.Health = player.Health;

			// Sync to JSON immediately upon combat end
			SaveGame.Save();

			Console.WriteLine("\n  Press any key to continue...");
			MouseInput.WaitForAnyInput();
		}

		private static readonly Random _rng = new();

		private static void GrantRewards(PlayerData.loadPlayer owner, Combatant enemy)
		{
			int levelBefore = owner.Level;
			if (enemy.XpReward > 0)
				owner.AddXP(enemy.XpReward);
			if (enemy.CoinReward > 0)
				owner.AddGold(enemy.CoinReward);

			Console.WriteLine($"  Gold: +{enemy.CoinReward}   XP: +{enemy.XpReward}".Pastel("#FFD700"));
			if (owner.Level > levelBefore)
				Console.WriteLine(("  >> " + Story.Line("combat.level_up", ("level", owner.Level))).Pastel("#00FF00"));

			foreach (var (itemName, chance) in enemy.Drops)
			{
				if (_rng.NextDouble() >= chance)
					continue;

				var item = ItemRegistry.CreateCopy(itemName);
				if (item == null)
				{
					Console.WriteLine($"  [?] Unknown drop: '{itemName}' isn't in the item data.".Pastel("#555555"));
					continue;
				}

				AddToInventory(owner, item);
				Console.WriteLine($"  [+] Looted: {item.Name} [{item.Rarity}]".Pastel("#00FFFF"));
			}
		}

		private static void AddToInventory(PlayerData.loadPlayer owner, ItemData item) =>
			PlayerInventory.Give(owner, item);
	}
}
