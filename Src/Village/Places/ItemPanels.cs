using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;
using static DungeonAdventures.Src.Village.Places.PanelText;

namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>One row of a panel list: a name on the left, an optional tag before it and a note on the right.</summary>
	internal sealed record PanelRow(string Name, string Color = "#DCDCDC", string Right = "", string RightColor = ItemPanels.Dim, string Tag = "");

	/// <summary>
	/// The two-panel look shared by the inventory, the General Store and the Blacksmith: a list on
	/// the left, details of the selected entry (with its icon) on the right, a summary bar, a message
	/// line and clickable key hints underneath. This class holds the layout, the colours and the frame
	/// drawing; <see cref="PanelMenu"/> runs the screen, <see cref="ItemDetails"/> fills the right panel.
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
				PanelHotspots.Add(0, tabX, label.Length, $"tab:{t}");
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
				PanelHotspots.Add(BoxTop + 1 + r, ListX, ListWidth, $"row:{idx}");
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
					PanelHotspots.Add(KeysRow, hintX + lead, text.Length - lead, command);
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
	}
}
