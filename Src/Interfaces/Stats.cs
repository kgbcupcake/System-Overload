using System_Overload.Src.GameData.Components;
using Pastel;
using Src.GameData.Components;
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
			string filledPart = new string('■', filledSlots).Pastel("#00FFFF");
			string emptyPart = new string('-', 10 - filledSlots).Pastel("#333333");

			string openB = "[".Pastel("#00FFFF");
			string closeB = "]".Pastel("#00FFFF");
			string xpBar = openB + filledPart + emptyPart + closeB;

			string xpString = $"{p.Experience}/{p.ExperienceToLevel}";

			Clear();

			// 3. Layout Settings
			int screenWidth = 85;
			int boxWidth = 50;
			int center = screenWidth / 2;
			int startX = center - (boxWidth / 2);

			string bColor = "#00FFFF"; // Cyber Cyan
			string vColor = "#FFFFFF"; // Pure White
			string hColor = "#FFD700"; // Gold Header

			List<string> statLines = new List<string>();

			statLines.Add("╔══════════════════════════════════════════════════╗".Pastel(bColor));
			statLines.Add(BuildStatLine("SIM LEVEL", playerLevel.ToString(), "#BC1DBC", bColor));
			statLines.Add(BuildStatLine("SYNC STABILITY", $"{p.Health}/{p.GetTotalMaxHealth()}", "#00FF00", bColor));
			statLines.Add(BuildStatLine("CREDITS", p.Coins.ToString(), vColor, bColor));
			statLines.Add(BuildStatLine("MEMORY FRAGS", xpString, vColor, bColor));
			statLines.Add(BuildBarLine("DATA UPLINK", xpBar, bColor));

			statLines.Add("╟──────────────────────────────────────────────────╢".Pastel(bColor));

			// --- DYNAMIC HARDWARE SECTION (Weapon/Sockets) ---
			// We look for the first weapon in inventory as the 'Active' one
			var weapon = p.Inventory.OfType<WeaponData>().FirstOrDefault();

			if (weapon != null)
			{
				statLines.Add(BuildStatLine("HARDWARE", weapon.Name, "#FF4500", bColor));
				statLines.Add(BuildStatLine("BASE DMG", weapon.Damage.ToString(), vColor, bColor));
				statLines.Add(BuildStatLine("SOCKETS", $"{weapon.SocketedGems.Count}/{weapon.NumSockets}", "#00FFFF", bColor));

				// List specific Socketed Gems
				foreach (var gem in weapon.SocketedGems)
				{
					statLines.Add(BuildStatLine(" > MODULE", gem.Name, "#DCDCDC", bColor));
				}
			}
			else
			{
				statLines.Add(BuildStatLine("HARDWARE", "UNARMED", "#555555", bColor));
			}

			statLines.Add("╚══════════════════════════════════════════════════╝".Pastel(bColor));
			statLines.Add("             [ PRESS ANY KEY TO DISCONNECT ]             ");

			// 5. Draw Header
			string header = $"LINKED PROFILE: {p.PlayerName.ToUpper()} ({p.PlayerClass})";
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
				WriteLine("\n" + "!!! SIMULATION COMPLEXITY INCREASED !!!".Pastel("#FF0000"));
				WriteLine($"Threat Rank: {p.DifficultyRating}".Pastel("#FF4500"));

				p.Health = (int)p.GetTotalMaxHealth();
				WriteLine("System integrity restored to 100%.".Pastel("#00FF00"));
				ReadKey(true);
			}
		}
	}
}