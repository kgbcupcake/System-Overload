using System.Text;
using Maries_Ui;
using Pastel;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// The bordered box along the bottom of a scene: two lines of text a page, with ▼ when there's more to
	/// read. Lines may carry colour (Pastel); padding counts only the visible characters.
	/// </summary>
	public sealed class StoryBox
	{
		/// <summary>Terminal rows the box takes: border, two lines, border.</summary>
		public const int Rows = 4;

		private readonly int _w, _top;

		public string FrameColour { get; init; } = "#FFD700";

		/// <param name="width">Terminal columns.</param>
		/// <param name="top">The terminal row of the box's top border.</param>
		public StoryBox(int width, int top)
		{
			_w = width;
			_top = top;
		}

		/// <summary>Columns a line of text can take (room is left for the border and the ▼).</summary>
		public int TextWidth => _w - 10;

		public void Draw(string[] lines, bool more)
		{
			int inner = _w - 4;
			string bar = "║".Pastel(FrameColour);
			var sb = new StringBuilder();
			sb.Append(Cup(_top, 1)).Append(("╔" + new string('═', inner) + "╗").Pastel(FrameColour));
			for (int r = 0; r < 2; r++)
			{
				string line = r < lines.Length ? lines[r] : "";
				int shown = UiEngine.StripAnsi(line).Length;
				sb.Append(Cup(_top + 1 + r, 1)).Append(bar).Append(' ')
				  .Append(line).Append(new string(' ', Math.Max(0, inner - 1 - shown))).Append(bar);
			}
			sb.Append(Cup(_top + 3, 1)).Append(("╚" + new string('═', inner) + "╝").Pastel(FrameColour));
			if (more) sb.Append(Cup(_top + 2, _w - 5)).Append("▼".Pastel(FrameColour));
			Console.Write(sb.ToString());
		}

		/// <summary>Text wrapped at word breaks to `width` columns.</summary>
		public static List<string> Wrap(string text, int width)
		{
			var lines = new List<string>();
			var line = new StringBuilder();
			foreach (string word in text.Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Length > 0 && line.Length + 1 + word.Length > width) { lines.Add(line.ToString()); line.Clear(); }
				if (line.Length > 0) line.Append(' ');
				line.Append(word);
			}
			if (line.Length > 0) lines.Add(line.ToString());
			return lines;
		}

		/// <summary>Lines grouped two to a page (at least one page, maybe empty).</summary>
		public static List<string[]> Pages(IReadOnlyList<string> lines)
		{
			var pages = new List<string[]>();
			for (int i = 0; i < lines.Count; i += 2) pages.Add(lines.Skip(i).Take(2).ToArray());
			if (pages.Count == 0) pages.Add(new[] { "" });
			return pages;
		}

		private static string Cup(int row, int col) => $"\x1b[{row + 1};{col + 1}H";
	}
}
