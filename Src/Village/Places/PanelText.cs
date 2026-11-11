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
	/// <summary>Text helpers for panel screens: stat lines, padding and cutting coloured text, word wrap.</summary>
	internal static class PanelText
	{
		public static string Stat(string label, string value, string color) =>
			$"{label.PadRight(12).Pastel(Dim)}{value.Pastel(color)}";

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
