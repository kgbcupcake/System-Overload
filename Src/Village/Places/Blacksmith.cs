using DungeonAdventures.Src.GameData.Components;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using Maries_DataModels;
using static System.Console;

namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>
	/// The forge. Sets gems into weapons, armor and clothing, takes them back out, punches new
	/// sockets, and sells gear. Which items take gems, how many sockets, what gems do and what
	/// everything costs all come from the equipment rules JSON; the stock comes from shops/Blacksmith.json.
	/// </summary>
	internal class Blacksmith
	{
		private const string Gold = "#FFD700";
		private const string Amber = "#FFAB00";
		private const string Gem = "#BC8FDF";
		private const string Muted = "#555555";
		private const string Good = "#00FF00";
		private const string Bad = "#FF4500";

		private static EquipmentRules Rules => EquipmentRules.Current;

		public static void Open()
		{
			var player = GameState.CurrentPlayer;
			CursorVisible = false;
			string message = Story.Pick("blacksmith.greeting").Pastel(Amber);

			while (true)
			{
				int choice = Choose("=== THE FORGE ===", new[] { message, $"Gold: {player.Coins}".Pastel(Gold) },
					new List<string> { "Work on gear (gem sockets)", "Buy gear" });

				if (choice == -1) break;
				message = choice == 0 ? GearMenu(player) : BuyMenu(player);
			}
			Clear();
		}

		// ---------------------------------------------------------------- sockets

		private static string GearMenu(PlayerData.loadPlayer player)
		{
			string message = "";
			while (true)
			{
				var gear = player.Inventory.Where(Rules.CanHoldGems)
					.OrderBy(i => i is WeaponData ? 0 : 1)
					.ThenBy(i => i.Name)
					.ToList();

				if (gear.Count == 0)
					return Story.Line("blacksmith.no_gear").Pastel(Bad);

				var equipped = player.GetEquippedItems().ToList();
				var lines = gear.Select(i =>
				{
					string tag = equipped.Any(e => ReferenceEquals(e, i)) ? "[E] " : "";
					return $"{tag}{i.Name} ({HostLabel(i)}) [{i.SocketedGems.Count}/{Rules.SocketsOn(i)}]";
				}).ToList();

				int pick = Choose("=== CHOOSE GEAR TO WORK ON ===", new[] { message, $"Gold: {player.Coins}".Pastel(Gold) }, lines);
				if (pick == -1) return "";
				message = Workbench(player, gear[pick]);
			}
		}

		private static string Workbench(PlayerData.loadPlayer player, ItemData item)
		{
			string message = "";
			while (true)
			{
				var host = Rules.HostFor(item)!;
				int sockets = Rules.SocketsOn(item);

				var info = new List<string>
				{
					message,
					$"{item.Name} ({host.Label})  Sockets {item.SocketedGems.Count}/{sockets}  Gold {player.Coins}".Pastel(Gold),
				};
				foreach (var gem in item.SocketedGems)
					info.Add($"◆ {gem.Name}: {EquipmentRules.Describe(Rules.GemBonuses(gem, host.Id))}".Pastel(Gem));
				for (int i = item.SocketedGems.Count; i < sockets; i++)
					info.Add("◇ empty socket".Pastel(Muted));

				var actions = new List<(string Label, Func<string> Run)>();
				if (item.HasEmptySocket())
					actions.Add(($"Set a gem ({Rules.SetGemCost}g)", () => SetGem(player, item)));
				if (item.SocketedGems.Count > 0)
					actions.Add(($"Remove a gem ({Rules.RemoveGemCost}g{(Rules.RemoveGemDestroysGem ? ", gem is lost" : "")})", () => RemoveGem(player, item)));
				if (Rules.CanPunchSocket(item))
					actions.Add(($"Punch a new socket ({Rules.PunchCost(item)}g, max {host.MaxSockets})", () => PunchSocket(player, item)));

				if (actions.Count == 0)
					actions.Add(("Nothing more can be done here", () => ""));

				int pick = Choose($"=== WORKING: {item.Name.ToUpper()} ===", info, actions.Select(a => a.Label).ToList());
				if (pick == -1) return "";
				message = actions[pick].Run();
			}
		}

		private static string SetGem(PlayerData.loadPlayer player, ItemData item)
		{
			var gems = player.Inventory.OfType<GemData>().ToList();
			if (gems.Count == 0)
				return Story.Line("blacksmith.no_gems").Pastel(Bad);

			var host = Rules.HostFor(item)!;
			var lines = gems.Select(g => $"{g.Name}  →  {EquipmentRules.Describe(Rules.GemBonuses(g, host.Id))}").ToList();
			int pick = Choose($"=== SET A GEM IN: {item.Name.ToUpper()} ===", new[] { $"Cost: {Rules.SetGemCost}g   Gold: {player.Coins}".Pastel(Gold) }, lines);
			if (pick == -1) return "";

			if (!Pay(player, Rules.SetGemCost))
				return Story.Line("blacksmith.no_gold").Pastel(Bad);

			var gem = gems[pick];
			player.Inventory.Remove(gem);
			item.SocketedGems.Add(gem);
			GameState.Sync();
			return Story.Pick("blacksmith.socketed", ("gem", gem.Name), ("weapon", item.Name)).Pastel(Good);
		}

		private static string RemoveGem(PlayerData.loadPlayer player, ItemData item)
		{
			var lines = item.SocketedGems.Select(g => g.Name).ToList();
			int pick = Choose($"=== REMOVE A GEM FROM: {item.Name.ToUpper()} ===", new[] { $"Cost: {Rules.RemoveGemCost}g   Gold: {player.Coins}".Pastel(Gold) }, lines);
			if (pick == -1) return "";

			if (!Pay(player, Rules.RemoveGemCost))
				return Story.Line("blacksmith.no_gold").Pastel(Bad);

			var gem = item.SocketedGems[pick];
			item.SocketedGems.RemoveAt(pick);
			if (Rules.RemoveGemDestroysGem)
			{
				GameState.Sync();
				return Story.Pick("blacksmith.gem_shattered", ("gem", gem.Name), ("item", item.Name)).Pastel(Bad);
			}

			player.Inventory.Add(gem);
			GameState.Sync();
			return Story.Pick("blacksmith.gem_removed", ("gem", gem.Name), ("item", item.Name)).Pastel(Good);
		}

		private static string PunchSocket(PlayerData.loadPlayer player, ItemData item)
		{
			if (!Pay(player, Rules.PunchCost(item)))
				return Story.Line("blacksmith.no_gold").Pastel(Bad);

			item.NumSockets++;
			GameState.Sync();
			return Story.Pick("blacksmith.socket_punched", ("item", item.Name)).Pastel(Good);
		}

		// ---------------------------------------------------------------- shop

		private static string BuyMenu(PlayerData.loadPlayer player)
		{
			var stock = (ShopLoader.GetByName("Blacksmith")?.InventoryRefs ?? new List<string>())
				.Select(ItemRegistry.CreateCopy)
				.OfType<ItemData>()
				.ToList();

			if (stock.Count == 0)
				return Story.Line("blacksmith.no_stock").Pastel(Bad);

			string message = "";
			while (true)
			{
				var lines = stock.Select(StockLine).ToList();
				int pick = Choose("=== BLACKSMITH'S WARES ===", new[] { message, $"Gold: {player.Coins}".Pastel(Gold) }, lines);
				if (pick == -1) return "";

				var item = stock[pick];
				if (item.LevelReq > player.Level)
				{
					message = Story.Line("blacksmith.level_too_low", ("item", item.Name), ("level", item.LevelReq)).Pastel(Bad);
					continue;
				}
				if (player.Inventory.Count >= player.InventorySize)
				{
					message = Story.Line("blacksmith.pack_full").Pastel(Bad);
					continue;
				}
				if (!Pay(player, item.Value))
				{
					message = Story.Line("blacksmith.no_gold").Pastel(Bad);
					continue;
				}

				// A fresh copy each time, so two of the same item don't share sockets.
				player.Inventory.Add(ItemRegistry.CreateCopy(item.Name) ?? item);
				GameState.Sync();
				message = Story.Pick("blacksmith.bought", ("item", item.Name), ("cost", item.Value)).Pastel(Good);
			}
		}

		private static string StockLine(ItemData item)
		{
			string stats = item switch
			{
				ArmorData a => $"{Rules.SlotLabel(a.Slot)} {a.Type}, Def {a.Defense}{(a.BonusHealth > 0 ? $", +{a.BonusHealth} HP" : "")}",
				GemData g => $"Gem, power {g.Power:0.#}",
				WeaponData w => $"{w.Type}, Dmg {w.Damage}",
				_ => item.Type,
			};
			string sockets = Rules.SocketsOn(item) > 0 ? $", {Rules.SocketsOn(item)} socket(s)" : "";
			string level = item.LevelReq > 0 ? $"  Lv{item.LevelReq}" : "";
			return $"{item.Name} ({stats}{sockets}) {item.Value}g{level}";
		}

		// ---------------------------------------------------------------- helpers

		private static string HostLabel(ItemData item) => item switch
		{
			ArmorData a => $"{Rules.SlotLabel(a.Slot)} {Rules.HostFor(item)?.Label}",
			_ => Rules.HostFor(item)?.Label ?? item.Type,
		};

		private static bool Pay(PlayerData.loadPlayer player, int cost)
		{
			if (player.Coins < cost) return false;
			player.Coins -= cost;
			return true;
		}

		/// <summary>
		/// A simple arrow-key menu. Returns the chosen index, or -1 on Escape.
		/// </summary>
		private static int Choose(string title, IEnumerable<string> info, List<string> options)
		{
			int selected = 0;
			while (true)
			{
				Clear();
				UiEngine.DrawCentered(title.Pastel(Amber), 3);

				int row = 5;
				foreach (var line in info.Where(l => !string.IsNullOrEmpty(l)))
					UiEngine.DrawCentered(line, row++);

				row++;
				MouseInput.ClearZones();
				for (int i = 0; i < options.Count; i++)
				{
					MouseInput.AddZone(row + i, 0, 94, i);
					string line = i == selected
						? $">> {options[i]} <<".Pastel(Good)
						: $"   {options[i]}   ".Pastel("#AAAAAA");
					UiEngine.DrawCentered(line, row + i);
				}

				string hint = "[↑↓] Select   [ENTER] Choose   [ESC] Back";
				UiEngine.DrawCentered(hint.Pastel(Gold), row + options.Count + 2);
				// "[ESC] Back" at the end of the centred hint is clickable.
				int hintX = 94 / 2 - hint.Length / 2;
				MouseInput.AddKeyZone(row + options.Count + 2, hintX + hint.Length - "[ESC] Back".Length, "[ESC] Back".Length, ConsoleKey.Escape);

				var key = MouseInput.ReadMenuKey(ref selected, options.Count);
				if (key is ConsoleKey.UpArrow or ConsoleKey.DownArrow)
					selected = MenuNavigator.HandleVerticalNav(key, selected, options.Count);
				else if (key == ConsoleKey.Enter && options.Count > 0)
					return selected;
				else if (key == ConsoleKey.Escape)
					return -1;
			}
		}
	}
}
