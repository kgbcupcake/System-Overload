using System_Overload.Src.GameData.Components;
using Pastel;
using Src.GameData.Components;
using static System_Overload.Src.GameData.Entities.PlayerData;
using static System.Console;
using Maries_Ui;

namespace System_Overload.Src.Interfaces.UI
{
	// Game-specific UI rendering that depends on GameState/PlayerData/WeaponData.
	// Kept out of Maries_Ui so that library stays free of System_Overload coupling.
	public static class GameUiEngine
	{
		public static void DrawDynamicFrame(string title, List<string> lines, string hint = "", int boxWidth = 66, int startY = 11, int selectedIndex = -1, int previewHp = -1, int previewCoins = -1)
		{
			CursorVisible = false;

			// 1. Unified Setup - Using 94 Anchor
			int screenWidth = 94;
			int startX = (screenWidth / 2) - (boxWidth / 2);
			// startY is already a parameter, we use the value passed in (11)
			string borderColor = UiEngine.BorderColor;

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
				UiEngine.DrawCentered(hint.Pastel("#555555"), startY + lines.Count + 3);
			}

			ResetColor();

			// --- FINAL FOOTER ---
			HudChrome.DisplayFooter(GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);
		}

		public static void DrawAttributeBox(loadPlayer player, int x, int y, int selectedStat = -1)
		{
			string bColor = UiEngine.BorderColor; // Reusing the shared BorderColor variable

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
			Console.CursorVisible = false;
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
			// Using bColor for the borders to match DrawAttributeBox
			Write(finalLine.Pastel(bColor));

			SetCursorPosition(0, 0);
		}
	}
}
