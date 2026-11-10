using Maries_DataModels;
using static Maries_DataModels.PlayerData;

namespace DungeonAdventures.Src.GameEngine.Systems
{
	/// <summary>Puts items in the hero's bag the same way everywhere (loot, quests, examining).</summary>
	public static class PlayerInventory
	{
		/// <summary>
		/// Adds an item, stacking non-weapons onto an existing stack with the same name, and
		/// counts it for "collect_item" quest objectives.
		/// </summary>
		public static void Give(loadPlayer owner, ItemData item)
		{
			QuestSystem.Notify(owner, "collect_item", item.Name, Math.Max(1, item.Amount));

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
	}
}
