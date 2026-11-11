using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Lpc;
using Maries_DataModels;
using Maries_Ui;
using Pastel;
using static System.Console;
using DungeonAdventures.Src.Village.Places;
using static DungeonAdventures.Src.Village.Places.ItemPanels;
using static DungeonAdventures.Src.Village.Places.PanelText;
using static DungeonAdventures.Src.Village.Places.ItemInfo;
using static DungeonAdventures.Src.Village.Places.ItemDetails;
using static DungeonAdventures.Src.Village.Places.PanelMenu;
using Kind = DungeonAdventures.Src.Game.Interfaces.LookRowKind;
using Row = DungeonAdventures.Src.Game.Interfaces.LookRow;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>Draws the "Make your character" screen: the walking hero, the rows, the buttons and the hints.</summary>
	internal static class MakeCharacterView
	{
		public const string Place = "MAKE YOUR CHARACTER";
		public const string PlaceColor = "#7A5A10";
		public static readonly string[] Tabs = { "Look", "Clothes", "Tailor" };

		// Layout (94x24): header row 0; preview box x 1..40, rows 2..21; options box x 42..93, rows 2..18;
		// message rows 19-20 on the right; keys row 22.
		public const int PreviewX = 1, PreviewW = 40, PreviewTop = 2, PreviewH = 20;
		public const int OptX = 42, OptW = 52, OptTop = 2, OptH = 17;
		public const int MsgRow = 19, HintRow = 22;

		public const int LabelW = 14, ValueW = 22;

		/// <summary>The line inside the options box that row `index` sits on.</summary>
		public static int RowY(int index) => index;

		public static void Draw(PlayerData.loadPlayer player, HeroLook look, Row[] rows, int tab, int selected, int dir, int frame, string message)
		{
			PanelHotspots.Clear();
			DrawHeader(Place, PlaceColor, player);
			DrawTabs(Tabs, tab);

			// Preview frame, with TURN set into its bottom edge.
			DrawFrame(PreviewX, PreviewTop, PreviewW, PreviewH);
			string turn = " ◀  TURN  ▶ ";
			int turnX = PreviewX + (PreviewW - turn.Length) / 2;
			WriteAt(turnX, PreviewTop + PreviewH - 1, turn.Pastel(Gold));
			PanelHotspots.Add(PreviewTop + PreviewH - 1, turnX, 3, "turnl");
			PanelHotspots.Add(PreviewTop + PreviewH - 1, turnX + turn.Length - 3, 3, "turnr");
			DrawSprite(look, dir, frame);

			// Options
			DrawFrame(OptX, OptTop, OptW, OptH);
			int inner = OptW - 4;
			for (int i = 0; i < OptH - 2; i++)
				WriteAt(OptX + 2, OptTop + 1 + i, new string(' ', inner));

			for (int i = 0; i < rows.Length; i++)
			{
				var r = rows[i];
				int y = OptTop + 1 + RowY(i);
				bool sel = i == selected;
				string label = (sel ? "> " : "  ") + r.Title;
				string labelText = Fit(label, LabelW).PadRight(LabelW);
				string line = sel ? labelText.Pastel("#000000").PastelBg(Gold) : labelText.Pastel(Dim);
				PanelHotspots.Add(y, OptX + 2, LabelW, $"row:{i}");

				if (r.Kind == Kind.Name)
				{
					line += " " + Fit(player.PlayerName, ValueW + 4).Pastel(Text) + "  " + (sel ? "[Enter] rename".Pastel(Muted) : "");
					PanelHotspots.Add(y, OptX + 2 + LabelW, inner - LabelW, $"next:{i}");
				}
				else
				{
					var options = LookEditor.Options(look, r);
					string current = LookEditor.Current(look, r);
					int at = options.IndexOf(current);
					string value = r.Kind switch
					{
						Kind.Body => LpcCatalog.Words(current),
						Kind.Part => LpcCatalog.NameOf(current),
						Kind.Wardrobe => current == LookEditor.Tailored ? "Tailored" : Wardrobe.Get(current)?.Name ?? "None",
						_ => LpcCatalog.Words(current),
					};
					string swatch = r.Kind is Kind.Colour or Kind.PartColour
						? "██".Pastel(LpcPalette.Swatch(r.Kind == Kind.Colour ? r.Key : "cloth", current)) + " "
						: "";
					int room = ValueW - (swatch.Length > 0 ? 3 : 0);
					string shown = Fit(value, room);
					int padL = (room - shown.Length) / 2;
					string centre = swatch + new string(' ', padL) + shown.Pastel(sel ? Gold : Text) + new string(' ', room - padL - shown.Length);
					string count = options.Count > 1 ? $"{at + 1}/{options.Count}".PadLeft(5).Pastel(Muted) : "";
					line += " " + "◀".Pastel(Amber) + " " + centre + " " + "▶".Pastel(Amber) + " " + count;

					int arrowL = OptX + 2 + LabelW + 1;
					PanelHotspots.Add(y, arrowL, 2, $"prev:{i}");
					PanelHotspots.Add(y, arrowL + 2, ValueW, $"next:{i}");
					PanelHotspots.Add(y, arrowL + 2 + ValueW + 1, 2, $"next:{i}");
				}
				WriteAt(OptX + 2, y, Pad(ClipVisible(line, inner), inner));

			}

			// The Raven icon of the piece on the selected clothing row, between the buttons.
			if (rows[selected].Kind == Kind.Wardrobe && Wardrobe.Worn(look, rows[selected].Key) is { } piece
				&& Wardrobe.Icon(piece) is { } icon)
			{
				int iconX = OptX + (OptW - PixelIcon.Width(icon)) / 2;
				int iconY = OptTop + OptH - 1 - icon.Length;
				for (int r = 0; r < icon.Length; r++) WriteAt(iconX, iconY + r, icon[r]);
			}

			// Buttons at the foot of the options box.
			int by = OptTop + OptH - 2;
			string random = " RANDOM ".Pastel(Text).PastelBg("#3A2A1A");
			string done = " DONE ".Pastel("#000000").PastelBg(Gold);
			WriteAt(OptX + 2, by, random);
			PanelHotspots.Add(by, OptX + 2, 8, "random");
			WriteAt(OptX + OptW - 2 - 6, by, done);
			PanelHotspots.Add(by, OptX + OptW - 2 - 6, 6, "done");

			// Message and keys
			WriteAt(OptX, MsgRow, Pad(" " + message, OptW));
			WriteAt(OptX, MsgRow + 1, Pad(" " + Summary(look).Pastel(Muted), OptW));
			string hints = " [↑↓] Row   [←→] Change   [Tab] Look / Clothes / Tailor   [ and ] Turn   [R] Random   [Enter] Done";
			WriteAt(0, HintRow, Pad(hints.Pastel(Muted), ScreenWidth));
			HudChrome.DisplayFooter(GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);
		}

		public static string Summary(HeroLook look)
		{
			string outfit = Wardrobe.Worn(look, "outfit")?.Name ?? LpcCatalog.NameOf(LpcLook.Part(look, "clothes"));
			string cloak = Wardrobe.Worn(look, "cape") is { } c ? " · " + c.Name : "";
			return Fit($"{LpcCatalog.Words(look.BodyType)} · {LpcCatalog.NameOf(LpcLook.Part(look, "hair"))} hair · {outfit}{cloak}", OptW - 2);
		}

		/// <summary>Draws just the hero (the animation redraws this alone).</summary>
		public static void DrawSprite(HeroLook look, int dir, int frame) =>
			HeroSprite.Draw(look, PreviewX + (PreviewW - HeroSprite.Width) / 2, PreviewTop + 1, dir, frame);

		public static void DrawFrame(int x, int y, int w, int h)
		{
			WriteAt(x, y, ("╔" + new string('═', w - 2) + "╗").Pastel(Border));
			for (int r = 1; r < h - 1; r++)
			{
				WriteAt(x, y + r, "║".Pastel(Border));
				WriteAt(x + w - 1, y + r, "║".Pastel(Border));
			}
			WriteAt(x, y + h - 1, ("╚" + new string('═', w - 2) + "╝").Pastel(Border));
		}
	}
}
