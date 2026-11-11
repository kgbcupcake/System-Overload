using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Lpc;
using DungeonAdventures.Src.GameEngine.Systems;
using Maries_Ui;
using Pastel;
using Maries_DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace DungeonAdventures.Src.GameEngine.Interfaces
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

			// 3. Layout Settings. With a look from "Make your character", the hero stands on the left
			// and the stats box moves right; otherwise the box is centred as before.
			bool portrait = HeroSprite.CanDraw(p.Look);
			int screenWidth = 85;
			int boxWidth = 50;
			int center = screenWidth / 2;
			int startX = center - (boxWidth / 2);
			if (portrait)
			{
				startX = PortraitX + PortraitW + 1;
				center = startX + 26;
			}

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
			// The weapon the hero fights with (equipped, or their best one)
			var weapon = p.GetEquippedWeapon();

			if (weapon != null)
			{
				statLines.Add(BuildStatLine("WEAPON", weapon.Name, "#FF4500", bColor));
				statLines.Add(BuildStatLine("BASE DMG", weapon.Damage.ToString(), vColor, bColor));
				statLines.Add(BuildStatLine("SOCKETS", $"{weapon.SocketedGems.Count}/{weapon.NumSockets}", "#FFD700", bColor));

				// Socketed gems, on one line so the sheet fits the 24-row screen
				if (weapon.SocketedGems.Count > 0)
				{
					string gems = string.Join(", ", weapon.SocketedGems.Select(g => g.Name));
					if (gems.Length > 28) gems = gems[..25] + "...";
					statLines.Add(BuildStatLine(" > GEMS", gems, "#DCDCDC", bColor));
				}
			}
			else
			{
				statLines.Add(BuildStatLine("WEAPON", "UNARMED", "#555555", bColor));
			}

			// --- GEAR SECTION (worn armor and clothing) ---
			var rules = EquipmentRules.Current;
			var armor = p.GetEquippedArmor();
			statLines.Add("╟──────────────────────────────────────────────────╢".Pastel(bColor));
			statLines.Add(BuildStatLine("DEFENSE", ((int)p.GetTotalDefense()).ToString(), "#87CEEB", bColor));
			if (armor.Count == 0)
				statLines.Add(BuildStatLine("GEAR", "NOTHING WORN", "#555555", bColor));
			foreach (var piece in armor)
			{
				string sockets = rules.SocketsOn(piece) > 0 ? $" [{piece.SocketedGems.Count}/{rules.SocketsOn(piece)}]" : "";
				statLines.Add(BuildStatLine(rules.SlotLabel(piece.Slot).ToUpper(), piece.Name + sockets, "#DCDCDC", bColor));
			}

			// Total gem bonuses from everything equipped, as set by the equipment rules JSON.
			var bonuses = Enum.GetValues<SocketStat>().Select(s => (s, p.GetSocketBonus(s)));
			string bonusText = EquipmentRules.Describe(bonuses);
			if (bonusText.Length > 28) bonusText = bonusText[..25] + "...";
			statLines.Add(BuildStatLine("GEM BONUS", bonusText, "#BC8FDF", bColor));

			statLines.Add("╚══════════════════════════════════════════════════╝".Pastel(bColor));
			statLines.Add(portrait
				? "   [ and ] turn the hero    any other key returns   ".Pastel("#777777")
				: "               [ PRESS ANY KEY TO RETURN ]               ");

			// 5. Draw Header
			string origin = StoryTokens.Origin(p);
			string header = $"HERO: {p.PlayerName.ToUpper()} ({(origin.Length > 0 ? origin + " " : "")}{p.PlayerClass})";
			int headerPadding = header.Length / 2;
			SetCursorPosition(Math.Max(0, center - headerPadding), 1);
			WriteLine(header.Pastel(hColor));

			// 6. Draw the Box
			int row = 2;
			foreach (string line in statLines)
			{
				SetCursorPosition(startX, row++);
				WriteLine(line);
			}

			if (!portrait)
			{
				MouseInput.WaitForAnyInput();
				Clear();
				return;
			}

			ShowPortrait(p.Look!);
			Clear();
		}

		// The hero's frame on the left of the sheet: x 1..40, rows 2..21, TURN set into its bottom edge.
		private const int PortraitX = 1, PortraitW = 40, PortraitTop = 2, PortraitH = 20;

		/// <summary>
		/// Draws the hero walking in place until a key or click. [ and ] (or the TURN arrows) turn the
		/// hero; anything else closes the sheet.
		/// </summary>
		private static void ShowPortrait(HeroLook look)
		{
			string border = "#B8860B";
			SetCursorPosition(PortraitX, PortraitTop);
			Write(("╔" + new string('═', PortraitW - 2) + "╗").Pastel(border));
			for (int r = 1; r < PortraitH - 1; r++)
			{
				SetCursorPosition(PortraitX, PortraitTop + r);
				Write("║".Pastel(border));
				SetCursorPosition(PortraitX + PortraitW - 1, PortraitTop + r);
				Write("║".Pastel(border));
			}
			string turn = " ◀  TURN  ▶ ";
			int side = (PortraitW - 2 - turn.Length) / 2;
			int turnX = PortraitX + 1 + side;
			SetCursorPosition(PortraitX, PortraitTop + PortraitH - 1);
			Write(("╚" + new string('═', side)).Pastel(border) + turn.Pastel("#FFD700")
				+ (new string('═', PortraitW - 2 - side - turn.Length) + "╝").Pastel(border));

			int spriteX = PortraitX + (PortraitW - HeroSprite.Width) / 2;
			int dir = 2, frame = 0;
			while (true)
			{
				HeroSprite.Draw(look, spriteX, PortraitTop + 1, dir, frame);
				while (!MouseInput.InputAvailable)
				{
					Thread.Sleep(110);
					frame = (frame + 1) % HeroSprite.Frames;
					HeroSprite.Draw(look, spriteX, PortraitTop + 1, dir, frame);
				}

				var input = MouseInput.Read();
				char ch = input.Key?.KeyChar ?? '\0';
				int turnStep = ch == '[' ? -1 : ch == ']' ? 1 : 0;
				if (input.Mouse is { Action: MouseAction.LeftClick } m && m.Y == PortraitTop + PortraitH - 1)
				{
					if (m.X >= turnX && m.X < turnX + 3) turnStep = -1;
					else if (m.X >= turnX + turn.Length - 3 && m.X < turnX + turn.Length) turnStep = 1;
				}
				if (input.Mouse is { Action: MouseAction.WheelUp or MouseAction.WheelDown }) continue;
				if (turnStep == 0) return;
				dir = (dir + turnStep + 4) % 4;
			}
		}

		private const int ContentWidth = 48;

		private static string BuildStatLine(string label, string value, string valColor, string borderColor)
		{
			string leftWall = "║ ".Pastel(borderColor);
			string rightWall = " ║".Pastel(borderColor);
			string labelPart = (label + ":").PadRight(18);

			// The border lines are 52 wide: "║ " + 48 columns of content + " ║"
			int totalContentWidth = ContentWidth;
			int remainingSpace = totalContentWidth - (labelPart.Length + value.Length);
			string padding = new string(' ', Math.Max(0, remainingSpace));

			return leftWall + labelPart + value.Pastel(valColor) + padding + rightWall;
		}

		private static string BuildBarLine(string label, string bar, string borderColor)
		{
			string leftWall = "║ ".Pastel(borderColor);
			string rightWall = " ║".Pastel(borderColor);
			string labelPart = (label + ":").PadRight(18);

			// Pad by the bar's visible width (its colour codes take no space on screen)
			int barWidth = UiEngine.StripAnsi(bar).Length;
			string padding = new string(' ', Math.Max(0, ContentWidth - labelPart.Length - barWidth));

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
				MouseInput.WaitForAnyInput();
			}
		}
	}
}