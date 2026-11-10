using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;

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
		private const int ScreenWidth = 94;
		private const int ListX = 1, ListWidth = 44;
		private const int DetailX = 46, DetailWidth = 47;
		private const int BoxTop = 2, BoxHeight = 17;
		private const int VisibleRows = BoxHeight - 2;
		private const int SummaryRow = 19, MessageRow = 20, KeysRow = 21;

		private const string Border = "#8B5A2B";
		private const string Gold = "#FFD700";
		private const string Amber = "#FFAB00";
		private const string Dim = "#8B6B4A";
		private const string Muted = "#777777";
		private const string Good = "#7CFC00";
		private const string Bad = "#FF4500";

		private enum Tab { All, Gear, Gems, Potions, Other }
		private static readonly Tab[] Tabs = Enum.GetValues<Tab>();

		/// <summary>One row of the list: a single weapon or piece of gear, or every copy of anything else.</summary>
		private sealed record Entry(ItemData Item, int Quantity);

		/// <summary>A clickable span on one screen row, recorded while drawing.</summary>
		private sealed record Hotspot(int Y, int X1, int X2, string Command);
		private static readonly List<Hotspot> _hotspots = new();

		private static void AddHotspot(int y, int x, int width, string command) =>
			_hotspots.Add(new Hotspot(y, x, x + width - 1, command));

		private static string? HitTest(int x, int y) =>
			_hotspots.FirstOrDefault(h => h.Y == y && x >= h.X1 && x <= h.X2)?.Command;

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
							string? command = HitTest(m.X, m.Y);
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
		private static bool IsGem(ItemData i) => i is GemData || i.Type.Equals("Gem", StringComparison.OrdinalIgnoreCase);
		private static bool IsPotion(ItemData i) => i.Type.Equals("Consumable", StringComparison.OrdinalIgnoreCase);

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
			_hotspots.Clear();

			DrawHeader(player);

			// Filter tabs, top-right corner of the header row
			var tabs = new StringBuilder();
			int tabsWidth = Tabs.Sum(tb => tb.ToString().Length + 2);
			int tabX = ScreenWidth - 1 - tabsWidth;
			int tabsStart = tabX;
			for (int t = 0; t < Tabs.Length; t++)
			{
				string label = $" {Tabs[t].ToString().ToUpper()} ";
				tabs.Append(t == tab ? label.Pastel("#000000").PastelBg(Amber) : label.Pastel(Dim));
				AddHotspot(0, tabX, label.Length, $"tab:{t}");
				tabX += label.Length;
			}
			WriteAt(tabsStart, 0, tabs.ToString());

			// Item list
			var listLines = new List<string>();
			if (entries.Count == 0)
				listLines.Add(Story.Line("inventory.empty").Pastel(Muted));
			for (int r = 0; r < VisibleRows; r++)
			{
				int idx = scroll + r;
				if (idx < entries.Count)
				{
					listLines.Add(ListLine(entries[idx], idx == selected, IsEquipped(entries[idx].Item, equipped)));
					AddHotspot(BoxTop + 1 + r, ListX, ListWidth, $"row:{idx}");
				}
			}
			int packCount = BuildEntries(player, Tab.All).Count;
			DrawBox(ListX, ListWidth, listLines,
				moreAbove: scroll > 0, moreBelow: scroll + VisibleRows < entries.Count);

			// Details
			var detail = entries.Count > 0
				? DetailLines(player, entries[selected], IsEquipped(entries[selected].Item, equipped))
				: new List<string>();
			DrawBox(DetailX, DetailWidth, detail);

			// Summary bar
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
			WriteAt(ListX, SummaryRow, Pad(summary, ScreenWidth - 2));

			// Message and keys
			WriteAt(ListX, MessageRow, Pad(" " + message, ScreenWidth - 2));
			string action = entries.Count > 0 ? ActionLabel(entries[selected].Item, IsEquipped(entries[selected].Item, equipped)) : "";
			var hints = new List<(string Text, string? Command)> { (" [↑↓] Select", null), ("   [←→] Filter", null) };
			if (action.Length > 0) hints.Add(($"   [ENTER] {action}", "use"));
			if (entries.Count > 0) hints.Add(("   [X] Drop", "drop"));
			hints.Add(("   [ESC] Close", "close"));

			var keys = new StringBuilder();
			int hintX = ListX;
			foreach (var (text, command) in hints)
			{
				// Clickable hints are a little brighter than the plain ones.
				keys.Append(text.Pastel(command is null ? Muted : "#B09070"));
				if (command is not null)
				{
					int lead = text.Length - text.TrimStart().Length;
					AddHotspot(KeysRow, hintX + lead, text.Length - lead, command);
				}
				hintX += text.Length;
			}
			WriteAt(ListX, KeysRow, Pad(keys.ToString(), ScreenWidth - 2));

			HudChrome.DisplayFooter(GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);
		}

		private static string ListLine(Entry e, bool selected, bool equipped)
		{
			int inner = ListWidth - 4;
			string tag = equipped ? "[E] " : "";
			string qty = e.Quantity > 1 ? $"x{e.Quantity}" : "";
			string name = Fit(e.Item.Name, inner - 2 - tag.Length - qty.Length - 1);
			string gap = new string(' ', Math.Max(1, inner - 2 - tag.Length - name.Length - qty.Length));

			if (selected)
				return $"> {tag}{name}{gap}{qty}".Pastel("#000000").PastelBg(Gold);

			return "  " + tag.Pastel(Good) + name.Pastel(RarityColor(e.Item.Rarity)) + gap + qty.Pastel(Dim);
		}

		private static List<string> DetailLines(PlayerData.loadPlayer player, Entry e, bool equipped)
		{
			var item = e.Item;
			int inner = DetailWidth - 4;
			var lines = new List<string>
			{
				Fit(item.Name.ToUpper(), inner).Pastel(RarityColor(item.Rarity)),
				$"{item.Rarity} {TypeLabel(item)}".Pastel(Muted),
			};

			string descKey = "item." + Slug(item.Name);
			if (Story.Has(descKey))
			{
				lines.Add("");
				lines.AddRange(Wrap(Story.Line(descKey), inner).Select(l => l.Pastel("#C8B08A")));
			}
			lines.Add("");

			switch (item)
			{
				case WeaponData w:
					int min = w.MinDamage > 0 ? w.MinDamage : (int)Math.Round(w.Damage * 0.8);
					int max = w.MaxDamage > 0 ? w.MaxDamage : (int)Math.Round(w.Damage * 1.2);
					lines.Add(Stat("Damage", $"{min}-{max}", Bad));
					lines.Add(Stat("Crit chance", $"{w.CritChance}%", Amber));
					if (w.Effect != EffectType.None)
						lines.Add(Stat(IsElement(w.Effect) ? "Element" : "Effect", w.Effect.ToString(), "#87CEEB"));
					AddSocketLines(lines, w, inner);
					lines.Add(Stat("Status", equipped ? "EQUIPPED" : "In pack", equipped ? Good : Muted));
					break;

				case ArmorData a:
					lines.Add(Stat("Slot", EquipmentRules.Current.SlotLabel(a.Slot), "#DCDCDC"));
					lines.Add(Stat("Defense", $"+{a.Defense}", "#87CEEB"));
					if (a.BonusHealth != 0) lines.Add(Stat("Max HP", $"+{a.BonusHealth}", Good));
					if (a.LevelReq > 0) lines.Add(Stat("Level req", a.LevelReq.ToString(), a.LevelReq > player.Level ? Bad : "#DCDCDC"));
					AddSocketLines(lines, a, inner);
					lines.Add(Stat("Status", equipped ? "WORN" : "In pack", equipped ? Good : Muted));
					break;

				case GemData g:
					lines.Add(Stat("Power", $"+{g.Power:0.#}", "#BC8FDF"));
					if (g.Attributes.Count > 0) lines.Add(Stat("Element", string.Join(", ", g.Attributes), "#87CEEB"));
					lines.Add("When set in:".Pastel(Dim));
					foreach (var host in EquipmentRules.Current.SocketHosts)
						lines.Add(Fit($"  {host.Label}: {EquipmentRules.Describe(EquipmentRules.Current.GemBonuses(g, host.Id))}", inner).Pastel(Good));
					lines.Add("Only counts in equipped gear.".Pastel(Muted));
					break;

				default:
					if (IsPotion(item)) lines.Add(Stat("Effect", PotionEffect(item), Good));
					if (item.IsQuestItem) lines.Add(Stat("Quest item", "Can't be dropped", Amber));
					break;
			}

			if (e.Quantity > 1) lines.Add(Stat("Carrying", e.Quantity.ToString(), "#DCDCDC"));
			lines.Add(Stat("Weight", $"{item.Weight:0.#}", "#DCDCDC"));
			lines.Add(Stat("Value", $"{item.Value} gold", Gold));

			return lines.Take(VisibleRows).ToList();
		}

		/// <summary>Socket count and each set gem with what it gives in this item.</summary>
		private static void AddSocketLines(List<string> lines, ItemData item, int inner)
		{
			var rules = EquipmentRules.Current;
			int sockets = rules.SocketsOn(item);
			if (sockets <= 0) return;

			string hostId = rules.HostFor(item)!.Id;
			lines.Add(Stat("Sockets", $"{item.SocketedGems.Count}/{sockets}", "#BC8FDF"));
			foreach (var gem in item.SocketedGems.Take(sockets))
				lines.Add(Fit($"  ◆ {gem.Name}: {EquipmentRules.Describe(rules.GemBonuses(gem, hostId))}", inner).Pastel("#BC8FDF"));
		}

		private static string Stat(string label, string value, string color) =>
			$"{label.PadRight(12).Pastel(Dim)}{value.Pastel(color)}";

		private static string TypeLabel(ItemData item) => item switch
		{
			WeaponData w => $"{w.Type} weapon",
			ArmorData a => $"{a.Type.ToLower()} ({EquipmentRules.Current.SlotLabel(a.Slot).ToLower()})",
			_ when IsGem(item) => "gem",
			_ when IsPotion(item) => "potion",
			_ => item.Type.ToLower(),
		};

		private static bool IsElement(EffectType e) =>
			e is EffectType.Fire or EffectType.Ice or EffectType.Poison or EffectType.Electric;

		private static string PotionEffect(ItemData item) => item.Effect switch
		{
			EffectType.Restorative => $"Restores {item.Value} HP",
			EffectType.Speed => "Foe loses a turn (battle)",
			EffectType.Defense => "+5 defense (battle)",
			_ => "None",
		};

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

		// ---------------------------------------------------------------- helpers

		private static void DrawHeader(PlayerData.loadPlayer p)
		{
			string loc = " INVENTORY ".Pastel("#FFFFFF").PastelBg("#8B0000");
			// Name is capped so the header never runs into the filter tabs on the right.
			string stats = $" | 👤 {Fit(p.PlayerName, 10)} | ❤️ HP: {p.Health}/{(int)p.GetTotalMaxHealth()} | 💰 {p.Coins} ".Pastel(Gold);
			SetCursorPosition(0, 0);
			Write(new string(' ', ScreenWidth));
			SetCursorPosition(0, 0);
			Write(loc + stats);
		}

		private static void DrawBox(int x, int width, List<string> lines, bool moreAbove = false, bool moreBelow = false)
		{
			int inner = width - 2;
			WriteAt(x, BoxTop, ("╔" + new string('═', inner) + "╗").Pastel(Border));

			for (int r = 0; r < VisibleRows; r++)
			{
				string content = r < lines.Count ? lines[r] : "";
				string right = r == 0 && moreAbove ? "▲" : r == VisibleRows - 1 && moreBelow ? "▼" : "║";
				WriteAt(x, BoxTop + 1 + r, "║".Pastel(Border) + " " + Pad(content, inner - 2) + " " + right.Pastel(Border));
			}

			WriteAt(x, BoxTop + BoxHeight - 1, ("╚" + new string('═', inner) + "╝").Pastel(Border));
		}

		private static void WriteAt(int x, int y, string text)
		{
			SetCursorPosition(x, y);
			Write(text);
		}

		/// <summary>Pads a (possibly coloured) string to a visible width.</summary>
		private static string Pad(string text, int width)
		{
			int visible = UiEngine.StripAnsi(text).Length;
			return visible >= width ? text : text + new string(' ', width - visible);
		}

		/// <summary>Cuts plain text to a width, adding "..." when it doesn't fit.</summary>
		private static string Fit(string text, int width)
		{
			if (width <= 0) return "";
			return text.Length <= width ? text : text[..Math.Max(0, width - 3)] + "...";
		}

		private static IEnumerable<string> Wrap(string text, int width)
		{
			var line = new StringBuilder();
			foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Length > 0 && line.Length + 1 + word.Length > width)
				{
					yield return line.ToString();
					line.Clear();
				}
				if (line.Length > 0) line.Append(' ');
				line.Append(word);
			}
			if (line.Length > 0) yield return line.ToString();
		}

		/// <summary>"Rogue's Dagger" -> "rogues_dagger", the key used for item descriptions.</summary>
		private static string Slug(string name) =>
			Regex.Replace(name.ToLowerInvariant().Replace("'", ""), "[^a-z0-9]+", "_").Trim('_');

		private static string RarityColor(ItemRarity rarity) => rarity switch
		{
			ItemRarity.Uncommon => "#3CDC3C",
			ItemRarity.Rare => "#4DA6FF",
			ItemRarity.Epic => "#B06CFF",
			ItemRarity.Legendary => "#FF8000",
			ItemRarity.Mythic => "#FF4040",
			_ => "#DCDCDC",
		};
	}
}
