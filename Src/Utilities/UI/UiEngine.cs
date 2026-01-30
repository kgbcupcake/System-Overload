using System_Overload.Src.GameData.Components;
using Pastel;
using Src.GameData.Components;
using System.Text.RegularExpressions;
using static System_Overload.Src.GameData.Entities.PlayerData;
using static System.Console;

namespace System_Overload.Src.Utilities.UI
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

		public static void DrawDynamicFrame(string title, List<string> lines, string hint = "", int boxWidth = 66, int startY = 11, int selectedIndex = -1, int previewHp = -1, int previewCoins = -1)
		{
			CursorVisible = false;

			// 1. Unified Setup - Using 94 Anchor
			int screenWidth = 94;
			int startX = (screenWidth / 2) - (boxWidth / 2);
			// startY is already a parameter, we use the value passed in (11)
			string borderColor = BorderColor;

			// 2. Top Border Logic
			SetCursorPosition(startX, startY);
			Write($"╔{new string('═', boxWidth - 2)}╗".Pastel(borderColor));

			// 3. The Hover-Aware Loop
			for (int i = 0; i < lines.Count; i++)
			{
				SetCursorPosition(startX, startY + 1 + i);
				string currentLine = lines[i];

				// Apply Highlight Color based on index
				if (i == selectedIndex)
				{
					currentLine = currentLine.Pastel("#00FF00"); // Hover Green
				}
				else
				{
					currentLine = currentLine.Pastel("#555555"); // Idle Gray
				}

				// Write line with borders
				Write($"║{currentLine}║".Pastel(borderColor));
			}

			// 4. Bottom Border
			SetCursorPosition(startX, startY + lines.Count + 1);
			Write($"╚{new string('═', boxWidth - 2)}╝".Pastel(borderColor));

			// 5. Hint Logic
			if (!string.IsNullOrEmpty(hint))
			{
				DrawCentered(hint.Pastel("#555555"), startY + lines.Count + 3);
			}

			ResetColor();

			// --- FINAL FOOTER ---
			UiFunctions.DisplayFooter();
		}

		//public static void DrawCenteredBoxLine(string content, int row, int x, int width, string borderColor)
		//{
		//	SetCursorPosition(x, row);
		//	Write($"{"║".Pastel(borderColor)}{content}{"║".Pastel(borderColor)}");
		//}


		public static void DrawAttributeBox(loadPlayer player, int x, int y, int selectedStat = -1)
		{
			string bColor = BorderColor; // Reusing your existing BorderColor variable

			// Header themed for Neural Simulation
			string top = "╔══════════════ SYSTEM SPECS ════════════╗";
			string mid = "╠════════════════════════════════════════╣";
			string bottom = "╚════════════════════════════════════════╝";

			SetCursorPosition(x, y);
			WriteLine(top.Pastel(bColor));

			// Pulling real data from your new WeaponData and Player logic
			var weapon = player.Inventory.OfType<WeaponData>().FirstOrDefault();
			string weaponName = weapon != null ? weapon.Name : "No Link Detected";
			float totalDmg = player.GetTotalDamage();
			int socketsUsed = weapon?.SocketedGems.Count ?? 0;
			int totalSockets = weapon?.NumSockets ?? 0;

			// Displaying Hardware/Software stats instead of fantasy Abilities
			DrawStatLine("Link Stability", player.Health, x, y + 1, false, "#00FF00", "#FFFFFF", bColor);
			DrawHardwareLine("Hardware", weaponName, x, y + 2, "#FFD700", bColor);
			DrawStatLine("Damage Rating", (int)totalDmg, x, y + 3, false, "#FF4500", "#FFFFFF", bColor);
			DrawStatLine("Defense Index", (int)player.GetTotalDefense(), x, y + 4, false, "#00FFFF", "#FFFFFF", bColor);
			DrawHardwareLine("Socket Array", $"{socketsUsed}/{totalSockets}", x, y + 5, "#BC1DBC", bColor);
			DrawStatLine("Threat Rank", player.DifficultyRating, x, y + 6, false, "#FF0000", "#FFFFFF", bColor);
			DrawHardwareLine("Buffer Cap", $"{player.Inventory.Count}/{player.InventorySize}", x, y + 7, "#DCDCDC", bColor);
			DrawHardwareLine("Status", "OPTIMIZED", x, y + 8, "#00FF00", bColor);

			SetCursorPosition(x, y + 9);
			WriteLine(mid.Pastel(bColor));

			// Core System Integrity line
			SetCursorPosition(x, y + 10);
			string integrityText = $" Integrity: {player.Health}/{player.GetTotalMaxHealth()}";
			WriteLine($"║{integrityText.PadRight(40)}║".Pastel(bColor));

			SetCursorPosition(x, y + 11);
			WriteLine(bottom.Pastel(bColor));
		}

		// Helper for strings like "Hardware: Kinetic Breacher"
		private static void DrawHardwareLine(string label, string value, int x, int y, string valColor, string bColor)
		{
			SetCursorPosition(x, y);
			string content = $" {label}: {value}";
			WriteLine($"║{content.PadRight(40)}║".Pastel(bColor).Replace(value, value.Pastel(valColor)));
		}
		private static void DrawStatLine(string label, int value, int x, int y, bool isSelected, string aCol, string iCol, string bColor)
		{
			// 1. Setup
			CursorVisible = false;
			SetCursorPosition(x, y);

			// 2. Select the text color based on focus
			string textColor = isSelected ? aCol : iCol;

			// 3. Build the visual content manually to ensure it hits exactly 40 chars
			// We calculate based on raw text to avoid ANSI math drift
			string indicator = isSelected ? "> " : "  ";
			string mainText = $"{indicator}{label}: {value}";

			// 4. Pad the content to 40 characters so the right '║' snaps to the edge
			string finalLine = $"║{mainText.PadRight(40)}║";

			// 5. Write with colors (Applying Pastel to the whole line or just text)
			// Using bColor for the borders to match your DrawAttributeBox
			Write(finalLine.Pastel(bColor));

			SetCursorPosition(0, 0);
		}

		public static bool GetArrowChoice(string prompt, string option1, string option2)
		{
			int selected = 0;
			while (true)
			{
				Clear();
				WriteLine($"\n {prompt.Pastel("#FFD700")}\n");
				WriteLine($" {(selected == 0 ? " > ".Pastel("#00FF00") : "    ")} {option1}");
				WriteLine($" {(selected == 1 ? " > ".Pastel("#00FF00") : "    ")} {option2}");
				var key = ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow) selected = 0;
				else if (key == ConsoleKey.DownArrow) selected = 1;
				else if (key == ConsoleKey.Enter) return selected == 0;
			}
		}
	}
}