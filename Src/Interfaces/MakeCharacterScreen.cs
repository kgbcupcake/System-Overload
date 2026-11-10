using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Lpc;
using Maries_DataModels;
using Maries_Ui;
using Pastel;
using static System.Console;
using static DungeonAdventures.Src.Village.Places.ItemPanels;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>
	/// "Make your character": the hero drawn from the Universal LPC catalog (assets/LPC) on the left,
	/// walking in place, with TURN under it; LOOK and CLOTHES tabs of ◀ choice ▶ rows on the right
	/// (body, head, face, hair, beard, wings, skin, hair and eye colour; top, legs, shoes and their
	/// colours). Modelled on MariesRPG's creator. DONE saves the look on the hero.
	/// Keys: ↑↓ row, ←→ change, Tab LOOK/CLOTHES, [ ] turn, R random, Enter done (or edit the name).
	/// Everything is clickable.
	/// </summary>
	internal static class MakeCharacterScreen
	{
		private const string Place = "MAKE YOUR CHARACTER";
		private const string PlaceColor = "#7A5A10";
		private static readonly string[] Tabs = { "Look", "Clothes" };

		// Layout (94x24): header row 0; preview box x 1..40, rows 2..21; options box x 42..93, rows 2..18;
		// message rows 19-20 on the right; keys row 22.
		private const int PreviewX = 1, PreviewW = 40, PreviewTop = 2, PreviewH = 20;
		private const int OptX = 42, OptW = 52, OptTop = 2, OptH = 17;
		private const int MsgRow = 19, HintRow = 22;

		// The part of a 64 px frame that is drawn (the character with wings), and the size it's shrunk to.
		private const int CropX = 6, CropY = 8, CropW = 52, CropH = 54;
		private const int SpriteW = 35, SpriteH = 36;

		private const int LabelW = 14, ValueW = 22;

		private enum Kind { Name, Body, Part, Colour, PartColour }

		/// <summary>One ◀ value ▶ row: a catalog part type, a palette material, or the name/body.</summary>
		private sealed record Row(string Title, Kind Kind, string Key, bool Optional = true);

		private static readonly Row[] LookRows =
		{
			new("NAME", Kind.Name, ""),
			new("BODY", Kind.Body, ""),
			new("HEAD", Kind.Part, "head", Optional: false),
			new("FACE", Kind.Part, "expression"),
			new("HAIR", Kind.Part, "hair"),
			new("BEARD", Kind.Part, "beard"),
			new("WINGS", Kind.Part, "wings"),
			new("SKIN", Kind.Colour, "body"),
			new("HAIR COLOUR", Kind.Colour, "hair"),
			new("EYES", Kind.Colour, "eye"),
		};

		private static readonly Row[] ClothesRows =
		{
			new("TOP", Kind.Part, "clothes"),
			new("LEGS", Kind.Part, "legs"),
			new("SHOES", Kind.Part, "shoes"),
			new("SHIRT", Kind.PartColour, "clothes"),
			new("PANTS", Kind.PartColour, "legs"),
			new("SHOE COLOUR", Kind.PartColour, "shoes"),
		};

		private static readonly string[] DirNames = { "up", "left", "down", "right" };

		/// <summary>The skin, hair and eye ramps RANDOM picks from.</summary>
		private static readonly Dictionary<string, string[]> NaturalColours = new()
		{
			["body"] = new[] { "light", "amber", "olive", "taupe", "bronze", "brown", "black" },
			["hair"] = new[] { "ash", "platinum", "white", "gray", "blonde", "sandy", "strawberry", "gold", "ginger", "carrot",
				"redhead", "red", "light_brown", "chestnut", "dark_brown", "dark_gray", "black", "raven" },
			["eye"] = new[] { "blue", "green", "brown", "gray" },
		};
		private static readonly Random Rng = new();

		/// <summary>Runs the screen and stores the look on the hero. Does nothing when the LPC catalog isn't on disk.</summary>
		public static void Run(PlayerData.loadPlayer player)
		{
			if (!LpcMeta.Available) return;

			var look = player.Look ?? LpcLook.Default();
			int tab = 0, row = 1, dir = 2, frame = 0; // start on BODY: Enter on NAME renames
			string message = "Pick a look, then choose DONE.".Pastel(Muted);

			CursorVisible = false;
			Clear();
			using var mouse = MouseInput.Enable();
			Tidy(look);

			while (true)
			{
				var rows = tab == 0 ? VisibleLookRows(look) : ClothesRows;
				row = Math.Clamp(row, 0, rows.Length - 1);
				Draw(player, look, rows, tab, row, dir, frame, message);
				message = "";

				// The hero walks in place until a key or click arrives.
				while (!MouseInput.InputAvailable)
				{
					Thread.Sleep(110);
					frame = (frame + 1) % 8;
					DrawSprite(look, dir, frame);
				}

				var input = MouseInput.Read();
				ConsoleKey key = input.Key?.Key ?? ConsoleKey.NoName;
				int step = 0;

				if (input.Mouse is { } m)
				{
					if (m.Action == MouseAction.WheelUp) key = ConsoleKey.UpArrow;
					else if (m.Action == MouseAction.WheelDown) key = ConsoleKey.DownArrow;
					else if (m.Action == MouseAction.LeftClick && HitTest(m.X, m.Y) is { } command)
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
					case ConsoleKey.Tab: tab = 1 - tab; row = 0; break;
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

		// ---------------------------------------------------------------- choices

		private static Row[] VisibleLookRows(HeroLook look) =>
			LookRows.Where(r => !(r.Key == "beard" && look.BodyType != "male")
				&& !(r.Key == "expression" && !FacesFit(look))).ToArray();

		/// <summary>Faces are drawn for human heads only.</summary>
		private static bool FacesFit(HeroLook look)
		{
			var sample = LpcCatalog.Of("expression", look.BodyType).FirstOrDefault();
			return sample != null && sample.Fits(LpcLook.Item(look, "head"));
		}

		/// <summary>The choices a row offers: catalog item ids (with "none" first when optional) or ramp names.</summary>
		private static List<string> Options(HeroLook look, Row row) => row.Kind switch
		{
			Kind.Body => LpcMeta.BodyTypes.ToList(),
			Kind.Part => (row.Optional ? new List<string> { LpcMeta.None } : new List<string>())
				.Concat(LpcCatalog.Of(row.Key, look.BodyType).Select(i => i.Id)).ToList(),
			Kind.Colour => LpcPalette.Names(row.Key),
			Kind.PartColour => LpcPalette.Names("cloth"),
			_ => new List<string>(),
		};

		private static string Current(HeroLook look, Row row) => row.Kind switch
		{
			Kind.Body => look.BodyType,
			Kind.Part => LpcLook.Part(look, row.Key),
			Kind.Colour => LpcLook.ColourOf(look, row.Key),
			Kind.PartColour => LpcLook.ColourOf(look, row.Key, "cloth"),
			_ => "",
		};

		/// <summary>Moves a row one choice back or on, and keeps the rest of the look valid.</summary>
		private static string Change(HeroLook look, Row row, int step)
		{
			var options = Options(look, row);
			if (options.Count == 0) return "Nothing to choose here.".Pastel(Muted);
			int i = options.IndexOf(Current(look, row));
			string pick = options[((i < 0 ? 0 : i + step) % options.Count + options.Count) % options.Count];
			Set(look, row, pick);
			Tidy(look);
			return "";
		}

		private static void Set(HeroLook look, Row row, string value)
		{
			switch (row.Kind)
			{
				case Kind.Body:
					look.BodyType = value;
					break;
				case Kind.Part:
					look.Parts[row.Key] = value;
					break;
				case Kind.Colour:
					look.Colours[row.Key] = value;
					break;
				case Kind.PartColour:
					if (!look.PartColours.TryGetValue(row.Key, out var own)) look.PartColours[row.Key] = own = new();
					own["cloth"] = value;
					break;
			}
		}

		/// <summary>
		/// After a change: a human head follows the body type, parts with no art for the body are swapped for
		/// the first that has, a woman has no beard, and a non-human head has no face.
		/// </summary>
		private static void Tidy(HeroLook look)
		{
			string head = LpcLook.Part(look, "head");
			if (head.StartsWith("heads_human_") && (head == "heads_human_female" || head == "heads_human_male"))
				look.Parts["head"] = "heads_human_" + look.BodyType;
			foreach (var row in LookRows.Concat(ClothesRows).Where(r => r.Kind == Kind.Part))
			{
				var available = LpcCatalog.Of(row.Key, look.BodyType);
				string part = LpcLook.Part(look, row.Key);
				if (part == LpcMeta.None && row.Optional) continue;
				if (available.All(i => i.Id != part))
					look.Parts[row.Key] = row.Optional ? LpcMeta.None : available.FirstOrDefault()?.Id ?? LpcMeta.None;
			}
			if (look.BodyType != "male") look.Parts["beard"] = LpcMeta.None;
			if (!FacesFit(look)) look.Parts["expression"] = LpcMeta.None;
			else if (LpcLook.Part(look, "expression") == LpcMeta.None && !look.Parts.ContainsKey("expression"))
				look.Parts["expression"] = "face_neutral";
		}

		/// <summary>A new random look: head, face, hair, beard, clothes and every colour. Wings stay a choice.</summary>
		private static void Randomise(HeroLook look)
		{
			T Pick<T>(IList<T> list) => list[Rng.Next(list.Count)];
			foreach (var row in LookRows.Concat(ClothesRows))
			{
				if (row.Kind is Kind.Name or Kind.Body || row.Key == "wings") continue;
				var options = Options(look, row);
				if (row.Kind == Kind.Part && row.Optional && row.Key is "clothes" or "legs" or "shoes" or "hair")
					options.Remove(LpcMeta.None); // a random hero is never bald and never naked
				// Random rolls an ordinary person; fur, green skin, blue hair and the like are there to pick by hand.
				if (row.Key == "head")
					options = options.Where(o => o.StartsWith("heads_human_" + look.BodyType) && !o.Contains("elderly")).ToList();
				if (row.Kind == Kind.Colour && NaturalColours.TryGetValue(row.Key, out var natural))
					options = options.Where(natural.Contains).ToList();
				if (options.Count > 0) Set(look, row, Pick(options));
			}
			if (Rng.Next(3) > 0) look.Parts["beard"] = LpcMeta.None;
			Tidy(look);
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

		// ---------------------------------------------------------------- drawing

		/// <summary>The line inside the options box that row `index` sits on.</summary>
		private static int RowY(int index) => index;

		private static void Draw(PlayerData.loadPlayer player, HeroLook look, Row[] rows, int tab, int selected, int dir, int frame, string message)
		{
			ClearHotspots();
			DrawHeader(Place, PlaceColor, player);
			DrawTabs(Tabs, tab);

			// Preview frame, with TURN set into its bottom edge.
			DrawFrame(PreviewX, PreviewTop, PreviewW, PreviewH);
			string turn = " ◀  TURN  ▶ ";
			int turnX = PreviewX + (PreviewW - turn.Length) / 2;
			WriteAt(turnX, PreviewTop + PreviewH - 1, turn.Pastel(Gold));
			AddHotspot(PreviewTop + PreviewH - 1, turnX, 3, "turnl");
			AddHotspot(PreviewTop + PreviewH - 1, turnX + turn.Length - 3, 3, "turnr");
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
				AddHotspot(y, OptX + 2, LabelW, $"row:{i}");

				if (r.Kind == Kind.Name)
				{
					line += " " + Fit(player.PlayerName, ValueW + 4).Pastel(Text) + "  " + (sel ? "[Enter] rename".Pastel(Muted) : "");
					AddHotspot(y, OptX + 2 + LabelW, inner - LabelW, $"next:{i}");
				}
				else
				{
					var options = Options(look, r);
					string current = Current(look, r);
					int at = options.IndexOf(current);
					string value = r.Kind switch
					{
						Kind.Body => LpcCatalog.Words(current),
						Kind.Part => LpcCatalog.NameOf(current),
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
					AddHotspot(y, arrowL, 2, $"prev:{i}");
					AddHotspot(y, arrowL + 2, ValueW, $"next:{i}");
					AddHotspot(y, arrowL + 2 + ValueW + 1, 2, $"next:{i}");
				}
				WriteAt(OptX + 2, y, Pad(ClipVisible(line, inner), inner));

			}

			// Buttons at the foot of the options box.
			int by = OptTop + OptH - 2;
			string random = " RANDOM ".Pastel(Text).PastelBg("#3A2A1A");
			string done = " DONE ".Pastel("#000000").PastelBg(Gold);
			WriteAt(OptX + 2, by, random);
			AddHotspot(by, OptX + 2, 8, "random");
			WriteAt(OptX + OptW - 2 - 6, by, done);
			AddHotspot(by, OptX + OptW - 2 - 6, 6, "done");

			// Message and keys
			WriteAt(OptX, MsgRow, Pad(" " + message, OptW));
			WriteAt(OptX, MsgRow + 1, Pad(" " + Summary(look).Pastel(Muted), OptW));
			string hints = " [↑↓] Row   [←→] Change   [Tab] Look / Clothes   [ and ] Turn   [R] Random   [Enter] Done";
			WriteAt(0, HintRow, Pad(hints.Pastel(Muted), ScreenWidth));
			HudChrome.DisplayFooter(GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);
		}

		private static string Summary(HeroLook look) =>
			Fit($"{LpcCatalog.Words(look.BodyType)} · {LpcCatalog.NameOf(LpcLook.Part(look, "hair"))} hair · {LpcCatalog.NameOf(LpcLook.Part(look, "clothes"))}", OptW - 2);

		/// <summary>Draws just the hero (the animation redraws this alone).</summary>
		private static void DrawSprite(HeroLook look, int dir, int frame)
		{
			var full = LpcComposer.Frame(look, "walk", DirNames[dir], 1 + frame);
			var small = LpcComposer.Shrink(full, CropX, CropY, CropW, CropH, SpriteW, SpriteH);
			var rows = PixelIcon.Render(small, SpriteW, SpriteH);
			int x = PreviewX + (PreviewW - SpriteW) / 2;
			int y = PreviewTop + 1;
			for (int r = 0; r < rows.Length && r < PreviewH - 2; r++)
				WriteAt(x, y + r, rows[r]);
		}

		private static void DrawFrame(int x, int y, int w, int h)
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
