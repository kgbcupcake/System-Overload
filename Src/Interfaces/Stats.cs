using System_Overload.Src.GameData.Components;
using Pastel;
using Maries_DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace System_Overload.Src.GameEngine.Interfaces
{
	internal class Stats
	{
		public static void PlayerStats()
		{
			var p = GameState.CurrentPlayer;

			// 1. Data Calculation
			int maxLevelXp = p.ExperienceToLevel;
			int currentXp = p.Experience;
			int playerLevel = p.Level;

			// Calculate progress for a 10-block bar
			double xpPercent = (double)currentXp / Math.Max(1, maxLevelXp);
			int filledSlots = Math.Clamp((int)(xpPercent * 10), 0, 10);

			// 2. Build the UI Components
			string filledPart = new string('■', filledSlots).Pastel("#FFD700");
			string emptyPart = new string('-', 10 - filledSlots).Pastel("#333333");

			string openB = "[".Pastel("#B8860B");
			string closeB = "]".Pastel("#B8860B");
			string xpBar = openB + filledPart + emptyPart + closeB;

			string xpString = $"{p.Experience}/{p.ExperienceToLevel}";

			Clear();

			// 3. Layout Settings
			int screenWidth = 85;
			int boxWidth = 50;
			int center = screenWidth / 2;
			int startX = center - (boxWidth / 2);

			string bColor = "#B8860B"; // Old gold
			string vColor = "#FFFFFF"; // Pure White
			string hColor = "#FFD700"; // Gold Header

			List<string> statLines = new List<string>();

			statLines.Add("╔══════════════════════════════════════════════════╗".Pastel(bColor));
			statLines.Add(BuildStatLine("LEVEL", playerLevel.ToString(), "#BC1DBC", bColor));
			statLines.Add(BuildStatLine("HEALTH", $"{p.Health}/{p.GetTotalMaxHealth()}", "#00FF00", bColor));
			statLines.Add(BuildStatLine("GOLD", p.Coins.ToString(), vColor, bColor));
			statLines.Add(BuildStatLine("EXPERIENCE", xpString, vColor, bColor));
			statLines.Add(BuildBarLine("NEXT LEVEL", xpBar, bColor));

			statLines.Add("╟──────────────────────────────────────────────────╢".Pastel(bColor));

			// --- WEAPON SECTION (Weapon/Sockets) ---
			// We look for the first weapon in inventory as the 'Active' one
			var weapon = p.Inventory.OfType<WeaponData>().FirstOrDefault();

			if (weapon != null)
			{
				statLines.Add(BuildStatLine("WEAPON", weapon.Name, "#FF4500", bColor));
				statLines.Add(BuildStatLine("BASE DMG", weapon.Damage.ToString(), vColor, bColor));
				statLines.Add(BuildStatLine("SOCKETS", $"{weapon.SocketedGems.Count}/{weapon.NumSockets}", "#FFD700", bColor));

				// List specific Socketed Gems
				foreach (var gem in weapon.SocketedGems)
				{
					statLines.Add(BuildStatLine(" > GEM", gem.Name, "#DCDCDC", bColor));
				}
			}
			else
			{
				statLines.Add(BuildStatLine("WEAPON", "UNARMED", "#555555", bColor));
			}

			statLines.Add("╚══════════════════════════════════════════════════╝".Pastel(bColor));
			statLines.Add("               [ PRESS ANY KEY TO RETURN ]               ");

			// 5. Draw Header
			string header = $"HERO: {p.PlayerName.ToUpper()} ({p.PlayerClass})";
			int headerPadding = header.Length / 2;
			SetCursorPosition(Math.Max(0, center - headerPadding), 4);
			WriteLine(header.Pastel(hColor));

			// 6. Draw the Box
			int row = 7;
			foreach (string line in statLines)
			{
				SetCursorPosition(startX, row++);
				WriteLine(line);
			}

			ReadKey(true);
			Clear();
		}

		private static string BuildStatLine(string label, string value, string valColor, string borderColor)
		{
			string leftWall = "║ ".Pastel(borderColor);
			string rightWall = " ║".Pastel(borderColor);
			string labelPart = (label + ":").PadRight(18);

			// Adjusted width for the 50-char box
			int totalContentWidth = 46;
			int remainingSpace = totalContentWidth - (labelPart.Length + value.Length);
			string padding = new string(' ', Math.Max(0, remainingSpace));

			return leftWall + labelPart + value.Pastel(valColor) + padding + rightWall;
		}

		private static string BuildBarLine(string label, string bar, string borderColor)
		{
			string leftWall = "║ ".Pastel(borderColor);
			string rightWall = " ║".Pastel(borderColor);
			string labelPart = (label + ":").PadRight(18);

			// Fixed padding for the progress bar
			string padding = new string(' ', 19);

			return leftWall + labelPart + bar + padding + rightWall;
		}

		public static void CheckLevelUp()
		{
			var p = GameState.CurrentPlayer;
			// Replace .Mods with .DifficultyRating
			if (p.Level > p.DifficultyRating)
			{
				p.DifficultyRating = p.Level;
				Clear();
				WriteLine("\n" + "!!! THE DUNGEON GROWS DARKER !!!".Pastel("#FF0000"));
				WriteLine($"Danger Rank: {p.DifficultyRating}".Pastel("#FF4500"));

				p.Health = (int)p.GetTotalMaxHealth();
				WriteLine("Your health is fully restored.".Pastel("#00FF00"));
				ReadKey(true);
			}
		}
	}
}