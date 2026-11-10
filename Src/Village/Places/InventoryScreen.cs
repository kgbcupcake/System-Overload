using Maries_DataModels;
using Maries_Ui;
using Pastel;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace DungeonAdventures.Src.Village.Places
{
	internal class InventoryScreen
	{
		public static void Open(PlayerData.loadPlayer player)
		{
			bool inDeck = true;
			int selectedIndex = 0;

			while (inDeck)
			{
				// 1. STACKING & SORTING: Group by name so duplicates don't clutter the screen
				var stackedInventory = player.Inventory
					.GroupBy(i => i.Name)
					.Select(group => new
					{
						Name = group.Key,
						Count = group.Count(),
						Type = group.First().Type,
						Rarity = group.First().Rarity,
						Effect = group.First().Effect,
						ItemReference = group.First() // Reference for the details panel
					})
					.OrderBy(i => i.Type)
					.ThenBy(i => i.Name)
					.ToList();

				// Build options list showing (xCount) for stacks
				List<string> options = stackedInventory
					.Select(i => i.Count > 1
						? $" [{i.Type.ToUpper()}] {i.Name} (x{i.Count}) "
						: $" [{i.Type.ToUpper()}] {i.Name} ")
					.ToList();

				options.Add(" CLOSE PACK ");

				Clear();
				DrawPrivateHeader(player);

				int boxWidth = 44;
				int screenWidth = 85;
				int startX = (screenWidth / 2) - (boxWidth / 2);
				string bColor = "#8B5A2B";

				List<string> menuLines = new List<string>();
				menuLines.Add("╔══════════ YOUR PACK ═════════════════════╗".Pastel(bColor));

				for (int i = 0; i < options.Count; i++)
				{
					bool isSelected = (i == selectedIndex);
					string color = isSelected ? "#FFD700" : "#8B6B4A";

					if (i == options.Count - 1 && !isSelected) color = "#888888";

					string text = isSelected ? $">> {options[i]} <<" : options[i];
					menuLines.Add(BuildMenuLine(text, color, bColor));
				}

				menuLines.Add("╚══════════════════════════════════════════╝".Pastel(bColor));

				// 2. The Info Panel & Controls Guide
				if (selectedIndex < stackedInventory.Count)
				{
					var item = stackedInventory[selectedIndex];
					menuLines.Add("");
					menuLines.Add("  DETAILS:".Pastel("#555555"));
					menuLines.Add($"  NAME: {item.Name}".Pastel("#FFD700"));
					menuLines.Add($"  TYPE: {item.Type} | RARITY: {item.Rarity}".Pastel("#FFD700"));
					menuLines.Add($"  EFFECT: {item.Effect}".Pastel("#00FF00"));
					menuLines.Add("");
					menuLines.Add("  [ENTER] Use | [X/DEL] Drop ".Pastel("#444444"));
				}

				int row = 5;
				foreach (string line in menuLines)
				{
					SetCursorPosition(startX, row++);
					WriteLine(line);
				}

				var key = ReadKey(true).Key;
				switch (key)
				{
					case ConsoleKey.UpArrow:
					case ConsoleKey.DownArrow:
						selectedIndex = MenuNavigator.HandleVerticalNav(key, selectedIndex, options.Count);
						break;

					case ConsoleKey.Enter:
						if (selectedIndex == options.Count - 1) inDeck = false;
						else if (stackedInventory.Count > 0)
						{
							// Find first actual instance of this stack to use
							var targetName = stackedInventory[selectedIndex].Name;
							var actualItem = player.Inventory.FirstOrDefault(i => i.Name == targetName);
							if (actualItem != null) UseItem(player, actualItem);
						}
						break;
					case ConsoleKey.X:
					case ConsoleKey.Delete:
						if (selectedIndex < stackedInventory.Count)
						{
							var targetName = stackedInventory[selectedIndex].Name;
							var actualItem = player.Inventory.FirstOrDefault(i => i.Name == targetName);
							if (actualItem != null) DiscardItem(player, actualItem);
						}
						break;
					case ConsoleKey.Escape:
						inDeck = false;
						break;

					

				}
				if (selectedIndex >= options.Count)
				{
					selectedIndex = Math.Max(0, options.Count - 1);
				}
			}
			Clear();
		}

		private static void UseItem(PlayerData.loadPlayer p, ItemData item)
		{
			if (item.Effect == EffectType.Restorative)
			{
				int maxHP = (int)p.GetTotalMaxHealth(); // Includes bonus max HP

				if (p.Health >= maxHP)
				{
					UiEngine.DrawCentered("You are already at full health.".Pastel("#FFD700"), 22);
				}
				else
				{
					p.Health = Math.Min(maxHP, p.Health + 25);
					p.Inventory.Remove(item); // Removes only ONE instance
					UiEngine.DrawCentered($"You use the {item.Name}.".Pastel("#00FF00"), 22);
				}
				Thread.Sleep(800);
			}
		}

		private static void DiscardItem(PlayerData.loadPlayer p, ItemData item)
		{
			UiEngine.DrawCentered($"You drop the {item.Name}.".Pastel("#FF4500"), 22);
			p.Inventory.Remove(item);
			Thread.Sleep(600);
		}

		private static void DrawPrivateHeader(PlayerData.loadPlayer p)
		{
			SetCursorPosition(0, 0);
			string loc = " INVENTORY ".Pastel("#FFFFFF").PastelBg("#8B0000");
			string stats = $" | 👤 {p.PlayerName} | ❤️ HP: {p.Health}/{(int)p.GetTotalMaxHealth()} | 💰 GOLD: {p.Coins} ".Pastel("#FFD700");
			Write(new string(' ', WindowWidth));
			SetCursorPosition(0, 0);
			Write(loc + stats);
		}

		private static string BuildMenuLine(string text, string textColor, string borderColor)
		{
			string leftWall = "║ ".Pastel(borderColor);
			string rightWall = " ║".Pastel(borderColor);
			int totalWidth = 40;

			// Calculate padding safely to prevent negative values
			int paddingSize = Math.Max(0, (totalWidth - text.Length) / 2);
			string leftPad = new string(' ', paddingSize);

			// Calculate right padding based on what's left over
			int rightPadCount = Math.Max(0, totalWidth - text.Length - paddingSize);
			string rightPad = new string(' ', rightPadCount);

			return leftWall + (leftPad + text.Pastel(textColor) + rightPad) + rightWall;
		}
	}
}
