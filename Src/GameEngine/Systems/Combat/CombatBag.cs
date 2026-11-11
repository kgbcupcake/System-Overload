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
	/// <summary>Using a consumable from the bag mid-fight.</summary>
	public static class CombatBag
	{
		/// <summary>Opens the bag over the fight. Returns true when something was used (that takes the turn).</summary>
		public static bool Use(BattleScene scene, Combatant player, Combatant enemy, List<ItemData> items)
		{
			int itemIndex = 0;
			var lines = items.Select(i => $"{i.Name} x{i.Amount} - {DescribeEffect(i)}").ToList();
			int pick = scene.Pick(HitChance(player, enemy), "BAG", lines, ref itemIndex);
			if (pick < 0)
				return false;

			var selectedItem = items[pick];
			string feedback = ApplyItem(player, enemy, selectedItem);

			// Consume one from the stack and remove it when empty.
			selectedItem.Amount--;
			if (selectedItem.Amount <= 0)
				player.Inventory.Remove(selectedItem);

			scene.Say(HitChance(player, enemy), feedback);
			return true;
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
