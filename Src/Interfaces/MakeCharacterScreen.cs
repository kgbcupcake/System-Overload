using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Lpc;
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
using static DungeonAdventures.Src.Game.Interfaces.MakeCharacterView;
using static DungeonAdventures.Src.Game.Interfaces.LookEditor;
using Kind = DungeonAdventures.Src.Game.Interfaces.LookRowKind;
using Row = DungeonAdventures.Src.Game.Interfaces.LookRow;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>
	/// "Make your character": the hero drawn from the Universal LPC catalog (assets/LPC) on the left,
	/// walking in place, with TURN under it; LOOK and CLOTHES tabs of ◀ choice ▶ rows on the right
	/// (body, head, face, hair, beard, wings, skin, hair and eye colour), CLOTHES (the Raven clothing pack's
	/// outfits, cloaks, boots, gloves and belts, each drawn on the hero through its LPC layer, with the
	/// piece's icon shown; see <see cref="Wardrobe"/>) and TAILOR (the raw LPC top, legs, shoes and their
	/// colours). Modelled on MariesRPG's creator. DONE saves the look on the hero.
	/// Keys: ↑↓ row, ←→ change, Tab LOOK/CLOTHES/TAILOR, [ ] turn, R random, Enter done (or edit the name).
	/// Everything is clickable.
	/// </summary>
	internal static class MakeCharacterScreen
	{
		/// <summary>Runs the screen and stores the look on the hero. Does nothing when the LPC catalog isn't on disk.</summary>
		public static void Run(PlayerData.loadPlayer player)
		{
			if (!LpcMeta.Available) return;

			// The race decides which heads, ears and skins are on offer (none for heroes made before races).
			RaceLook.Active = OriginLoader.Race(player.Race);
			var look = player.Look ?? LpcLook.Default();
			int tab = 0, row = 1, dir = 2, frame = 0; // start on BODY: Enter on NAME renames
			string message = "Pick a look, then choose DONE.".Pastel(Muted);

			CursorVisible = false;
			Clear();
			using var mouse = MouseInput.Enable();
			Tidy(look);

			while (true)
			{
				var rows = LookRows.RowsOf(tab, look);
				row = Math.Clamp(row, 0, rows.Length - 1);
				Draw(player, look, rows, tab, row, dir, frame, message);
				message = "";

				// The hero walks in place until a key or click arrives.
				while (!MouseInput.InputAvailable)
				{
					Thread.Sleep(110);
					frame = (frame + 1) % HeroSprite.Frames;
					DrawSprite(look, dir, frame);
				}

				var input = MouseInput.Read();
				ConsoleKey key = input.Key?.Key ?? ConsoleKey.NoName;
				int step = 0;

				if (input.Mouse is { } m)
				{
					if (m.Action == MouseAction.WheelUp) key = ConsoleKey.UpArrow;
					else if (m.Action == MouseAction.WheelDown) key = ConsoleKey.DownArrow;
					else if (m.Action == MouseAction.LeftClick && PanelHotspots.At(m.X, m.Y) is { } command)
					{
						var parts = command.Split(':');
						switch (parts[0])
						{
							case "tab": tab = int.Parse(parts[1]); row = 0; continue;
							case "row": row = int.Parse(parts[1]); break;
							case "prev": row = int.Parse(parts[1]); step = -1; break;
							case "next": row = int.Parse(parts[1]); step = 1; break;
							case "turnl": dir = Turn(dir, -1); continue;
							case "turnr": dir = Turn(dir, 1); continue;
							case "random": key = ConsoleKey.R; break;
							case "done": key = ConsoleKey.Enter; break;
						}
					}
				}

				// [ and ] turn the hero (matched by character: their ConsoleKey differs between keyboards).
				char ch = input.Key?.KeyChar ?? '\0';
				if (ch == '[') { dir = Turn(dir, -1); continue; }
				if (ch == ']') { dir = Turn(dir, 1); continue; }

				switch (key)
				{
					case ConsoleKey.UpArrow: row = row == 0 ? rows.Length - 1 : row - 1; break;
					case ConsoleKey.DownArrow: row = row == rows.Length - 1 ? 0 : row + 1; break;
					case ConsoleKey.LeftArrow: step = -1; break;
					case ConsoleKey.RightArrow: step = 1; break;
					case ConsoleKey.Tab: tab = (tab + 1) % Tabs.Length; row = 0; break;
					case ConsoleKey.R:
						Randomise(look);
						message = "Rolled a new look.".Pastel(Muted);
						break;
					case ConsoleKey.Escape:
						message = "Choose DONE to keep this look.".Pastel(Amber);
						break;
					case ConsoleKey.Enter:
						if (rows[row].Kind == Kind.Name && input.Mouse == null)
						{
							EditName(player, row);
							break;
						}
						player.Look = look;
						RaceLook.Active = null;
						LpcComposer.ReleaseAll();
						Clear();
						return;
				}

				if (step != 0)
				{
					var r = rows[row];
					if (r.Kind == Kind.Name) EditName(player, row);
					else message = Change(look, r, step);
				}
			}
		}

		private static void EditName(PlayerData.loadPlayer player, int row)
		{
			int y = OptTop + 1 + RowY(row);
			int x = OptX + 2 + LabelW;
			WriteAt(x, y, new string(' ', OptW - 4 - LabelW));
			SetCursorPosition(x, y);
			CursorVisible = true;
			string? name;
			using (MouseInput.Suspend())
				name = ReadLine();
			CursorVisible = false;
			if (!string.IsNullOrWhiteSpace(name)) player.PlayerName = name.Trim();
			Clear();
		}

		private static int Turn(int dir, int step) => (dir + step + 4) % 4;
	}
}
