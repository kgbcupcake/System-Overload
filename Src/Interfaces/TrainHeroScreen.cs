using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using static System.Console;
using DungeonAdventures.Src.Village.Places;
using static DungeonAdventures.Src.Village.Places.ItemPanels;
using static DungeonAdventures.Src.Village.Places.PanelText;
using static DungeonAdventures.Src.Village.Places.ItemInfo;
using static DungeonAdventures.Src.Village.Places.ItemDetails;
using static DungeonAdventures.Src.Village.Places.PanelMenu;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>
	/// Character creation's attribute step, in the same two-panel look as the inventory: the four
	/// attributes with [-]/[+] and a pip bar on the left, and on the right what the selected one does
	/// (with its icon) and the hero's stats as they will be in combat.
	/// Keys: ↑↓ select, ←→ (or -/+) take back / spend, R reset, Enter done. Everything is clickable.
	/// </summary>
	internal static class TrainHeroScreen
	{
		public const int TotalPoints = 10;

		private sealed record Attribute(string Name, string Tagline, string PerPoint, string Color,
			Func<PlayerData.loadPlayer, int> Get, Action<PlayerData.loadPlayer, int> Set);

		private static readonly Attribute[] Attributes =
		{
			new("Vitality", "Max health", "+15 max HP", "#FF6060", p => p.VitalityPoints, (p, v) => p.VitalityPoints = v),
			new("Strength", "Damage", "+1 damage", "#FF9933", p => p.StrengthPoints, (p, v) => p.StrengthPoints = v),
			new("Fortitude", "Defense", "+2 defense", "#87CEEB", p => p.FortitudePoints, (p, v) => p.FortitudePoints = v),
			new("Wisdom", "Critical hits", "+1% crit chance", "#BC8FDF", p => p.WisdomPoints, (p, v) => p.WisdomPoints = v),
		};

		// Left panel rows (inside the box): intro text, then the attributes, then points and buttons.
		private const int FirstAttrRow = 3;
		private const int PointsRow = FirstAttrRow + 5;
		private const int DoneRow = PointsRow + 2;
		// Columns inside an attribute line: "> Name       [-] ■■■■■■■■■■ 10 [+]"
		private const int MinusCol = 13, BarCol = 17, PlusCol = 31;
		private const int BarWidth = TotalPoints;

		private const string HeaderColor = "#B8860B";

		public static void Run(PlayerData.loadPlayer player)
		{
			foreach (var a in Attributes) a.Set(player, 0);
			int selected = 0;
			string message = (Story.Has("creation.attributes_left") ? Story.Line("creation.attributes_left") : "Spend all your points, then choose Done.").Pastel(Muted);

			CursorVisible = false;
			Clear();
			using var mouse = MouseInput.Enable();

			while (true)
			{
				int left = TotalPoints - Attributes.Sum(a => a.Get(player));
				player.Health = (int)player.GetTotalMaxHealth();
				Draw(player, selected, left, message);
				message = "";

				var input = MouseInput.Read();
				ConsoleKey key = input.Key?.Key ?? ConsoleKey.NoName;
				string? command = null;
				if (input.Mouse is { } m)
				{
					if (m.Action == MouseAction.WheelUp) key = ConsoleKey.UpArrow;
					else if (m.Action == MouseAction.WheelDown) key = ConsoleKey.DownArrow;
					else if (m.Action == MouseAction.LeftClick) command = PanelHotspots.At(m.X, m.Y);
				}

				if (command != null)
				{
					var parts = command.Split(':');
					if (parts.Length == 2) selected = int.Parse(parts[1]);
					key = parts[0] switch
					{
						"minus" => ConsoleKey.LeftArrow,
						"plus" => ConsoleKey.RightArrow,
						"done" => ConsoleKey.Enter,
						"reset" => ConsoleKey.R,
						_ => ConsoleKey.NoName, // "row": selecting is all a click on the name does
					};
				}

				var attr = Attributes[selected];
				switch (key)
				{
					case ConsoleKey.UpArrow:
						selected = selected == 0 ? Attributes.Length - 1 : selected - 1;
						break;
					case ConsoleKey.DownArrow:
						selected = selected == Attributes.Length - 1 ? 0 : selected + 1;
						break;
					case ConsoleKey.RightArrow or ConsoleKey.OemPlus or ConsoleKey.Add:
						if (left > 0) attr.Set(player, attr.Get(player) + 1);
						else message = "No points left. Take one back from another attribute first.".Pastel(Amber);
						break;
					case ConsoleKey.LeftArrow or ConsoleKey.OemMinus or ConsoleKey.Subtract:
						if (attr.Get(player) > 0) attr.Set(player, attr.Get(player) - 1);
						break;
					case ConsoleKey.R:
						foreach (var a in Attributes) a.Set(player, 0);
						message = "Points reset.".Pastel(Muted);
						break;
					case ConsoleKey.Enter:
						if (left == 0)
						{
							player.Health = (int)player.GetTotalMaxHealth();
							Clear();
							return;
						}
						message = $"You still have {left} point{(left == 1 ? "" : "s")} to spend.".Pastel(Amber);
						break;
				}
			}
		}

		private static void Draw(PlayerData.loadPlayer player, int selected, int left, string message)
		{
			PanelHotspots.Clear();
			DrawHeader("TRAIN YOUR HERO", HeaderColor, player);
			string cls = $" {player.PlayerClass.ToUpper()} ";
			WriteAt(ScreenWidth - 1 - cls.Length, 0, cls.Pastel("#000000").PastelBg(Amber));

			// ---- left: the attributes
			int textX = ListX + 2;
			int Row(int r) => BoxTop + 1 + r;
			var lines = new List<string>();
			string intro = Story.Has("creation.attributes") ? Story.Line("creation.attributes") : "Spend your points to shape your hero.";
			foreach (var l in Wrap(intro, ListWidth - 4).Take(2)) lines.Add(l.Pastel(Flavour));
			while (lines.Count < FirstAttrRow) lines.Add("");

			for (int i = 0; i < Attributes.Length; i++)
			{
				var a = Attributes[i];
				int v = a.Get(player);
				bool sel = i == selected;
				string name = (sel ? "> " : "  ") + a.Name.PadRight(MinusCol - 2 - 1) + " ";
				string minus = "[-]".Pastel(v > 0 ? Text : "#444444");
				string plus = "[+]".Pastel(left > 0 ? Text : "#444444");
				string bar = new string('■', v).Pastel(a.Color) + new string('·', BarWidth - v).Pastel("#444444");
				string value = $"{v,2}".Pastel(v > 0 ? a.Color : Muted);
				lines.Add((sel ? name.Pastel("#000000").PastelBg(Gold) : name.Pastel(Text)) + minus + " " + bar + " " + value + " " + plus);

				PanelHotspots.Add(Row(lines.Count - 1), textX, MinusCol, $"row:{i}");
				PanelHotspots.Add(Row(lines.Count - 1), textX + MinusCol, 3, $"minus:{i}");
				PanelHotspots.Add(Row(lines.Count - 1), textX + BarCol, BarWidth + 3, $"row:{i}");
				PanelHotspots.Add(Row(lines.Count - 1), textX + PlusCol, 3, $"plus:{i}");
			}

			while (lines.Count < PointsRow) lines.Add("");
			int spent = TotalPoints - left;
			lines.Add($"{"Points left".PadRight(MinusCol + 4).Pastel(Dim)}" +
				new string('■', left).Pastel(Gold) + new string('·', spent).Pastel("#444444") +
				$" {left,2}".Pastel(left > 0 ? Gold : Muted));

			while (lines.Count < DoneRow) lines.Add("");
			string done = left == 0 ? " DONE ".Pastel("#000000").PastelBg(Gold) : " DONE ".Pastel("#666666").PastelBg("#2A2A2A");
			string reset = " RESET ".Pastel(Text).PastelBg("#3A2A1A");
			lines.Add(done + "   " + reset);
			PanelHotspots.Add(Row(lines.Count - 1), textX, 6, "done");
			PanelHotspots.Add(Row(lines.Count - 1), textX + 9, 7, "reset");
			if (left > 0)
				lines.Add($"Spend {left} more to finish.".Pastel(Muted));

			DrawBox(ListX, ListWidth, lines);

			// ---- right: the selected attribute and the hero it makes
			DrawDetail(AttributeDetail(player, Attributes[selected], left));

			// ---- footer
			var hero = EnemyFactory.CreatePlayer(player);
			string summary =
				$" {"Max HP".Pastel(Dim)} {((int)player.GetTotalMaxHealth()).ToString().Pastel(Good)}" +
				$"   {"Damage".Pastel(Dim)} {$"{hero.MinDamage}-{hero.MaxDamage}".Pastel(Bad)}" +
				$"   {"Defense".Pastel(Dim)} {player.ArmorValue.ToString().Pastel("#87CEEB")}" +
				$"   {"Crit".Pastel(Dim)} {$"{hero.CritChance}%".Pastel(Amber)}" +
				$"   {"Points left".Pastel(Dim)} {left.ToString().Pastel(left > 0 ? Gold : Muted)}";

			DrawFooter(summary, message, new List<(string, string?)>
			{
				(" [↑↓] Select", null),
				("   [←→] Take back / Spend", null),
				("   [R] Reset", "reset"),
				($"   [ENTER] Done", "done"),
			});
		}

		private static List<string> AttributeDetail(PlayerData.loadPlayer player, Attribute a, int left)
		{
			var d = new Detail(a.Name);
			d.Line(a.Name.ToUpper(), a.Color);
			d.Line(a.Tagline, Muted);
			string key = "attribute." + a.Name.ToLower();
			if (Story.Has(key)) d.Wrapped(Story.Line(key), Flavour);
			d.Blank();
			d.Stat("Each point", a.PerPoint, a.Color);
			d.Stat("Spent", $"{a.Get(player)} of {TotalPoints}", Text);
			d.Blank();

			// What this hero will walk into the dungeon with, and what the next point would change.
			var hero = EnemyFactory.CreatePlayer(player);
			int hp = (int)player.GetTotalMaxHealth();
			d.Add("Your hero".Pastel(Dim));
			d.Stat("Max HP", $"{hp}{Next(a, "Vitality", left, "+15")}", Good);
			d.Stat("Damage", $"{hero.MinDamage}-{hero.MaxDamage}{Next(a, "Strength", left, "+1")}", Bad);
			d.Stat("Defense", $"{player.ArmorValue}{Next(a, "Fortitude", left, "+2")}", "#87CEEB");
			d.Stat("Crit chance", $"{hero.CritChance}%{Next(a, "Wisdom", left, "+1%")}", Amber);
			return d.Build();
		}

		/// <summary>" (+15 next)" beside the stat the selected attribute raises, while points are left.</summary>
		private static string Next(Attribute selected, string stat, int left, string gain) =>
			selected.Name == stat && left > 0 ? $"  ({gain} next)" : "";
	}
}
