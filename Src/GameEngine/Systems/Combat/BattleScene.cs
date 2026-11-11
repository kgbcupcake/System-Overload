using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Lpc;
using Maries_Ui;
using Pastel;

namespace DungeonAdventures.Src.GameEngine.Combat
{
	/// <summary>
	/// The fight screen, laid out like a monster-battle game: the foe stands top-right facing you with its
	/// name and health top-left, the hero stands bottom-left with their back to you and their stats
	/// bottom-right, and a box along the bottom holds either the commands or what just happened.
	///
	///   row 0           title bar
	///   rows 1..A       arena (sprites and info), A = height - 5
	///   rows A+1..H-1   the box: border, two lines, border
	///
	/// Sprites come from each side's LPC look and are drawn once per fight; without the LPC catalog (or a
	/// look) the same layout is drawn without them.
	/// </summary>
	public sealed class BattleScene
	{
		private const int BoxRows = 4;

		private readonly Combatant _hero, _foe;
		private readonly int _w, _h, _arenaBottom, _spriteRows;
		private readonly string[]? _heroSprite, _foeSprite;

		/// <summary>True while the foe is still drawn (cleared when it falls).</summary>
		private bool _foeVisible = true;

		public BattleScene(Combatant hero, Combatant foe)
		{
			_hero = hero;
			_foe = foe;
			_w = Math.Max(80, SafeWidth());
			_h = Math.Max(20, SafeHeight());
			_arenaBottom = _h - BoxRows - 1;
			// The foe's sprite has to end above the hero's info (four rows at the bottom of the arena).
			_spriteRows = Math.Clamp(_arenaBottom - 5, 8, 18);
			_heroSprite = Sprite(hero, 0);
			_foeSprite = Sprite(foe, 2);
		}

		public int BoxTop => _h - BoxRows;
		public int Width => _w;

		private int SpriteWidth => HeroSprite.WidthFor(_spriteRows);
		private int FoeX => _w - 4 - SpriteWidth;
		private const int FoeY = 1;
		private const int HeroX = 6;
		private int HeroY => _arenaBottom - _spriteRows + 1;

		private string[]? Sprite(Combatant c, int dir)
		{
			if (c.Look == null || !HeroSprite.CanDraw(c.Look) || !PixelIcon.Enabled) return null;
			try { return HeroSprite.RenderSized(c.Look, dir, _spriteRows); }
			catch (Exception) { return null; } // a broken sheet shouldn't stop the fight
		}

		// ------------------------------------------------------------------ drawing

		/// <summary>Clears the screen and draws everything but the box's contents.</summary>
		public void Draw(int hitChance)
		{
			Console.Clear();
			HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "",
				GameState.CurrentPlayer?.Health ?? 0, (int)(GameState.CurrentPlayer?.GetTotalMaxHealth() ?? 0),
				GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);

			// A floor line under each fighter, so they stand on something.
			string ground = new string('▁', SpriteWidth + 6).Pastel("#3A3A3A");
			if (_foeSprite != null) At(FoeX - 3, FoeY + _spriteRows, ground);
			if (_heroSprite != null && HeroY + _spriteRows <= _arenaBottom) At(HeroX - 3, HeroY + _spriteRows, ground);

			if (_foeVisible) DrawSprite(_foeSprite, FoeX, FoeY);
			DrawSprite(_heroSprite, HeroX, HeroY);

