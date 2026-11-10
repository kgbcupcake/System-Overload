using Maries_Loaders;
using Maries_Ui;
using Pastel;
using System.Text;
using static System.Console;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>
	/// Plays a paged story scene, such as the prologue or the "A Life Before" memory.
	/// A scene with the prefix "prologue" reads its text from the story data keys
	/// "prologue.header", "prologue.title", "prologue.subtitle", "prologue.page.1", "prologue.page.2", ...
	/// and "prologue.final". Each page is typed out on its own screen; players can page forward and
	/// back, or skip the whole scene. The last page is shown alone and centered, like a closing line.
	/// </summary>
	internal static class StoryPages
	{
		// The screen is 94 wide; the text block is narrower so it reads like a page, not a log.
		private const int ScreenWidth = 94;
		private const int TextWidth = 70;
		private const int TextLeft = (ScreenWidth - TextWidth) / 2;
		private const int TextTop = 7;
		private const int TextBottom = 18;
		private const int ButtonRow = 21;
		private const int DotsRow = 22;
		private const int TypeDelayMs = 12;

		private const string GoldColor = "#FFAB00";
		private const string BrightGold = "#FFD700";
		private const string LeatherColor = "#8B5A2B";
		private const string BodyColor = "#CBB994";
		private const string DimColor = "#777777";

		private enum PageResult { Next, Back, Skip }

		/// <summary>Plays the scene. Returns when the player finishes or skips it.</summary>
		public static void Play(string prefix)
		{
			var pages = LoadPages(prefix);
			if (pages.Count == 0) return; // Nothing in the data for this scene: carry on without it.

			CursorVisible = false;
			int page = 0;
			int furthestSeen = -1;

			while (page < pages.Count)
			{
				// Pages the player has already read appear at once when they page back to them.
				bool typeOut = page > furthestSeen;
				furthestSeen = Math.Max(furthestSeen, page);

				var result = ShowPage(prefix, pages, page, typeOut);
				if (result == PageResult.Skip) break;
				if (result == PageResult.Back) page = Math.Max(0, page - 1);
				else page++;
			}

			MouseInput.ClearZones();
			ResetColor();
			Clear();
		}

		private static List<List<string>> LoadPages(string prefix)
		{
			var pages = new List<List<string>>();
			for (int i = 1; Story.Has($"{prefix}.page.{i}"); i++)
				pages.Add(Story.Lines($"{prefix}.page.{i}"));
			return pages;
		}

		private static PageResult ShowPage(string prefix, List<List<string>> pages, int index, bool typeOut)
		{
			bool isLast = index == pages.Count - 1;

			Clear();
			DrawHeader(prefix);
			DrawRule(TextTop - 2);
			DrawRule(TextBottom + 2);
			DrawPageDots(index, pages.Count);
			DrawButtons(prefix, isLast, index > 0);

			bool skipped = isLast
				? DrawFinalPage(prefix, pages[index], typeOut)
				: DrawBody(pages[index], typeOut);
			if (skipped) return PageResult.Skip;

			while (true)
			{
				switch (MouseInput.ReadKey().Key)
				{
					case ConsoleKey.Enter:
					case ConsoleKey.Spacebar:
					case ConsoleKey.RightArrow:
					case ConsoleKey.DownArrow:
						return PageResult.Next;
					case ConsoleKey.LeftArrow:
					case ConsoleKey.UpArrow:
					case ConsoleKey.Backspace:
						if (index > 0) return PageResult.Back;
						break;
					case ConsoleKey.S:
					case ConsoleKey.Escape:
						return PageResult.Skip;
				}
			}
		}

		private static void DrawHeader(string prefix)
		{
			// Header lines are optional, so a scene can have just a title.
			if (Story.Has($"{prefix}.header")) UiEngine.DrawCentered(Story.Line($"{prefix}.header").Pastel(DimColor), 2);
			if (Story.Has($"{prefix}.title")) UiEngine.DrawCentered(Story.Line($"{prefix}.title").Pastel(GoldColor), 3);
			if (Story.Has($"{prefix}.subtitle")) UiEngine.DrawCentered(Story.Line($"{prefix}.subtitle").Pastel(LeatherColor), 4);
		}

		private static void DrawRule(int row)
		{
			UiEngine.DrawCentered(new string('─', TextWidth).Pastel(LeatherColor), row);
		}

		private static void DrawPageDots(int index, int count)
		{
			var dots = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				if (i > 0) dots.Append(' ');
				dots.Append(i == index ? "●".Pastel(GoldColor) : "·".Pastel(DimColor));
			}
			UiEngine.DrawCentered(dots.ToString(), DotsRow);
		}

		/// <summary>Draws the clickable buttons and registers them as click zones.</summary>
		private static void DrawButtons(string prefix, bool isLast, bool canGoBack)
		{
			MouseInput.ClearZones();

			var buttons = new List<(string Label, ConsoleKey Key, string Color)>();
			if (canGoBack) buttons.Add(("[ BACK ]", ConsoleKey.LeftArrow, DimColor));
			buttons.Add((isLast ? "[ BEGIN ]" : "[ CONTINUE ]", ConsoleKey.Enter, BrightGold));
			string skip = Story.Has($"{prefix}.skip") ? Story.Line($"{prefix}.skip") : "SKIP";
			if (!isLast) buttons.Add(($"[ {skip} ]", ConsoleKey.S, DimColor));

			const int gap = 4;
			int totalWidth = buttons.Sum(b => b.Label.Length) + gap * (buttons.Count - 1);
			int x = (ScreenWidth - totalWidth) / 2;

			foreach (var (label, key, color) in buttons)
			{
				SetCursorPosition(x, ButtonRow);
				Write(label.Pastel(color));
				MouseInput.AddKeyZone(ButtonRow, x, label.Length, key);
				x += label.Length + gap;
			}
		}

		/// <summary>Draws a story page's paragraphs. Returns true if the player chose to skip.</summary>
		private static bool DrawBody(List<string> paragraphs, bool typeOut)
		{
			var rows = new List<string>();
			foreach (var paragraph in paragraphs)
			{
				if (rows.Count > 0) rows.Add("");
				rows.AddRange(Wrap(paragraph, TextWidth));
			}

			// Pages that run long drop their blank lines first, then get cut, so nothing spills
			// past the bottom rule.
			int maxRows = TextBottom - TextTop + 1;
			if (rows.Count > maxRows) rows.RemoveAll(r => r.Length == 0);
			if (rows.Count > maxRows) rows = rows.Take(maxRows).ToList();

			// Center the block vertically in the text area so short pages don't hug the top.
			int top = TextTop + (maxRows - rows.Count) / 2;
			bool instant = !typeOut;

			for (int i = 0; i < rows.Count; i++)
			{
				if (TypeLine(rows[i], TextLeft, top + i, BodyColor, ref instant)) return true;
			}
			return false;
		}

		/// <summary>The last page: the promise, alone and bright, then the closing line.</summary>
		private static bool DrawFinalPage(string prefix, List<string> lines, bool typeOut)
		{
			int middle = (TextTop + TextBottom) / 2;
			bool instant = !typeOut;

			int row = middle - lines.Count;
			foreach (var line in lines)
			{
				int x = (ScreenWidth - line.Length) / 2;
				if (TypeLine(line, x, row, BrightGold, ref instant)) return true;
				row += 2;
			}

			if (!instant) Thread.Sleep(600);

			if (Story.Has($"{prefix}.final"))
				UiEngine.DrawCentered(Story.Line($"{prefix}.final").Pastel(GoldColor), row + 1);
			return false;
		}

		/// <summary>
		/// Writes one line, a character at a time unless <paramref name="instant"/> is set.
		/// Any key or click while typing finishes the page at once; S or Esc skips the scene.
		/// Returns true if the player chose to skip.
		/// </summary>
		private static bool TypeLine(string text, int x, int y, string color, ref bool instant)
		{
			SetCursorPosition(x, y);
			if (instant || text.Length == 0)
			{
				Write(text.Pastel(color));
				return false;
			}

			for (int i = 0; i < text.Length; i++)
			{
				if (MouseInput.InputAvailable)
				{
					var input = MouseInput.Read();
					if (input.Key?.Key is ConsoleKey.S or ConsoleKey.Escape) return true;

					// Finish this line now; the rest of the page follows without the typing delay.
					instant = true;
					SetCursorPosition(x + i, y);
					Write(text[i..].Pastel(color));
					return false;
				}

				Write(text[i].ToString().Pastel(color));
				Thread.Sleep(TypeDelayMs);
			}
			return false;
		}

		/// <summary>Splits text into lines no wider than <paramref name="width"/>, breaking between words.</summary>
		internal static List<string> Wrap(string text, int width)
		{
			var lines = new List<string>();
			var line = new StringBuilder();

			foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Length > 0 && line.Length + 1 + word.Length > width)
				{
					lines.Add(line.ToString());
					line.Clear();
				}
				if (line.Length > 0) line.Append(' ');
				line.Append(word);
			}
			if (line.Length > 0) lines.Add(line.ToString());
			return lines;
		}
	}
}
