using Pastel;
using System.Text.RegularExpressions;
using static System.Console;

namespace Maries_Ui
{
	public static class UiEngine
	{
		public const string BorderColor = "#8B0000"; // Dark Red for ominous borders
		public const int BoxWidth = 46;

		// --- SPICY THEME COLORS ---
		private const string DungeonPurple = "#2E0E4E";
		private const string TorchGold = "#FFAB00";
		private const string NeonGreen = "#00FF41"; // High-contrast Matrix green
		private const string DimGrey = "#777777";   // Severe retro grey
		private const string LockedRed = "#440000"; // Dark "locked" red

		public static string StripAnsi(string text)
		{
			if (string.IsNullOrEmpty(text)) return "";
			return Regex.Replace(text, @"\x1B\[[0-9;]*[a-zA-Z]", "");
		}
		
		public static string PadAnsiStringWithCenter(string text, int totalWidth)
		{
			if (string.IsNullOrEmpty(text)) return new string(' ', totalWidth);
			int cleanLength = UiEngine.StripAnsi(text).Length;
			int padding = (totalWidth - cleanLength) / 2;
			string leftPad = new string(' ', padding > 0 ? padding : 0);
			string rightPad = new string(' ', totalWidth - (padding + cleanLength) > 0 ? totalWidth - (padding + cleanLength) : 0);
			return leftPad + text + rightPad;
		}

		public static void DrawCentered(string text)
		{
			string cleanText = StripAnsi(text);
			int centerX = (WindowWidth / 2) - (cleanText.Length / 2);
			if (centerX < 0) centerX = 0;
			SetCursorPosition(centerX, CursorTop);
			WriteLine(text);
		}

		public static void DrawCentered(string text, int y)
		{
			int screenWidth = 94; // Match your ConsoleSize() anchor

			// We must use StripAnsi because Pastel color codes have 0 visual width 
			// but count as characters in string.Length
			int cleanLength = StripAnsi(text).Length;
			int x = (screenWidth / 2) - (cleanLength / 2);

			if (x < 0) x = 0; // Boundary safety

			SetCursorPosition(x, y);
			Write(text);
		}

		public static void DrawCentered(string text, int row, int clearWidth = 50)
		{
			string clean = StripAnsi(text);
			int startPos = WindowWidth / 2 - clean.Length / 2;
			int clearStart = WindowWidth / 2 - clearWidth / 2;

			SetCursorPosition(Math.Max(0, clearStart), row);
			Write(new string(' ', clearWidth));

			SetCursorPosition(Math.Max(0, startPos), row);
			Write(text);
		}

		public static void DrawLoadingScreen(string taskName, int durationMs)
		{
			CursorVisible = false;
			int width = 40;
			int centerX = WindowWidth / 2;
			int centerY = WindowHeight / 2;

			for (int i = 0; i <= 100; i += 10)
			{
				Clear();
				DrawCentered(taskName.Pastel(TorchGold), centerY - 1);

				SetCursorPosition(centerX - (width / 2), centerY + 1);
				string bar = new string('█', (i * width) / 100);
				string empty = new string('░', width - ((i * width) / 100));
				Write(bar.Pastel(DungeonPurple) + empty.Pastel("#333333"));

				Thread.Sleep(durationMs / 10);
			}
			Clear();
		}

		public static void DrawBoxLine(string content, int row, int left)
		{
			SetCursorPosition(left, row);
			Write("║".Pastel(BorderColor));
			SetCursorPosition(left + 1, row);
			Write(new string(' ', BoxWidth - 2));
			string cleanContent = StripAnsi(content);
			int contentStart = left + 1 + (BoxWidth - 2) / 2 - cleanContent.Length / 2;
			SetCursorPosition(contentStart, row);
			Write(content);
			SetCursorPosition(left + BoxWidth - 1, row);
			Write("║".Pastel(BorderColor));
		}

		public static void DrawBoxBorder(int row, int left, bool isTop)
		{
			SetCursorPosition(left, row);
			string leftCap = isTop ? "╔" : "╚";
			string rightCap = isTop ? "╗" : "╝";
			Write((leftCap + new string('═', BoxWidth - 2) + rightCap).Pastel(BorderColor));
		}

		public static bool GetArrowChoice(string prompt, string option1, string option2)
		{
			int selected = 0;
			while (true)
			{
				Clear();
				WriteLine($"\n {prompt.Pastel("#FFD700")}\n");
				// The prompt takes rows 0-2, so the options are on rows 3 and 4.
				MouseInput.ClearZones();
				MouseInput.AddZone(3, 0, 40, 0);
				WriteLine($" {(selected == 0 ? " > ".Pastel("#00FF00") : "    ")} {option1}");
				MouseInput.AddZone(4, 0, 40, 1);
				WriteLine($" {(selected == 1 ? " > ".Pastel("#00FF00") : "    ")} {option2}");
				var key = MouseInput.ReadMenuKey(ref selected, 2);
				if (key == ConsoleKey.UpArrow) selected = 0;
				else if (key == ConsoleKey.DownArrow) selected = 1;
				else if (key == ConsoleKey.Enter) return selected == 0;
			}
		}
	}
}