			// Foe: top-left.
			string colour = _foe.Boss?.HexColor ?? "#FF4500";
			string tag = _foe.IsBoss ? (_foe.Boss!.Enraged ? "DUNGEON LORD // ENRAGED" : "DUNGEON LORD") : "FOE";
			At(3, 2, $"{tag.Pastel("#888888")}  {_foe.Name.ToUpper().Pastel(colour)}");
			At(3, 3, $"LVL {_foe.Level}");
			At(3, 4, HealthBar(_foe.Health, _foe.MaxHealth) + $" {_foe.Health}/{_foe.MaxHealth}".Pastel("#AAAAAA"));

			// Hero: bottom-right of the arena.
			int x = Math.Max(HeroX + SpriteWidth + 4, _w - 46);
			int y = _arenaBottom - 3;
			string defense = _hero.BonusDefense > 0 ? $"{_hero.Defense}+{_hero.BonusDefense}" : $"{_hero.Defense}";
			At(x, y, $"{"HERO".Pastel("#888888")}  {_hero.Name.ToUpper().Pastel("#FFD700")}   LVL {_hero.Level}");
			At(x, y + 1, HealthBar(_hero.Health, _hero.MaxHealth) + $" {_hero.Health}/{_hero.MaxHealth} HP".Pastel("#AAAAAA"));
			At(x, y + 2, $"DMG {_hero.MinDamage}-{_hero.MaxDamage}  DEF {defense}  CRIT {_hero.CritChance}%  HIT {hitChance}%".Pastel("#666666"));

			DrawBox("#FFD700");
		}

		private void DrawSprite(string[]? rows, int x, int y)
		{
			if (rows == null) return;
			for (int r = 0; r < rows.Length; r++) At(x, y + r, rows[r]);
		}

		private void BlankSprite(int x, int y, int fromRow = 0)
		{
			string blank = new string(' ', SpriteWidth);
			for (int r = fromRow; r < _spriteRows; r++) At(x, y + r, blank);
		}

		private void DrawBox(string colour)
		{
			int inner = _w - 4;
			At(1, BoxTop, ("╔" + new string('═', inner) + "╗").Pastel(colour));
			for (int r = 1; r < BoxRows - 1; r++)
				At(1, BoxTop + r, "║".Pastel(colour) + new string(' ', inner) + "║".Pastel(colour));
			At(1, BoxTop + BoxRows - 1, ("╚" + new string('═', inner) + "╝").Pastel(colour));
		}

		/// <summary>Writes a line inside the box (line 0 or 1).</summary>
		public void BoxLine(int line, int x, string text) => At(3 + x, BoxTop + 1 + line, text);

		// ------------------------------------------------------------------ the box

		/// <summary>
		/// Shows lines in the box two at a time, each page waiting for a key or click, like a battle log.
		/// Lines may carry colour; blank ones and leading "\n" are dropped.
		/// </summary>
		public void Say(int hitChance, params string[] lines)
		{
			var clean = lines
				.SelectMany(l => l.Split('\n'))
				.Select(l => l.TrimStart())
				.Where(l => UiEngine.StripAnsi(l).Trim().Length > 0)
				.ToList();
			for (int i = 0; i < clean.Count; i += 2)
			{
				Draw(hitChance);
				BoxLine(0, 0, clean[i]);
				if (i + 1 < clean.Count) BoxLine(1, 0, clean[i + 1]);
				At(_w - 5, BoxTop + 2, "▼".Pastel("#FFD700"));
				MouseInput.WaitForAnyInput();
			}
		}

		/// <summary>The four commands as a 2x2 grid on the right of the box; returns the chosen index or -1 on Escape.</summary>
		public int Command(int hitChance, string[] options, ref int selected)
		{
			const int ColW = 14;
			int left = _w - 4 - ColW * 2 - 2;
			while (true)
			{
				Draw(hitChance);
				BoxLine(0, 1, $"What will {_hero.Name.Pastel("#FFD700")} do?");
				MouseInput.ClearZones();
				for (int i = 0; i < options.Length; i++)
				{
					int col = left + (i % 2) * ColW, row = BoxTop + 1 + i / 2;
					string label = (i == selected ? "▶ " : "  ") + options[i];
					At(col, row, i == selected ? label.PadRight(ColW - 1).Pastel("#000000").PastelBg("#FFD700") : label.Pastel("#AAAAAA"));
					MouseInput.AddZone(row, col, ColW - 1, i);
				}

				var key = MouseInput.ReadMenuKey(ref selected, options.Length);
				switch (key)
				{
					case ConsoleKey.Enter: return selected;
					case ConsoleKey.LeftArrow: if (selected % 2 == 1) selected--; break;
					case ConsoleKey.RightArrow: if (selected % 2 == 0 && selected + 1 < options.Length) selected++; break;
					case ConsoleKey.UpArrow: selected = selected >= 2 ? selected - 2 : selected + 2 < options.Length ? selected + 2 : selected; break;
					case ConsoleKey.DownArrow: selected = selected + 2 < options.Length ? selected + 2 : selected >= 2 ? selected - 2 : selected; break;
				}
			}
		}

		/// <summary>
		/// A list in a box drawn over the right of the arena (the bag). Returns the index chosen or -1 when
		/// cancelled.
		/// </summary>
		public int Pick(int hitChance, string title, IReadOnlyList<string> lines, ref int selected)
		{
			int width = Math.Min(_w - 6, Math.Max(40, lines.Max(l => UiEngine.StripAnsi(l).Length) + 6));
			int x = _w - 2 - width;
			int shown = Math.Min(lines.Count, _arenaBottom - 3);
			int top = _arenaBottom - shown - 1;
			while (true)
			{
				Draw(hitChance);
				int first = Math.Clamp(selected - shown + 1, 0, Math.Max(0, lines.Count - shown));
				At(x, top, ("┌─ " + title + " " + new string('─', Math.Max(0, width - title.Length - 5)) + "┐").Pastel("#00FFFF"));
				MouseInput.ClearZones();
				for (int i = 0; i < shown; i++)
				{
					int n = first + i;
					string text = (n == selected ? "> " : "  ") + lines[n];
					text = text.Length > width - 2 ? text[..(width - 2)] : text.PadRight(width - 2);
					At(x, top + 1 + i, "│".Pastel("#00FFFF") + (n == selected ? text.Pastel("#000000").PastelBg("#00FFFF") : text) + "│".Pastel("#00FFFF"));
					MouseInput.AddZone(top + 1 + i, x + 1, width - 2, n);
				}
				At(x, top + 1 + shown, ("└" + new string('─', width - 2) + "┘").Pastel("#00FFFF"));

				BoxLine(0, 0, "Pick something to use.");
				BoxLine(1, 0, "[ESC] Cancel   [ENTER] Use".Pastel("#888888"));
				MouseInput.AddKeyZone(BoxTop + 2, 3, 12, ConsoleKey.Escape);
				MouseInput.AddKeyZone(BoxTop + 2, 18, 11, ConsoleKey.Enter);

				var key = MouseInput.ReadMenuKey(ref selected, lines.Count);
				if (key == ConsoleKey.Escape) return -1;
				if (key == ConsoleKey.Enter) return selected;
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
					selected = MenuNavigator.HandleVerticalNav(key, selected, lines.Count);
			}
		}

		// ------------------------------------------------------------------ effects

		/// <summary>The foe (or the hero) blinks when hit; a critical blinks longer.</summary>
		public void Flash(bool foe, bool critical)
		{
			var rows = foe ? _foeSprite : _heroSprite;
			if (rows == null || (foe && !_foeVisible)) return;
			int x = foe ? FoeX : HeroX, y = foe ? FoeY : HeroY;
			for (int n = 0; n < (critical ? 4 : 2); n++)
			{
				BlankSprite(x, y);
				Thread.Sleep(70);
				DrawSprite(rows, x, y);
				Thread.Sleep(70);
			}
		}

		/// <summary>The foe sinks out of sight, top row last.</summary>
		public void Faint()
		{
			if (_foeSprite == null || !_foeVisible) return;
			for (int shift = 1; shift <= _spriteRows; shift++)
			{
				BlankSprite(FoeX, FoeY);
				for (int r = 0; r < _spriteRows - shift; r++) At(FoeX, FoeY + shift + r, _foeSprite[r]);
				Thread.Sleep(25);
			}
			_foeVisible = false;
		}

		// ------------------------------------------------------------------ helpers

		private void At(int x, int y, string text)
		{
			if (x < 0 || y < 0 || y >= _h) return;
			try
			{
				Console.SetCursorPosition(x, y);
				Console.Write(text);
			}
			catch (ArgumentOutOfRangeException) { } // the window shrank mid-fight
		}

		private static string HealthBar(int current, int max)
		{
			const int width = 24;
			float pct = max > 0 ? (float)current / max : 0;
			int filled = Math.Clamp((int)Math.Ceiling(pct * width), 0, width);
			if (current <= 0) filled = 0;
			string colour = pct < 0.25f ? "#FF0000" : pct < 0.60f ? "#FFFF00" : "#00FF00";
			return "HP ".Pastel("#FFD700") + new string('█', filled).Pastel(colour) + new string('░', width - filled).Pastel("#333333");
		}

		private static int SafeWidth() { try { return Console.WindowWidth; } catch { return 94; } }
		private static int SafeHeight() { try { return Console.WindowHeight; } catch { return 24; } }
	}
}
