using DungeonAdventures.Src.GameData.Components;
using Maries_Loaders; // FIX: Import the new Registry
using Maries_Ui;
using Pastel;
using Maries_DataModels;
using System.Text.Json;
using static Maries_DataModels.PlayerData;
using static System.Console;

namespace DungeonAdventures.Src.Village.Places
{
	public class Store
	{
		private static string dungeonPurple = "#2E0E4E";
		private static string torchGold = "#FFAB00";

		// 1. The Entry Point
		public static void LoadShop(string shopFile = "GeneralStore.json")
		{
			var p = GameState.CurrentPlayer;

			// This runs UniversalLoader and fills AllItems, AllWeapons, etc.
			// If you already call this in Program.cs, you don't need it here.
			if (ItemRegistry.AllItems.Count == 0)
			{
				ItemRegistry.Initialize();
			}

			// Rest of your restock and shop logic...
			TimeSpan elapsed = DateTime.Now - p.LastRestockTime;
			if (elapsed.TotalMinutes >= 30)
			{
				p.StorePotionStock = 5;
				p.LastRestockTime = DateTime.Now;
			}

			string shopName = Path.GetFileNameWithoutExtension(shopFile);
			var shopData = ShopLoader.GetByName(shopName) ?? new ShopDefinition();

			List<ItemData> storeInventory = new List<ItemData>();
			foreach (var name in shopData.InventoryRefs)
			{
				var template = ItemRegistry.GetItem(name);
				if (template != null) storeInventory.Add(new ItemData(template));
			}

			UiEngine.DrawLoadingScreen(Story.Pick("store.enter"), 1000);
			UpdateAndRun(p, storeInventory);
		}

