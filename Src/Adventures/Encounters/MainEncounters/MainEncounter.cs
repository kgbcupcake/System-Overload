using System_Overload.Src.GameData.Components;
using System_Overload.Src.GameData.Entities;
using System_Overload.Src.GameData.Loaders;
using System_Overload.Src.GameEngine.Combat;
using System_Overload.Src.Utilities.UI;
using Src.GameData.Components;
using static System_Overload.Src.GameData.Entities.PlayerData;
using static System.Console;

namespace Src.Adventures.Encounters.MainEncounters
{
	public static class MainEncounter
	{
		static readonly Random rand = new Random();

		public static void FirstEncounter()
		{
			// Removed "Goblin" placeholder - now calling a Rogue Security Program
			var enemy = DronesLoader.GetById("security_daemon");
			Combat(enemy);
		}

		public static void Combat(NewEnemyData enemyData)
		{
			Combatant enemy = new Combatant(
				enemyData.Name,
				enemyData.MaxHealth,
				enemyData.AttackPower,
				enemyData.Defense
			);

			var player = GameState.CurrentPlayer;

			while (enemy.Health > 0 && player.Health > 0)
			{
				// Logic: Count Neural Stim-Packs in the actual inventory list
				int stimCount = player.Inventory.Count(i => i.Name == "Neural Stim-Pack");

				var lines = new List<string>
				{
					$"Target: {enemy.Name}",
					$"Integrity: {enemy.Health}/{enemy.MaxHealth}",
					$"Threat Level: {enemy.Attack}",
					"--------------------",
					$"Operator: {player.PlayerName}",
					$"Sync Stability: {player.Health}",
					$"Stim-Packs: {stimCount} | Damage Rating: {player.GetTotalDamage()}"
				};

				UiEngine.DrawDynamicFrame("NEURAL COMBAT", lines, "[A]ttack    [H]eal");

				var key = ReadKey(true).Key;

				if (key == ConsoleKey.A)
				{
					// Logic: Use the new Socketed Damage system
					float baseDmg = player.GetTotalDamage();
					int playerDamage = rand.Next((int)baseDmg / 2, (int)baseDmg + 1);

					int enemyDamage = Math.Max(1, enemy.Attack);

					enemy.Health -= playerDamage;
					player.Health -= enemyDamage;

					UiEngine.DrawDynamicFrame(
						"COMBAT LOG",
						new List<string>
						{
							$"Executing strike... {enemy.Name} integrity reduced by {playerDamage}!",
							$"Counter-measure detected! Sync dropped by {enemyDamage}.",
							"",
							"Press any key to continue cycle..."
						}
					);

					ReadKey(true);
				}
				else if (key == ConsoleKey.H)
				{
					UsePotion(player);
				}
			}

			HandlePostCombat(player, enemy, enemyData);
		}

		private static void UsePotion(loadPlayer player)
		{
			// Logic: Find the first Stim-Pack in the inventory
			var stim = player.Inventory.FirstOrDefault(i => i.Name == "Neural Stim-Pack");

			if (stim == null)
			{
				UiEngine.DrawDynamicFrame("SYSTEM ERROR", new List<string> { "No Neural Stim-Packs found in buffer!", "", "Press any key..." });
				ReadKey(true);
				return;
			}

			int healAmount = 30;
			player.Inventory.Remove(stim); // Consume the item
			player.Health = Math.Min(player.Health + healAmount, (int)player.GetTotalMaxHealth());

			UiEngine.DrawDynamicFrame("RECOVERY", new List<string> { "Neural Stim-Pack injected.", "Sync stability re-established.", $"+{healAmount} Integrity", "", "Press any key..." });
			ReadKey(true);
		}

		private static void HandlePostCombat(loadPlayer player, Combatant enemy, NewEnemyData newEnemyData)
		{
			if (player.Health <= 0)
			{
				UiEngine.DrawDynamicFrame(
					"CONNECTION LOST",
					new List<string> { "Neural Link Severed.", "Simulation Terminated." },
					"Press any key to restart..."
				);
				ReadKey(true);
				return;
			}

			// Logic: Use your new currency/xp methods
			player.AddGold(newEnemyData.Rewards.Coins);
			player.AddXP(newEnemyData.Rewards.Xp);

			UiEngine.DrawDynamicFrame(
				"DATA ACQUIRED",
				new List<string>
				{
					$"Subject {newEnemyData.Name} de-rezzed.",
					$"Credits Harvested: +{newEnemyData.Rewards.Coins}",
					$"Memory Fragments: +{newEnemyData.Rewards.Xp}"
				},
				"Press any key to continue..."
			);

			ReadKey(true);

			// Leveling check now happens inside AddXP, but we keep the UI here
			if (player.Experience >= player.ExperienceToLevel)
				LevelUp(player);
		}

		public static void LevelUp(loadPlayer player)
		{
			// Simplified: No more Strength/Dexterity point spending
			player.Level++;
			player.HitPoints += 25; // Permanent integrity boost
			player.Health = (int)player.GetTotalMaxHealth();

			UiEngine.DrawDynamicFrame(
				"HARDWARE UPGRADE",
				new List<string>
				{
					"Neural capacity expanded!",
					$"New Level: {player.Level}",
					"Base Integrity increased by 25 units."
				},
				"Press any key to reboot..."
			);
			ReadKey(true);
		}
	}
}