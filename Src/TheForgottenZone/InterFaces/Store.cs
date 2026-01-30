using System_Overload.Src.GameData.Components;
using System_Overload.Src.GameEngine.Systems.Loaders; // FIX: Import the new Registry
using System_Overload.Src.Utilities.UI;
using Pastel;
using Src.GameData.Components;
using System.Text.Json;
using static System_Overload.Src.GameData.Entities.PlayerData;
using static System.Console;

namespace Src.TownSquare.InterFaces
{
	public class Store
	{
		private static string dungeonPurple = "#2E0E4E";
		private static string torchGold = "#FFAB00";

		// 1. The Entry Point
		public static void LoadShop(string shopFile = "GeneralStore.json")
		{
			var p = GameState.CurrentPlayer;

			// --- THE FIX: Call your actual method name ---
			// This runs UniversalLoader and fills AllItems, AllWeapons, etc.
			// If you already call this in Program.cs, you don't need it here.
			if (ItemRegistry.AllItems.Count == 0)
			{
				ItemRegistry.Initialize();
			}

			// DEBUG: This will tell you if the loader actually found the file
			if (ItemRegistry.AllItems.Count == 0)
			{
				WriteLine("[CRITICAL ERROR] ItemRegistry is STILL EMPTY after Initialize!");
				WriteLine("Check Path: " + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Items"));
			}


			// ----------------------------------------------

			// Rest of your restock and shop logic...
			TimeSpan elapsed = DateTime.Now - p.LastRestockTime;
			if (elapsed.TotalMinutes >= 30)
			{
				p.StoreStimStock = 5;
				p.LastRestockTime = DateTime.Now;
			}

			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Shops", shopFile);
			List<ItemData> storeInventory = new List<ItemData>();

			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				var shopData = JsonSerializer.Deserialize<ShopDefinition>(json) ?? new ShopDefinition();
				foreach (var name in shopData.InventoryRefs)
				{
					var template = ItemRegistry.GetItem(name);
					if (template != null) storeInventory.Add(new ItemData(template));
				}
			}

			UiEngine.DrawLoadingScreen("UNPACKING CARGO...", 1000);
			UpdateAndRun(p, storeInventory);
		}

		// 2. The Main Loop
		// 2. The Main Loop
		private static void UpdateAndRun(loadPlayer p, List<ItemData> storeInventory)
		{
			bool shopping = true;
			bool buyMode = true;
			int sellIndex = 0;
			string statusMessage = "SYSTEM READY: AWAITING INPUT".Pastel("#555555");

			while (shopping)
			{
				Clear();
				UiFunctions.TitleBar();

				int stimPrice = 20 + (10 * p.StorePriceModifier);
				int diffPrice = 300 + (100 * p.DifficultyRating);

				var sellableGroups = p.Inventory
					.GroupBy(i => i.Name)
					.Select(g => new { Name = g.Key, Count = g.Count(), Item = g.First() })
					.ToList();

				string modeHeader = buyMode ? "--- PURCHASE PROTOCOL ---" : "--- LIQUIDATION PROTOCOL ---";
				string modeToggleHint = buyMode ? "[TAB] Switch to Sell Mode" : "[TAB] Switch to Buy Mode";

				List<string> menuLines = new List<string>();
				menuLines.Add("╔════════════ DATA EXCHANGE ═══════════════╗".Pastel(dungeonPurple));
				menuLines.Add(BuildLine("AVAILABLE CREDITS:", p.Coins, "#FFD700", dungeonPurple));
				menuLines.Add("╟──────────────────────────────────────────╢".Pastel(dungeonPurple));

				if (buyMode)
				{
					menuLines.Add(BuildLine($"(S)tim-Pack [Qty: {p.StoreStimStock}]", p.StoreStimStock > 0 ? stimPrice : -1, torchGold, dungeonPurple));
					menuLines.Add(BuildLine("(D)ifficulty Logic", diffPrice, torchGold, dungeonPurple));
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

						menuLines.Add(BuildLine(lineText, displayVal, "#00FF00", dungeonPurple));
					}
					if (sellableGroups.Count == 0)
						menuLines.Add(BuildLine("NO RECYCLABLE DATA FOUND", -1, "#555555", dungeonPurple));
				}

				menuLines.Add("║                                          ║".Pastel(dungeonPurple));
				menuLines.Add(BuildLine("(E)xit to Town Square", -1, torchGold, dungeonPurple));
				menuLines.Add("╚══════════════════════════════════════════╝".Pastel(dungeonPurple));
				menuLines.Add("");
				menuLines.Add($" {modeHeader.Pastel("#00FFFF")} ");
				menuLines.Add($" {modeToggleHint.Pastel("#555555")} ");
				menuLines.Add("");
				menuLines.Add(" STATUS: " + statusMessage);

				int startX = (85 / 2) - (44 / 2);
				int startY = 6;
				foreach (var line in menuLines) { SetCursorPosition(startX, startY++); WriteLine(line); }

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.Tab) { buyMode = !buyMode; statusMessage = "MODE TOGGLED".Pastel("#FFD700"); }
				else if (key == ConsoleKey.E)
				{
					// Small exit animation
					statusMessage = "TERMINATING UPLINK...".Pastel("#FF0000");
					SetCursorPosition(startX, startY - 1); // Re-draw the status line
					WriteLine(" STATUS: " + statusMessage);
					Thread.Sleep(500);
					shopping = false;
					Clear();
				}
				else if (buyMode)
				{
					if (key == ConsoleKey.S && p.StoreStimStock > 0)
					{
						// CALLING IT HERE: Pass the EXACT name the Registry/JSON expects
						statusMessage = TryBuyReturnStatus("Neural Stim-Pack", stimPrice, p);

						if (statusMessage.Contains("SUCCESS"))
						{
							p.StoreStimStock--;
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
					if (key == ConsoleKey.UpArrow) sellIndex = (sellIndex == 0) ? sellableGroups.Count - 1 : sellIndex - 1;
					if (key == ConsoleKey.DownArrow) sellIndex = (sellIndex == sellableGroups.Count - 1) ? 0 : sellIndex + 1;

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

						statusMessage = $"RECYCLED: {target.Name} (+{finalProfit} CR)".Pastel("#00FF00");

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
					return "SUCCESS: Difficulty Logic Rewritten.".Pastel("#00FF00");
				}

				// Logic for Neural Stim-Pack
				if (item == "Neural Stim-Pack")
				{
					var template = ItemRegistry.GetItem("Neural Stim-Pack");

					if (template != null)
					{
						p.Coins -= cost;
						p.Inventory.Add(new ItemData(template));
						return "SUCCESS: Neural Stim-Pack Synced.".Pastel("#00FF00");
					}

					// If it hits here, the NAME is right, but the Registry is empty or the file wasn't loaded
					return $"ERROR: Registry Template [{item}] Missing from Memory.".Pastel("#FF0000");
				}
			}
			return "DENIED: Insufficient Credits.".Pastel("#FF0000");
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
				string valStr = label.Contains("CREDITS") ? val.ToString() : $"Cost: {val}";
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