using DungeonAdventures.Src.GameEngine; // Added for SaveGame
using DungeonAdventures.Src.GameData.Components;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using Maries_DataModels;
using DungeonAdventures.Src.Village;
using static DungeonAdventures.Src.GameEngine.Combat.DamageCalculator;

namespace DungeonAdventures.Src.GameEngine.Combat
{
	public static class CombatEncounter
	{
		private static void DrawTitleBar()
		{
			HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "", GameState.CurrentPlayer?.Health ?? 0, (int)(GameState.CurrentPlayer?.GetTotalMaxHealth() ?? 0), GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);
		}

		private static readonly Random _rng = new();

		/// <summary>
		/// Runs one fight. Returns true if the player won.
		/// </summary>
		/// <param name="owner">
		/// The player profile behind <paramref name="player"/>. When given, XP, coins and
		/// drops are awarded to it and its health is synced before saving.
		/// </param>
		public static bool Start(Combatant player, Combatant enemy, PlayerData.loadPlayer? owner = null)
		{
			int selectedIndex = 0;
			string[] menuOptions = { "ATTACK", "INVENTORY", "SPECIAL", "RETREAT" };

			if (enemy.IsBoss)
				ShowBossIntro(player, enemy);

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

					if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
					{
						selectedIndex = MenuNavigator.HandleVerticalNav(key, selectedIndex, menuOptions.Length);
					}
					else if (key == ConsoleKey.Enter)
					{
						if (selectedIndex == 0) // ATTACK
						{
							PlayerAttack(player, enemy);
							turnActionTaken = true;
						}
						else if (selectedIndex == 1) // INVENTORY
						{
							var usableItems = player.Inventory
								.Where(i => i.Type.Equals("Consumable", StringComparison.OrdinalIgnoreCase) && i.Amount > 0)
								.ToList();

							if (usableItems.Count == 0)
							{
								Console.WriteLine("\n  [!] You have nothing to use.");
								Console.ReadKey(true);
							}
							else
							{
								UseItemInCombat(player, enemy, usableItems, ref turnActionTaken);
							}
						}
						else if (selectedIndex == 2) // SPECIAL
						{
							Console.WriteLine("\n  Special abilities are currently locked!");
							Console.ReadKey(true);
						}
						else if (selectedIndex == 3) // RETREAT
						{
							if (enemy.IsBoss)
							{
								Console.WriteLine("\n  " + Story.Pick("combat.no_retreat", ("enemy", enemy.Name)).Pastel("#FF0000"));
								Console.ReadKey(true);
								continue;
							}

							Console.Clear();
							DrawTitleBar();
							Console.WriteLine("\n  " + Story.Pick("combat.fled", ("enemy", enemy.Name)));
							Console.ReadKey(true);
							if (owner != null)
								owner.Health = player.Health;
							return false;
						}
					}
				}

