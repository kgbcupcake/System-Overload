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
	public static class CombatEncounter
	{
		internal static void DrawTitleBar()
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
			string[] menuOptions = { "ATTACK", "BAG", "SPECIAL", "RUN" };
			var scene = new BattleScene(player, enemy);
			int Hit() => HitChance(player, enemy);

			if (enemy.IsBoss)
				BossIntro.Show(enemy, Story.Line("combat.boss_banner"), enemy.Boss!.OnEncounter);
			scene.Say(Hit(), enemy.IsBoss
				? $"{enemy.Name.Pastel(enemy.Boss!.HexColor)} bars the way!"
				: $"A wild {enemy.Name.Pastel("#FF4500")} appears!");

			while (player.Health > 0 && enemy.Health > 0)
			{
				int choice = scene.Command(Hit(), menuOptions, ref selectedIndex);
				bool turnTaken = false;

				if (choice == 0) // ATTACK
				{
					PlayerAttack(scene, player, enemy);
					turnTaken = true;
				}
				else if (choice == 1) // BAG
				{
					var usableItems = player.Inventory
						.Where(i => i.Type.Equals("Consumable", StringComparison.OrdinalIgnoreCase) && i.Amount > 0)
						.ToList();
					if (usableItems.Count == 0)
						scene.Say(Hit(), "You have nothing to use.");
					else
						turnTaken = CombatBag.Use(scene, player, enemy, usableItems);
				}
				else if (choice == 2) // SPECIAL
				{
					scene.Say(Hit(), "Special abilities are currently locked!");
				}
				else if (choice == 3) // RUN
				{
					if (enemy.IsBoss)
					{
						scene.Say(Hit(), Story.Pick("combat.no_retreat", ("enemy", enemy.Name)).Pastel("#FF0000"));
						continue;
					}
					scene.Say(Hit(), Story.Pick("combat.fled", ("enemy", enemy.Name)));
					if (owner != null)
						owner.Health = player.Health;
					return false;
				}

				// The enemy answers every action that used up the player's turn.
				if (turnTaken && enemy.Health > 0 && player.Health > 0)
					EnemyTurn(scene, player, enemy);
			}

			CombatOutcome.Show(player, enemy, owner);
			return player.Health > 0;
		}

		// ---------------------------------------------------------------- turns

		private static void PlayerAttack(BattleScene scene, Combatant player, Combatant enemy)
		{
			var result = Roll(player, enemy);
			int hit = HitChance(player, enemy);
			if (!result.Hit)
			{
				scene.Say(hit, $"Your attack misses {enemy.Name}.".Pastel("#888888"));
				return;
			}

			enemy.Health = Math.Max(0, enemy.Health - result.Damage);
			scene.Draw(hit);
			scene.Flash(foe: true, result.Critical);

			var lines = new List<string>
			{
				result.Critical
					? $"CRITICAL! You {player.AttackVerb} {result.Damage} damage to {enemy.Name}!".Pastel("#FFD700")
					: $"You {player.AttackVerb} {result.Damage} damage to {enemy.Name}."
			};
			if (enemy.Health <= 0)
			{
				scene.Say(hit, lines.ToArray());
				scene.Faint();
				scene.Say(hit, Story.Pick("combat.enemy_falls", ("enemy", enemy.Name)).Pastel("#00FF00"));
				return;
			}
			lines.AddRange(CheckEnrage(enemy));
			scene.Say(hit, lines.ToArray());
		}

		/// <summary>A boss below half health enrages once: it speaks and hits harder. Returns what to show.</summary>
		private static IEnumerable<string> CheckEnrage(Combatant enemy)
		{
			var boss = enemy.Boss;
			if (boss == null || boss.Enraged || enemy.Health > enemy.MaxHealth / 2)
				yield break;

			boss.Enraged = true;
			if (!string.IsNullOrWhiteSpace(boss.OnDamage))
				yield return $"{enemy.Name}: \"{boss.OnDamage}\"".Pastel(boss.HexColor);
			if (boss.EnrageMultiplier > 1.0)
				yield return $"[!] {enemy.Name.ToUpper()} IS ENRAGED. Damage x{boss.EnrageMultiplier:0.##}".Pastel("#FF0000");
		}

		private static void EnemyTurn(BattleScene scene, Combatant player, Combatant enemy)
		{
			int hit = HitChance(player, enemy);
			if (enemy.SkipTurns > 0)
			{
				enemy.SkipTurns--;
				scene.Say(hit, $"{enemy.Name} is too slow and loses its turn.".Pastel("#00FFFF"));
				return;
			}

			string message;
			bool landed, critical = false;
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
					landed = critical = true;
					message = $"{enemy.Name} unleashes {boss.SpecialAbility.ToUpper()} for {result.Damage} damage!".Pastel("#FF0000");
				}
				else
				{
					string attack = boss.AttackNames.Count > 0 ? boss.AttackNames[_rng.Next(boss.AttackNames.Count)] : "an attack";
					var result = Roll(enemy, player, multiplier);
					landed = result.Hit;
					critical = result.Critical;
					if (result.Hit)
						player.Health = Math.Max(0, player.Health - result.Damage);
					message = !result.Hit
						? $"{enemy.Name} uses {attack}... and misses."
						: $"{enemy.Name} uses {attack} for {result.Damage} damage{(result.Critical ? " (CRITICAL)" : "")}!".Pastel(boss.HexColor);
				}
			}
			else
			{
				var result = Roll(enemy, player);
				landed = result.Hit;
				critical = result.Critical;
				if (result.Hit)
					player.Health = Math.Max(0, player.Health - result.Damage);
				message = !result.Hit
					? $"{enemy.Name} {enemy.AttackVerb} and misses."
					: result.Critical
						? $"CRITICAL! {enemy.Name} {enemy.AttackVerb} for {result.Damage} damage!".Pastel("#FF4500")
						: $"{enemy.Name} {enemy.AttackVerb} for {result.Damage} damage!";
			}

			if (landed)
			{
				scene.Draw(hit);
				scene.Flash(foe: false, critical);
			}
			scene.Say(hit, message);
		}
	}
}
