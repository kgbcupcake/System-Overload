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
	/// <summary>One row of a panel list: a name on the left, an optional tag before it and a note on the right.</summary>
	internal sealed record PanelRow(string Name, string Color = "#DCDCDC", string Right = "", string RightColor = ItemPanels.Dim, string Tag = "");

	/// <summary>
	/// The two-panel look shared by the inventory, the General Store and the Blacksmith: a list on
	/// the left, details of the selected entry (with its icon) on the right, a summary bar, a message
	/// line and clickable key hints underneath.
	/// Layout (94x24): header row 0, panels rows 2-18, summary 19, message 20, keys 21.
	/// </summary>
	internal static class ItemPanels
	{
		public const int ScreenWidth = 94;
		public const int ListX = 1, ListWidth = 44;
		public const int DetailX = 46, DetailWidth = 47;
		public const int BoxTop = 2, BoxHeight = 17;
		public const int VisibleRows = BoxHeight - 2;
		public const int SummaryRow = 19, MessageRow = 20, KeysRow = 21;
		/// <summary>Visible width of text inside a detail panel.</summary>
		public const int DetailInner = DetailWidth - 4;

		public const string Border = "#8B5A2B";
		public const string Gold = "#FFD700";
		public const string Amber = "#FFAB00";
		public const string Dim = "#8B6B4A";
		public const string Muted = "#777777";
		public const string Good = "#7CFC00";
		public const string Bad = "#FF4500";
		public const string Text = "#DCDCDC";
		public const string GemColor = "#BC8FDF";
		public const string Flavour = "#C8B08A";

		// ---------------------------------------------------------------- click spots

		/// <summary>A clickable span on one screen row, recorded while drawing.</summary>
		private sealed record Hotspot(int Y, int X1, int X2, string Command);
		private static readonly List<Hotspot> _hotspots = new();

		public static void ClearHotspots() => _hotspots.Clear();

		public static void AddHotspot(int y, int x, int width, string command) =>
			_hotspots.Add(new Hotspot(y, x, x + width - 1, command));

		public static string? HitTest(int x, int y) =>
			_hotspots.FirstOrDefault(h => h.Y == y && x >= h.X1 && x <= h.X2)?.Command;

		// ---------------------------------------------------------------- frame

		/// <summary>Row 0: a coloured place label, the hero's name, HP and gold.</summary>
		public static void DrawHeader(string place, string placeColor, PlayerData.loadPlayer p)
		{
			string loc = $" {place} ".Pastel("#FFFFFF").PastelBg(placeColor);
			// Name is capped so the header never runs into the tabs on the right.
			string stats = $" | 👤 {Fit(p.PlayerName, 10)} | ❤️ HP: {p.Health}/{(int)p.GetTotalMaxHealth()} | 💰 {p.Coins} ".Pastel(Gold);
			SetCursorPosition(0, 0);
			Write(new string(' ', ScreenWidth));
			SetCursorPosition(0, 0);
			Write(loc + stats);
		}

		/// <summary>Tabs in the top-right corner of row 0. Each is clickable as "tab:i".</summary>
		public static void DrawTabs(IReadOnlyList<string> labels, int active)
		{
			var tabs = new StringBuilder();
			int tabX = ScreenWidth - 1 - labels.Sum(l => l.Length + 2);
			int start = tabX;
			for (int t = 0; t < labels.Count; t++)
			{
				string label = $" {labels[t].ToUpper()} ";
				tabs.Append(t == active ? label.Pastel("#000000").PastelBg(Amber) : label.Pastel(Dim));
				AddHotspot(0, tabX, label.Length, $"tab:{t}");
				tabX += label.Length;
			}
			WriteAt(start, 0, tabs.ToString());
		}

		/// <summary>The left list. Rows are clickable as "row:i".</summary>
		public static void DrawList(IReadOnlyList<PanelRow> rows, int selected, int scroll, string emptyText)
		{
			var lines = new List<string>();
			if (rows.Count == 0)
				lines.Add(emptyText.Pastel(Muted));
			for (int r = 0; r < VisibleRows; r++)
			{
				int idx = scroll + r;
				if (idx >= rows.Count) break;
				lines.Add(ListLine(rows[idx], idx == selected));
				AddHotspot(BoxTop + 1 + r, ListX, ListWidth, $"row:{idx}");
			}
			DrawBox(ListX, ListWidth, lines, moreAbove: scroll > 0, moreBelow: scroll + VisibleRows < rows.Count);
		}

		public static void DrawDetail(List<string> lines) => DrawBox(DetailX, DetailWidth, lines);

		/// <summary>Summary bar, message line and the key hints (hints with a command are clickable).</summary>
		public static void DrawFooter(string summary, string message, IEnumerable<(string Text, string? Command)> hints)
		{
			WriteAt(ListX, SummaryRow, Pad(summary, ScreenWidth - 2));
			WriteAt(ListX, MessageRow, Pad(" " + message, ScreenWidth - 2));

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

		public static string ListLine(PanelRow row, bool selected)
		{
			int inner = ListWidth - 4;
			string name = Fit(row.Name, inner - 2 - row.Tag.Length - row.Right.Length - 1);
			string gap = new string(' ', Math.Max(1, inner - 2 - row.Tag.Length - name.Length - row.Right.Length));

			if (selected)
				return $"> {row.Tag}{name}{gap}{row.Right}".Pastel("#000000").PastelBg(Gold);

			return "  " + row.Tag.Pastel(Good) + name.Pastel(row.Color) + gap + row.Right.Pastel(row.RightColor);
		}

		// ---------------------------------------------------------------- the main loop

		/// <summary>What the player did on a <see cref="Choose"/> screen.</summary>
		public readonly record struct Choice(int Index, int Tab)
		{
			public bool Back => Index < 0 && Tab < 0;
		}

		/// <summary>
		/// Shows a list with details and waits for a pick. Returns the chosen row, a tab switch
		/// (when <paramref name="tabs"/> are given), or Back on Escape. <paramref name="selected"/>
		/// keeps the highlighted row between calls.
		/// </summary>
		public static Choice Choose(
			string place, string placeColor, PlayerData.loadPlayer player,
			IReadOnlyList<PanelRow> rows, Func<int, List<string>> detail,
			string message, string summary, string actionLabel, ref int selected,
			string emptyText = "Nothing here.", IReadOnlyList<string>? tabs = null, int activeTab = 0)
		{
			int scroll = 0;
			CursorVisible = false;
			Clear();
			using var mouse = MouseInput.Enable();

			while (true)
			{
				selected = Math.Clamp(selected, 0, Math.Max(0, rows.Count - 1));
				if (selected < scroll) scroll = selected;
				if (selected >= scroll + VisibleRows) scroll = selected - VisibleRows + 1;

				ClearHotspots();
				DrawHeader(place, placeColor, player);
				if (tabs != null) DrawTabs(tabs, activeTab);
				DrawList(rows, selected, scroll, emptyText);
				DrawDetail(rows.Count > 0 ? detail(selected) : new List<string>());

				var hints = new List<(string, string?)> { (" [↑↓] Select", null) };
				if (tabs != null) hints.Add(("   [←→] Switch", null));
				if (rows.Count > 0 && actionLabel.Length > 0) hints.Add(($"   [ENTER] {actionLabel}", "use"));
				hints.Add(("   [ESC] Back", "close"));
				DrawFooter(summary, message, hints);

				var input = MouseInput.Read();
				ConsoleKey key = input.Key?.Key ?? ConsoleKey.NoName;
				if (input.Mouse is { } m)
				{
					switch (m.Action)
					{
						case MouseAction.WheelUp: selected = Math.Max(0, selected - 1); break;
						case MouseAction.WheelDown: selected = Math.Min(Math.Max(0, rows.Count - 1), selected + 1); break;
						case MouseAction.LeftClick:
							string? command = HitTest(m.X, m.Y);
							if (command is null) break;
							if (command.StartsWith("tab:"))
								return new Choice(-1, int.Parse(command[4..]));
							if (command.StartsWith("row:"))
							{
								int row = int.Parse(command[4..]);
								if (row == selected) key = ConsoleKey.Enter; // second click picks it
								else selected = row;
							}
							else key = command == "use" ? ConsoleKey.Enter : command == "close" ? ConsoleKey.Escape : key;
							break;
					}
				}

				switch (key)
				{
					case ConsoleKey.UpArrow:
						if (rows.Count > 0) selected = selected == 0 ? rows.Count - 1 : selected - 1;
						break;
					case ConsoleKey.DownArrow:
						if (rows.Count > 0) selected = selected == rows.Count - 1 ? 0 : selected + 1;
						break;
					case ConsoleKey.LeftArrow when tabs != null:
						return new Choice(-1, activeTab == 0 ? tabs.Count - 1 : activeTab - 1);
					case ConsoleKey.RightArrow or ConsoleKey.Tab when tabs != null:
						return new Choice(-1, activeTab == tabs.Count - 1 ? 0 : activeTab + 1);
					case ConsoleKey.Enter when rows.Count > 0 && actionLabel.Length > 0:
						return new Choice(selected, -1);
					case ConsoleKey.Escape:
					case ConsoleKey.Q:
						return new Choice(-1, -1);
				}
			}
		}

		// ---------------------------------------------------------------- detail panel

		/// <summary>
		/// Builds the lines of a detail panel. With an icon, the first rows are kept narrow so text
		/// wraps beside it; <see cref="Build"/> places the icon in the top-right corner.
		/// </summary>
		public sealed class Detail
		{
			private const int IconGap = 2;
			private readonly List<string> _lines = new();
			private readonly string[]? _icon;
			private readonly int _narrow;

			public Detail(string? iconFor = null)
			{
				_icon = iconFor != null ? PixelIcon.ForItem(iconFor) : null;
				_narrow = _icon != null ? DetailInner - PixelIcon.Width(_icon) - IconGap : DetailInner;
			}

			public bool HasIcon => _icon != null;
			public int Count => _lines.Count;

			/// <summary>Width available to the next line.</summary>
			public int Width => _lines.Count < (_icon?.Length ?? 0) ? _narrow : DetailInner;

			public Detail Add(string line) { _lines.Add(line); return this; }
			public Detail Blank() => Add("");
			public Detail Stat(string label, string value, string color) => Add(ItemPanels.Stat(label, value, color));

			/// <summary>Plain text cut to the current width, then coloured.</summary>
			public Detail Line(string text, string color) => Add(Fit(text, Width).Pastel(color));

			/// <summary>Word-wrapped text, narrower while beside the icon.</summary>
			public Detail Wrapped(string text, string color)
			{
				var words = new Queue<string>(text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
				while (words.Count > 0)
					_lines.Add(TakeLine(words, Width).Pastel(color));
				return this;
			}

			public List<string> Build()
			{
				var lines = new List<string>(_lines);
				int iconRows = _icon?.Length ?? 0;
				for (int r = 0; r < iconRows; r++)
				{
					// Where the icon has no pixels (its transparent margin) the line can use the full width.
					if (r < lines.Count && UiEngine.StripAnsi(_icon![r]).Trim().Length == 0) continue;
					// A few stat lines can still be a little too long for the narrow column: cut those.
					string text = r < lines.Count ? ClipVisible(lines[r], _narrow) : "";
					string row = Pad(text, _narrow) + new string(' ', IconGap) + _icon![r];
					if (r < lines.Count) lines[r] = row; else lines.Add(row);
				}
				return lines.Take(VisibleRows).ToList();
			}
		}

		/// <summary>
		/// Full details of an item with its icon. <paramref name="equipped"/> null leaves out the
		/// equipped/worn line (shop stock); <paramref name="extra"/> lines go after the stats, before
		/// weight and value.
		/// </summary>
		public static Detail ItemDetail(PlayerData.loadPlayer player, ItemData item, bool? equipped = null,
			int quantity = 1, IEnumerable<string>? extra = null, bool showValue = true)
		{
			var d = new Detail(item.Name);
			d.Line(item.Name.ToUpper(), RarityColor(item.Rarity));
			d.Line($"{item.Rarity} {TypeLabel(item)}", Muted);

			string descKey = "item." + Slug(item.Name);
			if (Story.Has(descKey))
			{
				// The narrow column beside an icon wraps to more lines, so skip this spacer to keep
				// the stats at the bottom inside the panel.
				if (!d.HasIcon) d.Blank();
				d.Wrapped(Story.Line(descKey), Flavour);
			}
			d.Blank();

			var rules = EquipmentRules.Current;
			switch (item)
			{
				case WeaponData w:
					int min = w.MinDamage > 0 ? w.MinDamage : (int)Math.Round(w.Damage * 0.8);
					int max = w.MaxDamage > 0 ? w.MaxDamage : (int)Math.Round(w.Damage * 1.2);
					d.Stat("Damage", $"{min}-{max}", Bad);
					d.Stat("Crit chance", $"{w.CritChance}%", Amber);
					if (w.Effect != EffectType.None)
						d.Stat(IsElement(w.Effect) ? "Element" : "Effect", w.Effect.ToString(), "#87CEEB");
					if (w.LevelReq > 0) d.Stat("Level req", w.LevelReq.ToString(), w.LevelReq > player.Level ? Bad : Text);
					AddSocketLines(d, w);
					if (equipped is { } we) d.Stat("Status", we ? "EQUIPPED" : "In pack", we ? Good : Muted);
					break;

				case ArmorData a:
					d.Stat("Slot", rules.SlotLabel(a.Slot), Text);
					d.Stat("Defense", $"+{a.Defense}", "#87CEEB");
					if (a.BonusHealth != 0) d.Stat("Max HP", $"+{a.BonusHealth}", Good);
					if (a.LevelReq > 0) d.Stat("Level req", a.LevelReq.ToString(), a.LevelReq > player.Level ? Bad : Text);
					AddSocketLines(d, a);
					if (equipped is { } ae) d.Stat("Status", ae ? "WORN" : "In pack", ae ? Good : Muted);
					break;

				case GemData g:
					d.Stat("Power", $"+{g.Power:0.#}", GemColor);
					if (g.Attributes.Count > 0) d.Stat("Element", string.Join(", ", g.Attributes), "#87CEEB");
					d.Add("When set in:".Pastel(Dim));
					foreach (var host in rules.SocketHosts)
						d.Line($"  {host.Label}: {EquipmentRules.Describe(rules.GemBonuses(g, host.Id))}", Good);
					d.Add("Only counts in equipped gear.".Pastel(Muted));
					break;

				default:
					if (IsPotion(item)) d.Stat("Effect", PotionEffect(item), Good);
					if (item.IsQuestItem) d.Stat("Quest item", "Can't drop", Amber);
					break;
			}

			if (extra != null)
				foreach (var line in extra) d.Add(line);

			if (quantity > 1) d.Stat("Carrying", quantity.ToString(), Text);
			d.Stat("Weight", $"{item.Weight:0.#}", Text);
			if (showValue) d.Stat("Value", $"{item.Value} gold", Gold);
			return d;
		}

		/// <summary>Socket count and each set gem with what it gives in this item.</summary>
		private static void AddSocketLines(Detail d, ItemData item)
		{
			var rules = EquipmentRules.Current;
			int sockets = rules.SocketsOn(item);
			if (sockets <= 0) return;

			string hostId = rules.HostFor(item)!.Id;
			d.Stat("Sockets", $"{item.SocketedGems.Count}/{sockets}", GemColor);
			foreach (var gem in item.SocketedGems.Take(sockets))
				d.Line($"  ◆ {gem.Name}: {EquipmentRules.Describe(rules.GemBonuses(gem, hostId))}", GemColor);
		}

		// ---------------------------------------------------------------- item helpers

		public static bool IsGem(ItemData i) => i is GemData || i.Type.Equals("Gem", StringComparison.OrdinalIgnoreCase);
		public static bool IsPotion(ItemData i) => i.Type.Equals("Consumable", StringComparison.OrdinalIgnoreCase);

		public static string TypeLabel(ItemData item) => item switch
		{
			WeaponData w => $"{w.Type} weapon",
			ArmorData a => $"{a.Type.ToLower()} ({EquipmentRules.Current.SlotLabel(a.Slot).ToLower()})",
			_ when IsGem(item) => "gem",
			_ when IsPotion(item) => "potion",
			_ => item.Type.ToLower(),
		};

		public static bool IsElement(EffectType e) =>
			e is EffectType.Fire or EffectType.Ice or EffectType.Poison or EffectType.Electric;

		public static string PotionEffect(ItemData item) => item.Effect switch
		{
			EffectType.Restorative => $"Heals {item.Value} HP",
			EffectType.Speed => "Foe loses a turn (battle)",
			EffectType.Defense => "+5 defense (battle)",
			_ => "None",
		};

		public static string RarityColor(ItemRarity rarity) => rarity switch
		{
			ItemRarity.Uncommon => "#3CDC3C",
			ItemRarity.Rare => "#4DA6FF",
			ItemRarity.Epic => "#B06CFF",
			ItemRarity.Legendary => "#FF8000",
			ItemRarity.Mythic => "#FF4040",
			_ => Text,
		};

		/// <summary>"Rogue's Dagger" -> "rogues_dagger", the key used for item descriptions.</summary>
		public static string Slug(string name) =>
			Regex.Replace(name.ToLowerInvariant().Replace("'", ""), "[^a-z0-9]+", "_").Trim('_');

		// ---------------------------------------------------------------- text helpers

		public static string Stat(string label, string value, string color) =>
			$"{label.PadRight(12).Pastel(Dim)}{value.Pastel(color)}";

		public static void DrawBox(int x, int width, List<string> lines, bool moreAbove = false, bool moreBelow = false)
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

		public static void WriteAt(int x, int y, string text)
		{
			SetCursorPosition(x, y);
			Write(text);
		}

		/// <summary>Pads a (possibly coloured) string to a visible width.</summary>
		public static string Pad(string text, int width)
		{
			int visible = UiEngine.StripAnsi(text).Length;
			return visible >= width ? text : text + new string(' ', width - visible);
		}

		/// <summary>Cuts plain text to a width, adding "..." when it doesn't fit.</summary>
		public static string Fit(string text, int width)
		{
			if (width <= 0) return "";
			return text.Length <= width ? text : text[..Math.Max(0, width - 3)] + "...";
		}

		/// <summary>Takes as many whole words as fit in one line of the given width.</summary>
		public static string TakeLine(Queue<string> words, int width)
		{
			var line = new StringBuilder(words.Dequeue());
			while (words.Count > 0 && line.Length + 1 + words.Peek().Length <= width)
				line.Append(' ').Append(words.Dequeue());
			return Fit(line.ToString(), width); // a single word longer than the line
		}

		/// <summary>Word-wraps plain text to lines of the given width.</summary>
		public static List<string> Wrap(string text, int width)
		{
			var words = new Queue<string>(text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
			var lines = new List<string>();
			while (words.Count > 0) lines.Add(TakeLine(words, width));
			return lines;
		}

		/// <summary>Cuts a coloured string to a visible width, ending in "…", keeping its colour codes intact.</summary>
		public static string ClipVisible(string text, int width)
		{
			if (UiEngine.StripAnsi(text).Length <= width) return text;
			var sb = new StringBuilder();
			int visible = 0;
			for (int i = 0; i < text.Length && visible < width - 1; i++)
			{
				if (text[i] == '\x1b')
				{
					int end = text.IndexOf('m', i);
					if (end < 0) break;
					sb.Append(text, i, end - i + 1);
					i = end;
					continue;
				}
				sb.Append(text[i]);
				visible++;
			}
			return sb.Append('…').Append("\x1b[0m").ToString();
		}
	}
}