				// The enemy answers every action that used up the player's turn.
				if (enemy.Health > 0 && player.Health > 0)
					EnemyTurn(player, enemy);
			}

			EndCombat(player, enemy, owner);
			return player.Health > 0;
		}

		// ---------------------------------------------------------------- turns

		private static void PlayerAttack(Combatant player, Combatant enemy)
		{
			var result = Roll(player, enemy);
			if (result.Hit)
				enemy.Health = Math.Max(0, enemy.Health - result.Damage);

			RefreshCombatUI(player, enemy);
			if (!result.Hit)
				Console.WriteLine($"\n  > Your attack misses {enemy.Name}.".Pastel("#888888"));
			else if (result.Critical)
				Console.WriteLine($"\n  > CRITICAL! You {player.AttackVerb} {result.Damage} damage to {enemy.Name}!".Pastel("#FFD700"));
			else
				Console.WriteLine($"\n  > You {player.AttackVerb} {result.Damage} damage to {enemy.Name}.");

			if (enemy.Health <= 0)
				Console.WriteLine(("\n  > " + Story.Pick("combat.enemy_falls", ("enemy", enemy.Name))).Pastel("#00FF00"));
			else
				CheckEnrage(enemy);

			Console.WriteLine("\n  Press any key...");
			Console.ReadKey(true);
		}

		/// <summary>A boss below half health enrages once: it speaks and hits harder.</summary>
		private static void CheckEnrage(Combatant enemy)
		{
			var boss = enemy.Boss;
			if (boss == null || boss.Enraged || enemy.Health > enemy.MaxHealth / 2)
				return;

			boss.Enraged = true;
			if (!string.IsNullOrWhiteSpace(boss.OnDamage))
				Console.WriteLine($"\n  {enemy.Name}: \"{boss.OnDamage}\"".Pastel(boss.HexColor));
			if (boss.EnrageMultiplier > 1.0)
				Console.WriteLine($"  [!] {enemy.Name.ToUpper()} IS ENRAGED. Damage x{boss.EnrageMultiplier:0.##}".Pastel("#FF0000"));
		}

		private static void EnemyTurn(Combatant player, Combatant enemy)
		{
			if (enemy.SkipTurns > 0)
			{
				enemy.SkipTurns--;
				RefreshCombatUI(player, enemy);
				Console.WriteLine($"\n  > {enemy.Name} is too slow and loses its turn.".Pastel("#00FFFF"));
				Console.WriteLine("\n  Press any key for the next turn...");
				Console.ReadKey(true);
				return;
			}

			string message;
			var boss = enemy.Boss;
			if (boss != null)
			{
				boss.TurnCount++;
				double multiplier = boss.Enraged ? boss.EnrageMultiplier : 1.0;
				bool special = boss.SpecialEvery > 0 && boss.TurnCount % boss.SpecialEvery == 0
					&& !string.IsNullOrWhiteSpace(boss.SpecialAbility);

				if (special)
				{
					var result = Roll(enemy, player, multiplier * 1.5, alwaysHit: true);
					player.Health = Math.Max(0, player.Health - result.Damage);
					message = $"\n  >> {enemy.Name} unleashes {boss.SpecialAbility.ToUpper()} for {result.Damage} damage!".Pastel("#FF0000");
				}
				else
				{
					string attack = boss.AttackNames.Count > 0 ? boss.AttackNames[_rng.Next(boss.AttackNames.Count)] : "an attack";
					var result = Roll(enemy, player, multiplier);
					if (result.Hit)
						player.Health = Math.Max(0, player.Health - result.Damage);
					message = !result.Hit
						? $"\n  > {enemy.Name} uses {attack}... and misses."
						: $"\n  > {enemy.Name} uses {attack} for {result.Damage} damage{(result.Critical ? " (CRITICAL)" : "")}!".Pastel(boss.HexColor);
				}
			}
			else
			{
				var result = Roll(enemy, player);
				if (result.Hit)
					player.Health = Math.Max(0, player.Health - result.Damage);
				message = !result.Hit
					? $"\n  > {enemy.Name} {enemy.AttackVerb} and misses."
					: result.Critical
						? $"\n  > CRITICAL! {enemy.Name} {enemy.AttackVerb} for {result.Damage} damage!".Pastel("#FF4500")
						: $"\n  > {enemy.Name} {enemy.AttackVerb} for {result.Damage} damage!";
			}

			RefreshCombatUI(player, enemy);
			Console.WriteLine(message);
			Console.WriteLine("\n  Press any key for the next turn...");
			Console.ReadKey(true);
		}

		// ---------------------------------------------------------------- screens

		private static void ShowBossIntro(Combatant player, Combatant enemy)
		{
			var boss = enemy.Boss!;
			Console.Clear();
			DrawTitleBar();
			Console.WriteLine();
			Console.WriteLine("  ╔══════════════════════════════════════════════╗".Pastel(boss.HexColor));
			Console.WriteLine($"  ║  {Story.Line("combat.boss_banner").PadRight(44)}║".Pastel(boss.HexColor));
			Console.WriteLine("  ╚══════════════════════════════════════════════╝".Pastel(boss.HexColor));
			Console.WriteLine($"\n  {enemy.Name.ToUpper()}  //  LVL {enemy.Level}  //  HP {enemy.MaxHealth}".Pastel(boss.HexColor));
			if (!string.IsNullOrWhiteSpace(boss.OnEncounter))
				Console.WriteLine($"\n  {enemy.Name}: \"{boss.OnEncounter}\"");
			Console.WriteLine("\n  Press any key to engage...");
			Console.ReadKey(true);
		}

		// Combined UI Refresh to prevent "flicker" or "lag"
		private static void RefreshCombatUI(Combatant player, Combatant enemy)
		{
			Console.Clear();
			DrawTitleBar();

			// 1. ENEMY SECTION
			string enemyColor = enemy.Boss?.HexColor ?? "#FF4500";
			string tag = enemy.IsBoss ? (enemy.Boss!.Enraged ? "DUNGEON LORD // ENRAGED" : "DUNGEON LORD") : "FOE";
			Console.WriteLine($"\n  [ {tag}: {enemy.Name.ToUpper().Pastel(enemyColor)} ]  LVL {enemy.Level}");
			DrawHealthBar(enemy.Health, enemy.MaxHealth, true);
			Console.WriteLine(new string('·', 50).Pastel("#333333"));

			// 2. PLAYER SECTION
			Console.WriteLine($"\n  [ HERO: {player.Name.ToUpper().Pastel("#FFD700")} ]  LVL {player.Level}");
			DrawHealthBar(player.Health, player.MaxHealth, false);
			string defense = player.BonusDefense > 0 ? $"{player.Defense}+{player.BonusDefense}" : $"{player.Defense}";
			Console.WriteLine($"  DMG {player.MinDamage}-{player.MaxDamage}  DEF {defense}  CRIT {player.CritChance}%  HIT {HitChance(player, enemy)}%".Pastel("#888888"));
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

		private static void EndCombat(Combatant player, Combatant enemy, PlayerData.loadPlayer? owner)
		{
			Console.Clear();
			DrawTitleBar();

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
					GrantRewards(owner, enemy);
			}

			if (owner != null)
				owner.Health = player.Health;

			// Sync to JSON immediately upon combat end
			SaveGame.Save();

			Console.WriteLine("\n  Press any key to continue...");
			Console.ReadKey(true);
		}

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

		/// <summary>Stacks non-weapon items with an existing stack of the same name.</summary>
		private static void AddToInventory(PlayerData.loadPlayer owner, ItemData item)
		{
			if (item is not WeaponData)
			{
				var stack = owner.Inventory.FirstOrDefault(i => i is not WeaponData && i.Name == item.Name);
				if (stack != null)
				{
					stack.Amount += item.Amount;
					return;
				}
			}
			owner.Inventory.Add(item);
		}

		// ---------------------------------------------------------------- items

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
					string line = $"{items[i].Name} x{items[i].Amount} - {DescribeEffect(items[i])}";
					if (i == itemIndex)
						Console.WriteLine($"  │ > {line}".Pastel("#000000").PastelBg("#00FFFF"));
					else
						Console.WriteLine($"  │   {line}");
				}
				Console.WriteLine("  └─────────────────────────────────────┘".Pastel("#00FFFF"));
				Console.WriteLine("  [ESC] Cancel | [ENTER] Use");

				var key = Console.ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow) itemIndex = MenuNavigator.HandleVerticalNav(key, itemIndex, items.Count);
				else if (key == ConsoleKey.Escape) choosing = false;
				else if (key == ConsoleKey.Enter)
				{
					var selectedItem = items[itemIndex];
					string feedback = ApplyItem(player, enemy, selectedItem);

					// Consume one from the stack and remove it when empty.
					selectedItem.Amount--;
					if (selectedItem.Amount <= 0)
						player.Inventory.Remove(selectedItem);

					RefreshCombatUI(player, enemy);
					Console.WriteLine(feedback);
					Console.WriteLine("\n  Press any key...");
					Console.ReadKey(true);

					choosing = false;
					turnActionTaken = true;
				}
			}
		}

		private static string DescribeEffect(ItemData item) => item.Effect switch
		{
			EffectType.Restorative => $"restore {item.Value} HP",
			EffectType.Defense => "+5 DEF this fight",
			EffectType.Speed => "enemy loses its next turn",
			_ => "no combat effect",
		};

		/// <summary>Applies a consumable and returns the log line to show.</summary>
		private static string ApplyItem(Combatant player, Combatant enemy, ItemData item)
		{
			switch (item.Effect)
			{
				case EffectType.Restorative:
				{
					int actualHeal = Math.Min(player.MaxHealth - player.Health, item.Value);
					player.Health += actualHeal;
					string line = $"\n  > You use the {item.Name} and recover {actualHeal} HP.".Pastel("#00FF00");
					if (actualHeal < item.Value)
						line += $"\n  > {item.Value - actualHeal} HP of healing wasted (you were nearly full).".Pastel("#555555");
					return line;
				}
				case EffectType.Defense:
					player.BonusDefense += 5;
					return $"\n  > {item.Name} applied: +5 DEF until the fight ends.".Pastel("#00FFFF");
				case EffectType.Speed:
					enemy.SkipTurns++;
					return $"\n  > {item.Name} quickens you: {enemy.Name} loses its next turn.".Pastel("#00FFFF");
				default:
					return $"\n  > {item.Name} does nothing in combat.".Pastel("#555555");
			}
		}
	}
}
