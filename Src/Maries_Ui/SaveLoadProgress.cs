using Maries_Loaders;
using Pastel;
using System.Text;

using static System.Console;

namespace Maries_Ui
{
	/// <summary>The progress bar while a save is read from disk.</summary>
	public static class SaveLoadProgress
	{
		public static void Play()
		{
			Console.Clear();

			int centerX = 85 / 2;
			int centerY = 12;
			int barWidth = 40;
			Random rnd = new Random();

			// --- RAIN CONFIG ---
			// Columns on the left (5, 10, 15) and right (70, 75, 80)
			int[] rainCols = { 5, 10, 15, 70, 75, 80 };
			int[] rainY = new int[rainCols.Length];
			for (int r = 0; r < rainY.Length; r++) rainY[r] = rnd.Next(2, 22);

			// 1. PHASE ONE: RAPID DATA SCAN
			for (int i = 0; i < 15; i++)
			{
				Console.SetCursorPosition(centerX - 20, centerY);
				string noise = Story.Pick("load_save.scan", ("n", rnd.Next(1, 999))).PadRight(40);
				Console.Write(noise.Pastel("#B8860B"));
				Thread.Sleep(90);
			}

			var steps = Story.Lines("load_save.steps");

			// 2. PHASE TWO: THE PROGRESS BAR + RAIN
			for (int i = 0; i < steps.Count; i++)
			{
				Console.SetCursorPosition(centerX - 25, centerY - 2);
				Console.Write(new string(' ', 50));
				Console.SetCursorPosition(centerX - (steps[i].Length / 2), centerY - 2);
				Console.Write(steps[i].Pastel("#FFAB00"));

				for (int p = 0; p <= 10; p++)
				{
					// --- UPDATE FALLING EMBERS ---
					for (int r = 0; r < rainCols.Length; r++)
					{
						// Erase old bit
						Console.SetCursorPosition(rainCols[r], rainY[r]);
						Console.Write(" ");

						// Increment and wrap Y
						rainY[r]++;
						if (rainY[r] > 21) rainY[r] = 2;

						// Draw new bit
						Console.SetCursorPosition(rainCols[r], rainY[r]);
						string bit = rnd.Next(2) == 0 ? "*" : "·";
						// Alternate between bright and dim embers for depth
						string color = rnd.Next(3) == 0 ? "#FF8C00" : "#5A2A00";
						Console.Write(bit.Pastel(color));
					}

					// --- UPDATE PROGRESS BAR ---
					float totalPercent = ((i * 10) + p) / (steps.Count * 10f);
					ProgressBarRenderer.Draw(centerX - (barWidth / 2), centerY, totalPercent, barWidth, "#FFAB00", "#222222");

					Thread.Sleep(rnd.Next(30, 80));
				}
			}

			// 3. FINAL LOCK-ON
			Console.SetCursorPosition(centerX - 10, centerY + 2);
			Console.Write(Story.Line("load_save.done").Pastel("#FFD700"));

			PlatformConsole.PlayBeepSequence((1200, 100), (1500, 150));
			Thread.Sleep(800);
		}
	}
}
