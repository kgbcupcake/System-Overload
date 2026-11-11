using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;
using static DungeonAdventures.Src.Village.Places.ItemPanels;

namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>The panel screen's loop: draws the list and details, reads keys and clicks, returns what was picked.</summary>
	internal static class PanelMenu
	{
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

				PanelHotspots.Clear();
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
							string? command = PanelHotspots.At(m.X, m.Y);
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
	}
}
