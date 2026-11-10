using DungeonAdventures.Src.GameData.Components;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using Maries_DataModels;
using static System.Console;
using static DungeonAdventures.Src.Village.Places.ItemPanels;

namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>
	/// The forge. Sets gems into weapons, armor and clothing, takes them back out, punches new
	/// sockets, and sells gear. Which items take gems, how many sockets, what gems do and what
	/// everything costs all come from the equipment rules JSON; the stock comes from shops/Blacksmith.json.
	/// Every screen uses the inventory's two-panel layout (<see cref="ItemPanels"/>).
	/// </summary>
	internal class Blacksmith
	{
		private const string Place = "THE FORGE";
		private const string PlaceColor = "#7A3B10";

		private static EquipmentRules Rules => EquipmentRules.Current;

		public static void Open()
		{
			var player = GameState.CurrentPlayer;
			string message = Story.Pick("blacksmith.greeting").Pastel(Amber);
			int selected = 0;

			var rows = new List<PanelRow>
			{
				new("Work on gear", Text, Right: "gem sockets"),
				new("Buy gear", Text, Right: "the smith's racks"),
			};

			while (true)
			{
				var choice = Choose(Place, PlaceColor, player, rows, ForgeDetail, message, Summary(player), "Choose", ref selected);
				if (choice.Back) break;
				message = choice.Index == 0 ? GearMenu(player) : BuyMenu(player);
			}
			Clear();
		}

		private static List<string> ForgeDetail(int index)
		{
			var d = new Detail();
			if (index == 0)
			{
				d.Line("WORK ON GEAR", Gold).Line("Gem sockets", Muted).Blank();
				d.Wrapped("Set gems into weapons, armor and clothing, pry them back out, or have a new socket punched.", Flavour).Blank();
				d.Stat("Set a gem", $"{Rules.SetGemCost} gold", Gold);
				d.Stat("Remove gem", $"{Rules.RemoveGemCost} gold", Gold);
				if (Rules.RemoveGemDestroysGem) d.Add("Removed gems are destroyed.".Pastel(Bad));
				d.Stat("New socket", "price depends on the item", Muted);
			}
			else
			{
				d.Line("BUY GEAR", Gold).Line("The smith's racks", Muted).Blank();
				d.Wrapped("Armor, clothing and gems, hammered out right here in Ashford.", Flavour).Blank();
				int count = ShopLoader.GetByName("Blacksmith")?.InventoryRefs.Count ?? 0;
				d.Stat("For sale", $"{count} items", Text);
			}
			return d.Build();
		}

		// ---------------------------------------------------------------- sockets

		private static string GearMenu(PlayerData.loadPlayer player)
		{
			string message = "";
			int selected = 0;
			while (true)
			{
				var gear = player.Inventory.Where(Rules.CanHoldGems)
					.OrderBy(i => i is WeaponData ? 0 : 1)
					.ThenBy(i => i.Name)
					.ToList();

				if (gear.Count == 0)
					return Story.Line("blacksmith.no_gear").Pastel(Bad);

				var equipped = player.GetEquippedItems().ToList();
				bool IsWorn(ItemData i) => equipped.Any(e => ReferenceEquals(e, i));

				var rows = gear.Select(i => new PanelRow(i.Name, RarityColor(i.Rarity),
					Right: $"◆ {i.SocketedGems.Count}/{Rules.SocketsOn(i)}", RightColor: GemColor,
					Tag: IsWorn(i) ? "[E] " : "")).ToList();

				var choice = Choose(Place, PlaceColor, player, rows,
					i => ItemDetail(player, gear[i], IsWorn(gear[i])).Build(),
					message, Summary(player), "Work on it", ref selected);
				if (choice.Back) return "";
				message = Workbench(player, gear[choice.Index], IsWorn(gear[choice.Index]));
			}
		}

		private static string Workbench(PlayerData.loadPlayer player, ItemData item, bool worn)
		{
			string message = $"At the anvil: {item.Name}.".Pastel(Dim);
			int selected = 0;
			while (true)
			{
				var host = Rules.HostFor(item)!;

				var actions = new List<(PanelRow Row, Func<string> Run)>();
				if (item.HasEmptySocket())
					actions.Add((new PanelRow("Set a gem", Text, Right: $"{Rules.SetGemCost}g", RightColor: Gold), () => SetGem(player, item)));
				if (item.SocketedGems.Count > 0)
					actions.Add((new PanelRow(Rules.RemoveGemDestroysGem ? "Remove a gem (it breaks)" : "Remove a gem", Text,
						Right: $"{Rules.RemoveGemCost}g", RightColor: Gold), () => RemoveGem(player, item)));
				if (Rules.CanPunchSocket(item))
					actions.Add((new PanelRow($"Punch a new socket (max {host.MaxSockets})", Text,
						Right: $"{Rules.PunchCost(item)}g", RightColor: Gold), () => PunchSocket(player, item)));

				if (actions.Count == 0)
					actions.Add((new PanelRow("Nothing more can be done", Muted), () => ""));

				// The item being worked on stays on the right, with its sockets and gems.
				int empty = Rules.SocketsOn(item) - item.SocketedGems.Count;
				var emptyLines = Enumerable.Repeat("  ◇ empty socket".Pastel(Muted), Math.Max(0, empty));

				var choice = Choose(Place, PlaceColor, player, actions.Select(a => a.Row).ToList(),
					_ => ItemDetail(player, item, worn, extra: emptyLines, showValue: false).Build(),
					message, Summary(player), "Do it", ref selected);
				if (choice.Back) return "";
				message = actions[choice.Index].Run();
			}
		}

		private static string SetGem(PlayerData.loadPlayer player, ItemData item)
		{
			var gems = player.Inventory.OfType<GemData>().ToList();
			if (gems.Count == 0)
				return Story.Line("blacksmith.no_gems").Pastel(Bad);

			var host = Rules.HostFor(item)!;
			var rows = gems.Select(g => new PanelRow(g.Name, RarityColor(g.Rarity), Right: $"+{g.Power:0.#}", RightColor: GemColor)).ToList();
			int selected = 0;

			var choice = Choose(Place, PlaceColor, player, rows,
				i => GemForItem(gems[i], item, $"In your {item.Name}:", Rules.SetGemCost, breaks: false),
				$"Which gem goes into the {item.Name}?".Pastel(Dim), Summary(player), "Set it", ref selected);
			if (choice.Back) return "";

			if (!Pay(player, Rules.SetGemCost))
				return Story.Line("blacksmith.no_gold").Pastel(Bad);

			var gem = gems[choice.Index];
			player.Inventory.Remove(gem);
			item.SocketedGems.Add(gem);
			GameState.Sync();
			return Story.Pick("blacksmith.socketed", ("gem", gem.Name), ("weapon", item.Name)).Pastel(Good);
		}

		private static string RemoveGem(PlayerData.loadPlayer player, ItemData item)
		{
			var host = Rules.HostFor(item)!;
			var gems = item.SocketedGems;
			var rows = gems.Select(g => new PanelRow(g.Name, RarityColor(g.Rarity), Right: $"+{g.Power:0.#}", RightColor: GemColor)).ToList();
			int selected = 0;

			var choice = Choose(Place, PlaceColor, player, rows,
				i => GemForItem(gems[i], item, $"Gives your {item.Name}:", Rules.RemoveGemCost, breaks: Rules.RemoveGemDestroysGem),
				$"Which gem comes out of the {item.Name}?".Pastel(Dim), Summary(player), "Remove it", ref selected);
			if (choice.Back) return "";

			if (!Pay(player, Rules.RemoveGemCost))
				return Story.Line("blacksmith.no_gold").Pastel(Bad);

			var gem = gems[choice.Index];
			gems.RemoveAt(choice.Index);
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
			int selected = 0;
			while (true)
			{
				var rows = stock.Select(i => new PanelRow(i.Name, RarityColor(i.Rarity),
					Right: i.LevelReq > player.Level ? $"Lv{i.LevelReq}  {i.Value}g" : $"{i.Value}g",
					RightColor: i.LevelReq > player.Level ? Bad : i.Value > player.Coins ? Muted : Gold)).ToList();

				var choice = Choose(Place, PlaceColor, player, rows,
					i => ItemDetail(player, stock[i], extra: new[]
					{
						Stat("Price", $"{stock[i].Value} gold", stock[i].Value > player.Coins ? Bad : Gold),
					}, showValue: false).Build(),
					message, Summary(player), "Buy", ref selected);
				if (choice.Back) return "";

				var item = stock[choice.Index];
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

		// ---------------------------------------------------------------- helpers

		/// <summary>
		/// A gem's details for one item: what it does there and the price. Shorter than the full gem
		/// panel (which lists every kind of gear) so the part that matters always fits.
		/// </summary>
		private static List<string> GemForItem(GemData gem, ItemData item, string heading, int cost, bool breaks)
		{
			var d = new Detail(gem.Name);
			d.Line(gem.Name.ToUpper(), RarityColor(gem.Rarity));
			d.Line($"{gem.Rarity} gem", Muted);
			string descKey = "item." + Slug(gem.Name);
			if (Story.Has(descKey)) d.Wrapped(Story.Line(descKey), Flavour);
			d.Blank();
			d.Stat("Power", $"+{gem.Power:0.#}", GemColor);
			if (gem.Attributes.Count > 0) d.Stat("Element", string.Join(", ", gem.Attributes), "#87CEEB");
			d.Blank();
			d.Line(heading, Dim);
			d.Line($"  {EquipmentRules.Describe(Rules.GemBonuses(gem, Rules.HostFor(item)!.Id))}", Good);
			d.Blank();
			d.Stat("Cost", $"{cost} gold", Gold);
			if (breaks) d.Add("The gem breaks when removed.".Pastel(Bad));
			return d.Build();
		}

		private static string Summary(PlayerData.loadPlayer player) =>
			$" {"Gold".Pastel(Dim)} {player.Coins.ToString().Pastel(Gold)}" +
			$"   {"Gems in pack".Pastel(Dim)} {player.Inventory.OfType<GemData>().Count().ToString().Pastel(GemColor)}" +
			$"   {"Pack".Pastel(Dim)} {player.Inventory.Count}/{player.InventorySize}";

		private static bool Pay(PlayerData.loadPlayer player, int cost)
		{
			if (player.Coins < cost) return false;
			player.Coins -= cost;
			return true;
		}
	}
}
