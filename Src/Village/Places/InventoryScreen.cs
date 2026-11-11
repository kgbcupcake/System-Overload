using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;
using static DungeonAdventures.Src.Village.Places.ItemPanels;
using static DungeonAdventures.Src.Village.Places.PanelText;
using static DungeonAdventures.Src.Village.Places.ItemInfo;
using static DungeonAdventures.Src.Village.Places.ItemDetails;
using static DungeonAdventures.Src.Village.Places.PanelMenu;

namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>
	/// The hero's pack. A filterable item list on the left, full details of the selected item on
	/// the right, a summary bar underneath, and Equip / Drink / Drop actions.
	/// Layout (94x24): header + filter tabs row 0, panels rows 2-18, summary 19, message 20, keys 21.
	/// Mouse: click a tab to filter, click an item to select it (click it again to use it), scroll the
	/// wheel to move through the list, and click the [ENTER]/[X]/[ESC] hints at the bottom.
	/// </summary>
	internal class InventoryScreen
	{
		private enum Tab { All, Gear, Gems, Potions, Other }
		private static readonly Tab[] Tabs = Enum.GetValues<Tab>();

		/// <summary>One row of the list: a single weapon or piece of gear, or every copy of anything else.</summary>
		private sealed record Entry(ItemData Item, int Quantity);


		public static void Open(PlayerData.loadPlayer player)
		{
			int tab = 0, selected = 0, scroll = 0;
			string message = Story.Pick("inventory.open").Pastel(Dim);
			bool open = true;

			CursorVisible = false;
			Clear();
			using var mouse = MouseInput.Enable();

			while (open)
			{
				var entries = BuildEntries(player, Tabs[tab]);
				selected = Math.Clamp(selected, 0, Math.Max(0, entries.Count - 1));
				if (selected < scroll) scroll = selected;
				if (selected >= scroll + VisibleRows) scroll = selected - VisibleRows + 1;

				Draw(player, entries, tab, selected, scroll, message);

				var input = MouseInput.Read();
				var current = entries.Count > 0 ? entries[selected] : null;
				message = "";

				// Mouse input is turned into the same actions the keys use.
				ConsoleKey key = input.Key?.Key ?? ConsoleKey.NoName;
				if (input.Mouse is { } m)
				{
					switch (m.Action)
					{
						case MouseAction.WheelUp:
							selected = Math.Max(0, selected - 1);
							break;
						case MouseAction.WheelDown:
							selected = Math.Min(Math.Max(0, entries.Count - 1), selected + 1);
							break;
						case MouseAction.LeftClick:
							string? command = PanelHotspots.At(m.X, m.Y);
							if (command is null) break;
							if (command.StartsWith("tab:"))
							{
								tab = int.Parse(command[4..]);
								selected = scroll = 0;
							}
							else if (command.StartsWith("row:"))
							{
								int row = int.Parse(command[4..]);
								if (row == selected) key = ConsoleKey.Enter; // second click uses it
								else selected = row;
							}
							else
							{
								key = command switch
								{
									"use" => ConsoleKey.Enter,
									"drop" => ConsoleKey.X,
									"close" => ConsoleKey.Escape,
									_ => ConsoleKey.NoName,
								};
							}
							break;
					}
				}

				switch (key)
				{
					case ConsoleKey.UpArrow:
						if (entries.Count > 0) selected = selected == 0 ? entries.Count - 1 : selected - 1;
						break;
					case ConsoleKey.DownArrow:
						if (entries.Count > 0) selected = selected == entries.Count - 1 ? 0 : selected + 1;
						break;
					case ConsoleKey.LeftArrow:
						tab = tab == 0 ? Tabs.Length - 1 : tab - 1;
						selected = scroll = 0;
						break;
					case ConsoleKey.RightArrow:
					case ConsoleKey.Tab:
						tab = tab == Tabs.Length - 1 ? 0 : tab + 1;
						selected = scroll = 0;
						break;
					case ConsoleKey.Enter:
						if (current != null) message = Activate(player, current.Item);
						break;
					case ConsoleKey.X:
					case ConsoleKey.Delete:
						if (current != null) message = Drop(player, current.Item);
						break;
					case ConsoleKey.Escape:
					case ConsoleKey.Q:
						open = false;
						break;
				}
			}
			Clear();
		}

		// ---------------------------------------------------------------- list

		private static List<Entry> BuildEntries(PlayerData.loadPlayer player, Tab tab)
		{
			var items = player.Inventory.Where(i => InTab(i, tab)).ToList();
			// Weapons and gear each have their own sockets, so they never stack.
			var singles = items.Where(IsUnique).Select(i => new Entry(i, 1));
			var stacks = items.Where(i => !IsUnique(i))
				.GroupBy(i => i.Name)
				.Select(g => new Entry(g.First(), g.Sum(i => Math.Max(1, i.Amount))));

			return singles.Concat(stacks)
				.OrderBy(e => Category(e.Item))
				.ThenByDescending(e => (int)e.Item.Rarity)
				.ThenBy(e => e.Item.Name)
				.ToList();
		}

		private static bool IsUnique(ItemData i) => i is WeaponData or ArmorData;

		private static int Category(ItemData i) => i is WeaponData ? 0 : i is ArmorData ? 1 : IsGem(i) ? 2 : IsPotion(i) ? 3 : 4;

		/// <summary>True for the weapon in hand and any gear being worn.</summary>
		private static bool IsEquipped(ItemData item, WeaponData? weapon) =>
			item is ArmorData a ? a.IsEquipped : ReferenceEquals(item, weapon);

		private static bool InTab(ItemData i, Tab tab) => tab switch
		{
			Tab.Gear => IsUnique(i), // weapons, armor and clothing
			Tab.Gems => IsGem(i),
			Tab.Potions => IsPotion(i),
			Tab.Other => Category(i) == 4,
			_ => true,
		};

		// ---------------------------------------------------------------- drawing

		private static void Draw(PlayerData.loadPlayer player, List<Entry> entries, int tab, int selected, int scroll, string message)
		{
			var equipped = player.GetEquippedWeapon();
			PanelHotspots.Clear();

			DrawHeader("INVENTORY", "#8B0000", player);
			DrawTabs(Tabs.Select(t => t.ToString()).ToList(), tab);

			// Item list
			var rows = entries.Select(e => new PanelRow(
				e.Item.Name, RarityColor(e.Item.Rarity),
				Right: e.Quantity > 1 ? $"x{e.Quantity}" : "",
				Tag: IsEquipped(e.Item, equipped) ? "[E] " : "")).ToList();
			DrawList(rows, selected, scroll, Story.Line("inventory.empty"));

			// Details, with the item's icon (if it has one) in the top-right corner
			DrawDetail(entries.Count > 0
				? ItemDetail(player, entries[selected].Item, IsEquipped(entries[selected].Item, equipped), entries[selected].Quantity).Build()
				: new List<string>());

			// Summary bar
			int packCount = BuildEntries(player, Tab.All).Count;
			var hero = EnemyFactory.CreatePlayer(player);
			// Strength and gem bonus that combat adds on top of the weapon's own range.
			int bonus = Math.Max(0, (int)player.GetTotalDamage() - 10);
			float weight = player.Inventory.Sum(i => i.Weight * Math.Max(1, i.Amount));
			string summary =
				$" {"Weapon".Pastel(Dim)} {(equipped?.Name ?? "Unarmed").Pastel(Gold)}" +
				$"   {"Damage".Pastel(Dim)} {$"{hero.MinDamage}-{hero.MaxDamage}".Pastel(Bad)}" +
				(bonus > 0 ? $" {$"(+{bonus} bonus)".Pastel(Muted)}" : "") +
				$"   {"Defense".Pastel(Dim)} {player.ArmorValue.ToString().Pastel("#87CEEB")}" +
				$"   {"Weight".Pastel(Dim)} {weight:0.#}" +
				$"   {"Pack".Pastel(Dim)} {packCount}/{player.InventorySize}";

			// Keys
			string action = entries.Count > 0 ? ActionLabel(entries[selected].Item, IsEquipped(entries[selected].Item, equipped)) : "";
			var hints = new List<(string Text, string? Command)> { (" [↑↓] Select", null), ("   [←→] Filter", null) };
			if (action.Length > 0) hints.Add(($"   [ENTER] {action}", "use"));
			if (entries.Count > 0) hints.Add(("   [X] Drop", "drop"));
			hints.Add(("   [ESC] Close", "close"));

			DrawFooter(summary, message, hints);
		}

		private static string ActionLabel(ItemData item, bool equipped) => item switch
		{
			WeaponData => equipped ? "" : "Equip",
			ArmorData => equipped ? "Take off" : "Wear",
			_ when IsPotion(item) && item.Effect == EffectType.Restorative => "Drink",
			_ => "",
		};

		// ---------------------------------------------------------------- actions

		private static string Activate(PlayerData.loadPlayer player, ItemData item)
		{
			var name = ("item", (object?)item.Name);

			if (item is WeaponData w)
			{
				if (ReferenceEquals(player.GetEquippedWeapon(), w))
					return Story.Line("inventory.already_equipped", name).Pastel(Muted);
				player.EquippedWeapon = w.Name;
				GameState.Sync();
				return Story.Pick("inventory.equip", name).Pastel(Good);
			}

			if (item is ArmorData a)
			{
				if (a.IsEquipped)
				{
					a.IsEquipped = false;
					GameState.Sync();
					return Story.Pick("inventory.take_off", name).Pastel(Muted);
				}
				if (a.LevelReq > player.Level)
					return Story.Line("inventory.level_too_low", name, ("level", a.LevelReq)).Pastel(Bad);
				player.EquipArmor(a);
				GameState.Sync();
				return Story.Pick("inventory.wear", name).Pastel(Good);
			}

			if (IsGem(item))
				return Story.Line("inventory.gem_hint", name).Pastel(Dim);

			if (IsPotion(item))
			{
				if (item.Effect != EffectType.Restorative)
					return Story.Line("inventory.battle_only", name).Pastel(Dim);

				int max = (int)player.GetTotalMaxHealth();
				if (player.Health >= max)
					return Story.Line("inventory.full_health", name).Pastel(Amber);

				int heal = Math.Min(max - player.Health, item.Value > 0 ? item.Value : 25);
				player.Health += heal;
				RemoveOne(player, item);
				GameState.Sync();
				return Story.Pick("inventory.drink", name, ("hp", heal)).Pastel(Good);
			}

			return Story.Line("inventory.cant_use", name).Pastel(Muted);
		}

		private static string Drop(PlayerData.loadPlayer player, ItemData item)
		{
			var name = ("item", (object?)item.Name);
			if (item.IsQuestItem)
				return Story.Line("inventory.cant_drop", name).Pastel(Amber);

			string question = $" Drop the {item.Name}?  ";
			WriteAt(ListX, MessageRow, Pad((question + "[Y] Yes   [N] No").Pastel(Bad), ScreenWidth - 2));
			int yesX = ListX + question.Length;

			// Y, or a click on "[Y] Yes", confirms. Anything else cancels.
			var answer = MouseInput.Read();
			bool yes = answer.Key?.Key == ConsoleKey.Y
				|| (answer.Mouse is { Action: MouseAction.LeftClick } click
					&& click.Y == MessageRow && click.X >= yesX && click.X < yesX + "[Y] Yes".Length);
			if (!yes)
				return "";

			RemoveOne(player, item);
			if (item is WeaponData && player.EquippedWeapon == item.Name && !player.Inventory.OfType<WeaponData>().Any(x => x.Name == item.Name))
				player.EquippedWeapon = "";
			GameState.Sync();
			return Story.Pick("inventory.drop", name).Pastel(Bad);
		}

		/// <summary>Uses up one of an item: lowers a stack's Amount, or removes the item.</summary>
		private static void RemoveOne(PlayerData.loadPlayer player, ItemData item)
		{
			var target = IsUnique(item)
				? item
				: player.Inventory.FirstOrDefault(i => !IsUnique(i) && i.Name == item.Name) ?? item;
			if (!IsUnique(target) && target.Amount > 1)
				target.Amount--;
			else
				player.Inventory.Remove(target);
		}
	}
}