		// 2. The Main Loop
		// 2. The Main Loop
		private static void UpdateAndRun(loadPlayer p, List<ItemData> storeInventory)
		{
			bool shopping = true;
			bool buyMode = true;
			int sellIndex = 0;
			string statusMessage = Story.Pick("store.greeting").Pastel("#888888");

			while (shopping)
			{
				Clear();
				HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "", GameState.CurrentPlayer?.Health ?? 0, (int)(GameState.CurrentPlayer?.GetTotalMaxHealth() ?? 0), GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);

				int potionPrice = 20 + (10 * p.StorePriceModifier);
				int diffPrice = 300 + (100 * p.DifficultyRating);

				var sellableGroups = p.Inventory
					.GroupBy(i => i.Name)
					.Select(g => new { Name = g.Key, Count = g.Count(), Item = g.First() })
					.ToList();

				string modeHeader = buyMode ? "--- BUYING ---" : "--- SELLING ---";
				string modeToggleHint = buyMode ? "[TAB] Switch to Sell Mode" : "[TAB] Switch to Buy Mode";

				List<string> menuLines = new List<string>();
				// Clickable lines: (line index, key it acts like, sell row it selects)
				var clicks = new List<(int Line, ConsoleKey Key, int? Index)>();
				menuLines.Add("╔════════════ GENERAL STORE ═══════════════╗".Pastel(dungeonPurple));
				menuLines.Add(BuildLine("YOUR GOLD:", p.Coins, "#FFD700", dungeonPurple));
				menuLines.Add("╟──────────────────────────────────────────╢".Pastel(dungeonPurple));

				if (buyMode)
				{
					clicks.Add((menuLines.Count, ConsoleKey.P, null));
					menuLines.Add(BuildLine($"(P)otion [Qty: {p.StorePotionStock}]", p.StorePotionStock > 0 ? potionPrice : -1, torchGold, dungeonPurple));
					clicks.Add((menuLines.Count, ConsoleKey.D, null));
					menuLines.Add(BuildLine("(D)ark Pact (harder foes)", diffPrice, torchGold, dungeonPurple));
				}
				else
				{
					for (int i = 0; i < sellableGroups.Count; i++)
					{
						var group = sellableGroups[i];

						string rarityColor = group.Item.Rarity switch
						{
							ItemRarity.Uncommon => "#1EFF00",
							ItemRarity.Rare => "#0070DD",
							ItemRarity.Epic => "#A335EE",
							ItemRarity.Legendary => "#FF8000",
							_ => "#FFFFFF"
						};

						// 1. Calculate Base Value
						int baseVal = group.Item.Rarity switch
						{
							ItemRarity.Common => 15,
							ItemRarity.Uncommon => 45,
							ItemRarity.Rare => 120,
							ItemRarity.Epic => 300,
							ItemRarity.Legendary => 750,
							_ => 10
						};

						// 2. Apply Difficulty Multiplier to the Display Price
						float multi = 1.0f + (p.DifficultyRating * 0.15f);
						int displayVal = (int)(baseVal * multi);

						string selector = (i == sellIndex) ? ">> " : "   ";
						string lineText = $"{selector}{group.Name.Pastel(rarityColor)} (x{group.Count})";

						clicks.Add((menuLines.Count, ConsoleKey.Enter, i));
						menuLines.Add(BuildLine(lineText, displayVal, "#00FF00", dungeonPurple));
					}
					if (sellableGroups.Count == 0)
						menuLines.Add(BuildLine("NOTHING TO SELL", -1, "#555555", dungeonPurple));
				}

				menuLines.Add("║                                          ║".Pastel(dungeonPurple));
				clicks.Add((menuLines.Count, ConsoleKey.E, null));
				menuLines.Add(BuildLine("(E)xit to Town Square", -1, torchGold, dungeonPurple));
				menuLines.Add("╚══════════════════════════════════════════╝".Pastel(dungeonPurple));
				menuLines.Add("");
				menuLines.Add($" {modeHeader.Pastel("#00FFFF")} ");
				clicks.Add((menuLines.Count, ConsoleKey.Tab, null));
				menuLines.Add($" {modeToggleHint.Pastel("#555555")} ");
				menuLines.Add("");
				menuLines.Add(" STATUS: " + statusMessage);

				int startX = (85 / 2) - (44 / 2);
				int startY = 6;
				MouseInput.ClearZones();
				foreach (var (line, clickKey, index) in clicks)
					MouseInput.AddKeyZone(startY + line, startX, 44, clickKey, index);
				foreach (var line in menuLines) { SetCursorPosition(startX, startY++); WriteLine(line); }

				var key = MouseInput.ReadMenuKey(ref sellIndex, sellableGroups.Count);
				if (key == ConsoleKey.Tab) { buyMode = !buyMode; statusMessage = "MODE TOGGLED".Pastel("#FFD700"); }
				else if (key == ConsoleKey.E)
				{
					// Small exit animation
					statusMessage = Story.Pick("store.farewell").Pastel("#FFAB00");
					SetCursorPosition(startX, startY - 1); // Re-draw the status line
					WriteLine(" STATUS: " + statusMessage);
					Thread.Sleep(500);
					shopping = false;
					Clear();
				}
				else if (buyMode)
				{
					if (key == ConsoleKey.P && p.StorePotionStock > 0)
					{
						// CALLING IT HERE: Pass the EXACT name the Registry/JSON expects
						statusMessage = TryBuyReturnStatus("Healing Potion", potionPrice, p);

						if (statusMessage.Contains("SUCCESS"))
						{
							p.StorePotionStock--;
							p.StorePriceModifier++;
							GameState.Sync();
						}
					}
					else if (key == ConsoleKey.D)
					{
						statusMessage = TryBuyReturnStatus("Difficulty", diffPrice, p);
						if (statusMessage.Contains("SUCCESS")) GameState.Sync();
					}
				}
				else if (!buyMode && sellableGroups.Count > 0)
				{
					if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow) sellIndex = MenuNavigator.HandleVerticalNav(key, sellIndex, sellableGroups.Count);

					if (key == ConsoleKey.Enter)
					{
						var target = sellableGroups[sellIndex];

						int baseProfit = target.Item.Rarity switch
						{
							ItemRarity.Common => 15,
							ItemRarity.Uncommon => 45,
							ItemRarity.Rare => 120,
							ItemRarity.Epic => 300,
							ItemRarity.Legendary => 750,
							_ => 10
						};

						// APPLY MULTIPLIER TO ACTUAL SALE
						float multiplier = 1.0f + (p.DifficultyRating * 0.15f);
						int finalProfit = (int)(baseProfit * multiplier);

						p.Coins += finalProfit;
						p.Inventory.Remove(target.Item);

						statusMessage = $"SOLD: {target.Name} (+{finalProfit} gold)".Pastel("#00FF00");

						GameState.Sync();

						if (sellIndex >= sellableGroups.Count) sellIndex = Math.Max(0, sellableGroups.Count - 1);
					}
				}
			}
		}
		private static string TryBuyReturnStatus(string item, int cost, loadPlayer p)
		{
			if (p.Coins >= cost)
			{
				if (item == "Difficulty")
				{
					p.Coins -= cost;
					p.DifficultyRating++;
					return "SUCCESS: The pact is sealed. Your foes grow stronger.".Pastel("#00FF00");
				}

				// Logic for Healing Potion
				if (item == "Healing Potion")
				{
					var template = ItemRegistry.GetItem("Healing Potion");

					if (template != null)
					{
						p.Coins -= cost;
						p.Inventory.Add(new ItemData(template));
						return "SUCCESS: Healing Potion bought.".Pastel("#00FF00");
					}

					// If it hits here, the NAME is right, but the Registry is empty or the file wasn't loaded
					return $"ERROR: [{item}] is missing from the item data.".Pastel("#FF0000");
				}
			}
			return "DENIED: Not enough gold.".Pastel("#FF0000");
		}
		private static string BuildLine(string label, int val, string valColor, string borderColor)
		{
			string leftWall = "║ ".Pastel(borderColor);
			string rightWall = " ║".Pastel(borderColor);
			int contentWidth = 40;

			// STRIP ANSI so we know exactly how many visible characters we have
			string cleanLabel = UiEngine.StripAnsi(label);
			string result;

			if (val == -1)
			{
				int paddingNeeded = contentWidth - cleanLabel.Length;
				result = leftWall + label + new string(' ', Math.Max(0, paddingNeeded)) + rightWall;
			}
			else
			{
				string valStr = label.Contains("YOUR GOLD") ? val.ToString() : $"Cost: {val}";
				string coloredVal = valStr.Pastel(valColor);

				int spacesNeeded = contentWidth - cleanLabel.Length - valStr.Length;
				string gap = new string(' ', Math.Max(0, spacesNeeded));

				// Combine it all into one solid string before returning
				result = leftWall + label + gap + coloredVal + rightWall;
			}

			return result;
		}
	}
}