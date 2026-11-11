using Maries_Loaders;
using Pastel;
using System.Text;

using static System.Console;

namespace Maries_Ui
{
	/// <summary>The goodbye sequence when the game closes.</summary>
	public static class ShutdownSequence
	{
		public static void Play()
		{
			CursorVisible = false;
			Random rnd = new Random();

			// 1. Farewell lines
			Clear();
			var tasks = Story.Lines("shutdown.tasks");
			string pending = Story.Line("shutdown.task_pending");
			string done = Story.Line("shutdown.task_done");
			string suffix = Story.Line("shutdown.task_suffix");

			for (int i = 0; i < tasks.Count; i++)
			{
				SetCursorPosition(4, 5 + i);
				Write($"{pending} {tasks[i]}".Pastel("#555555"));
				Thread.Sleep(1000);
				SetCursorPosition(4, 5 + i);
				Write($"{done} {tasks[i]}".Pastel("#8B0000"));
				WriteLine($" {suffix}".Pastel("#444444"));
			}
			Thread.Sleep(1000);

			// 2. Falling ash + warnings
			for (int i = 0; i < 15; i++)
			{
				Clear();
				for (int r = 0; r < 20; r++) // Ash background
				{
					string rain = new string(' ', rnd.Next(2, 80)) + (rnd.Next(2) == 0 ? "*" : "·");
					SetCursorPosition(0, r);
					Write(rain.Pastel("#3A2A1A")); // Dim ash
				}

				UiEngine.DrawCentered(Story.Line("shutdown.warning").Pastel("#FF0000"), 8);
				UiEngine.DrawCentered(Story.Line("shutdown.whisper").Pastel("#555555"), 10);
				Thread.Sleep(80);
			}

			// 3. The Purple Loading Bar
			int barWidth = 40;
			for (int p = 0; p <= 100; p += 2)
			{
				ProgressBarRenderer.Draw(27, 12, p / 100.0, barWidth, "#800080", "#333333");

				if (p >= 40)
				{
					UiEngine.DrawCentered(Story.Line("shutdown.doom").Pastel("#FF0000"), 15);
				}
				// Stutters more as it reaches 100% to simulate a crash
				Thread.Sleep(p > 80 ? rnd.Next(150, 400) : rnd.Next(30, 100));
			}

			// 4. The flickering skull (Art Folder Integration)
			for (int flicker = 0; flicker < 10; flicker++)
			{
				Clear();
				string color = (flicker % 2 == 0) ? "#FF0000" : "#220000";
				int startY = 4;
				// Skull art from GameArt, with the caption from the story data
				var art = new List<string> { Story.Line("game_over.header") };
				art.AddRange(GameOver.Skull);
				art.Add(Story.Line("game_over.footer"));
				foreach (string line in art)
				{
					UiEngine.DrawCentered(line.Pastel(color), startY++);
				}
				Thread.Sleep(100);
			}

			// 5. Fatal Termination Bar
			Clear();
			SetCursorPosition(0, 11);
			string finalMsg = $" {Story.Line("shutdown.final")} ";
			// Force the red background to fill the 94-width reality anchor
			Write(finalMsg.PadLeft((94 + finalMsg.Length) / 2).PadRight(94).PastelBg("#8B0000").Pastel("#FFFFFF"));

			Thread.Sleep(3500);
			Environment.Exit(0);
		}
	}
}
