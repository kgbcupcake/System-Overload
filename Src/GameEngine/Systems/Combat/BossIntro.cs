using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Lpc;
using DungeonAdventures.Src.Scenes;
using Maries_Ui;
using Pastel;

namespace DungeonAdventures.Src.GameEngine.Combat
{
	/// <summary>The screen before a boss fight: the boss stands tall on the right while it introduces itself.</summary>
	public static class BossIntro
	{
		public static void Show(Combatant boss, string banner, string? speech)
		{
			int w = Math.Max(80, SafeWidth()), h = Math.Max(20, SafeHeight());
			string colour = boss.Boss!.HexColor;
			Console.Clear();
			HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "",
				GameState.CurrentPlayer?.Health ?? 0, (int)(GameState.CurrentPlayer?.GetTotalMaxHealth() ?? 0),
				GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);

			int rows = Math.Clamp(h - 4, 8, HeroSprite.Rows);
			if (boss.Look != null && HeroSprite.CanDraw(boss.Look) && PixelIcon.Enabled)
			{
				try
				{
					var sprite = HeroSprite.RenderSized(boss.Look, 2, rows);
					int x = w - 6 - HeroSprite.WidthFor(rows);
					for (int r = 0; r < sprite.Length; r++)
					{
						Console.SetCursorPosition(x, 2 + r);
						Console.Write(sprite[r]);
					}
				}
				catch (Exception) { }
			}

			void Line(int y, string text) { Console.SetCursorPosition(3, y); Console.Write(text); }
			Line(3, "╔══════════════════════════════════════════════╗".Pastel(colour));
			Line(4, $"║  {banner.PadRight(44)}║".Pastel(colour));
			Line(5, "╚══════════════════════════════════════════════╝".Pastel(colour));
			Line(7, $"{boss.Name.ToUpper()}  //  LVL {boss.Level}  //  HP {boss.MaxHealth}".Pastel(colour));
			int y = 9;
			if (!string.IsNullOrWhiteSpace(speech))
				foreach (string part in StoryBox.Wrap($"\"{speech}\"", Math.Max(30, w - HeroSprite.WidthFor(rows) - 14)))
					Line(y++, part);
			Line(y + 1, "Press any key to engage...".Pastel("#888888"));
			MouseInput.WaitForAnyInput();
		}

		private static int SafeWidth() { try { return Console.WindowWidth; } catch { return 94; } }
		private static int SafeHeight() { try { return Console.WindowHeight; } catch { return 24; } }
	}
}
