using System_Overload.Src.GameEngine; // Added for SaveGame
using System_Overload.Src.Utilities.UI;
using Pastel;
using Src.GameData.Components;
using Src.TownSquare;
using static System_Overload.Src.GameEngine.Combat.DamageCalculator;

namespace System_Overload.Src.GameEngine.Combat
{
	public static class CombatEncounter
	{
		public static bool Start(Combatant player, Combatant enemy)
		{
			int selectedIndex = 0;
			string[] menuOptions = { "ATTACK", "INVENTORY", "SPECIAL", "RETREAT" };

			while (player.Health > 0 && enemy.Health > 0)
			{
				bool turnActionTaken = false;

				// INNER LOOP: Handles the arrow key movement and menu drawing
				while (!turnActionTaken)
				{
					// 1. Draw the Header and Health Bars (This makes HP updates live)
					RefreshCombatUI(player, enemy);

					// 2. Draw the Command Menu Box
					Console.WriteLine("\n  ┌── INPUT COMMAND ────────────────────┐".Pastel("#FFD700"));
					for (int i = 0; i < menuOptions.Length; i++)
					{
						if (i == selectedIndex)
						{
							// Selection styling
							string selection = $">> {menuOptions[i]} <<";
							Console.WriteLine($"  │  {selection.PadRight(34).Pastel("#000000").PastelBg("#FFD700")} │");
						}
						else
						{
							// Unselected styling
							Console.WriteLine($"  │     {menuOptions[i].PadRight(31).Pastel("#888888")} │");
						}
					}
					Console.WriteLine("  └─────────────────────────────────────┘".Pastel("#FFD700"));

					// 3. Handle Input
					var key = Console.ReadKey(true).Key;

					if (key == ConsoleKey.UpArrow)
					{
						selectedIndex = (selectedIndex == 0) ? menuOptions.Length - 1 : selectedIndex - 1;
						// No 'turnActionTaken = true' here, so it just loops and redraws the menu
					}
					else if (key == ConsoleKey.DownArrow)
					{
						selectedIndex = (selectedIndex == menuOptions.Length - 1) ? 0 : selectedIndex + 1;
					}
					else if (key == ConsoleKey.Enter)
					{
						if (selectedIndex == 0) // ATTACK
						{
							int pDmg = CalculateDamage(player, enemy);
							enemy.Health -= pDmg;

							RefreshCombatUI(player, enemy);
							Console.WriteLine($"\n  > You hit {enemy.Name} for {pDmg} damage!");

							if (enemy.Health > 0)
							{
								EnemyTurn(player, enemy);
							}
							else
							{
								Console.WriteLine($"\n  > {enemy.Name} has been slain!");
								Console.ReadKey(true);
							}

							turnActionTaken = true; // Ends the player's turn
						}
						else if (selectedIndex == 1) // INVENTORY
						{
							// FIX: Grab the whole list instead of filtering by EffectType
							var usableItems = player.Inventory.ToList();

							if (usableItems.Count == 0)
							{
								Console.WriteLine("\n  [!] THE BUFFER IS EMPTY. NO ITEMS DETECTED.");
								Console.ReadKey(true);
							}
							else
							{
								UseItemInCombat(player, enemy, usableItems, ref turnActionTaken);

								if (turnActionTaken && enemy.Health > 0)
								{
									EnemyTurn(player, enemy);
								}
							}
						}

						else if (selectedIndex == 2) // SPECIAL
						{
							Console.WriteLine("\n  Special abilities are currently locked!");
							Console.ReadKey(true);
						}
						else if (selectedIndex == 3) // RETREAT
						{
							Console.Clear();
							UiFunctions.TitleBar();
							Console.WriteLine("\n  You turned tail and fled!");
							Console.ReadKey(true);
							return false;
						}
					}
				}
			}

			EndCombat(player, enemy);
			return player.Health > 0;
		}

		// Combined UI Refresh to prevent "flicker" or "lag"
		private static void RefreshCombatUI(Combatant player, Combatant enemy)
		{
			Console.Clear();
			UiFunctions.TitleBar();

			// 1. ENEMY SECTION
			Console.WriteLine($"\n  [ HOSTILE SOURCE: {enemy.Name.ToUpper().Pastel("#FF4500")} ]");
			DrawHealthBar(enemy.Health, enemy.MaxHealth, true);
			Console.WriteLine(new string('·', 50).Pastel("#333333"));

			// 2. PLAYER SECTION
			Console.WriteLine($"\n  [ USER IDENTITY: {player.Name.ToUpper().Pastel("#00FFFF")} ]");
			DrawHealthBar(player.Health, player.MaxHealth, false);
			Console.WriteLine(new string('═', 50).Pastel("#333333"));
		}

		private static void DrawHealthBar(int current, int max, bool isEnemy)
		{
			int barWidth = 30;
			float percentage = max > 0 ? (float)current / max : 0;
			int filledSegments = Math.Clamp((int)(percentage * barWidth), 0, barWidth);

			// Dynamic Color Logic: Green > Yellow > Red
			string color = "#00FF00"; // Default Green
			if (percentage < 0.25f) color = "#FF0000";      // Red
			else if (percentage < 0.60f) color = "#FFFF00"; // Yellow

			string bar = new string('█', filledSegments).Pastel(color);
			string empty = new string('░', barWidth - filledSegments).Pastel("#222222");

			Console.WriteLine($"  {bar}{empty} {current}/{max} HP");
		}
		private static void EnemyTurn(Combatant player, Combatant enemy)
		{
			int eDmg = CalculateDamage(enemy, player);
			player.Health -= eDmg;

			// Refresh UI immediately after the player takes damage
			RefreshCombatUI(player, enemy);

			// Show the log after the bar has updated
			Console.WriteLine($"\n  > {enemy.Name} strikes back for {eDmg} damage!");
			Console.WriteLine("\n  Press any key for the next turn...");
			Console.ReadKey(true);
		}

		private static void EndCombat(Combatant player, Combatant enemy)
		{
			Console.Clear();
			UiFunctions.TitleBar();

			if (player.Health <= 0)
			{
				Console.WriteLine("\n  " + "--- DEFEAT ---".Pastel("#FF0000"));
				Console.WriteLine($"  {enemy.Name} has bested you.");
			}
			else
			{
				Console.WriteLine("\n  " + "--- VICTORY ---".Pastel("#00FF00"));
				Console.WriteLine($"  The {enemy.Name} has been slain!");
			}

			// Sync to JSON immediately upon combat end
			SaveGame.Save();

			Console.WriteLine("\n  Press any key to continue...");
			Console.ReadKey(true);
		}


		private static void UseItemInCombat(Combatant player, Combatant enemy, List<ItemData> items, ref bool turnActionTaken)
		{
			int itemIndex = 0;
			bool choosing = true;

			while (choosing)
			{
				RefreshCombatUI(player, enemy);
				Console.WriteLine("\n  ┌── SELECT CONSUMABLE ────────────────┐".Pastel("#00FFFF"));

				for (int i = 0; i < items.Count; i++)
				{
					if (i == itemIndex)
						Console.WriteLine($"  │ > {items[i].Name} (+{items[i].Value} HP) ".Pastel("#000000").PastelBg("#00FFFF") + "│");
					else
						Console.WriteLine($"  │   {items[i].Name.PadRight(30)} │");
				}
				Console.WriteLine("  └─────────────────────────────────────┘".Pastel("#00FFFF"));
				Console.WriteLine("  [ESC] Cancel | [ENTER] Use");

				var key = Console.ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow) itemIndex = (itemIndex == 0) ? items.Count - 1 : itemIndex - 1;
				else if (key == ConsoleKey.DownArrow) itemIndex = (itemIndex == items.Count - 1) ? 0 : itemIndex + 1;
				else if (key == ConsoleKey.Escape) choosing = false;
				else if (key == ConsoleKey.Enter)
				{
					// --- FIX: Logic moved inside the Enter block ---
					var selectedItem = items[itemIndex];
					int healAmount = selectedItem.Value;

					// Calculate actual healing vs waste
					int spaceAvailable = player.MaxHealth - player.Health;
					int actualHeal = Math.Min(spaceAvailable, healAmount);

					// 1. APPLY HEAL
					player.Health += actualHeal;

					// 2. CONSUME ITEM (Subtract from amount and remove if empty)
					selectedItem.Amount--;
					if (selectedItem.Amount <= 0)
					{
						player.Inventory.Remove(selectedItem);
					}

					// 3. LOG FEEDBACK
					RefreshCombatUI(player, enemy);
					if (actualHeal < healAmount)
					{
						Console.WriteLine($"\n  > System Restored: +{actualHeal} HP.".Pastel("#00FF00"));
						Console.WriteLine($"  > {healAmount - actualHeal} units of restorative energy wasted (Over-cap).".Pastel("#555555"));
					}
					else
					{
						Console.WriteLine($"\n  > System Restored: +{actualHeal} HP via {selectedItem.Name}.".Pastel("#00FF00"));
					}

					Console.WriteLine("\n  Press any key...");
					Console.ReadKey(true);

					choosing = false;
					turnActionTaken = true;
				}
			}
		}


	}
}