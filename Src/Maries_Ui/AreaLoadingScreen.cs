using Maries_Loaders;
using Pastel;
using System.Text;

using static System.Console;

namespace Maries_Ui
{
	/// <summary>A short loading screen when entering an area, in the area's colour.</summary>
	public static class AreaLoadingScreen
	{
		public static void Show(string areaName, string hexColor)
		{
			Console.Clear();
			Console.CursorVisible = false;

			int barWidth = 40; // Slightly narrower for a "smaller" area feel
			int centerX = 94 / 2;
			int centerY = 25 / 2;

			// 1. Label (e.g., "ENTERING THE GENERAL STORE...")
			string plainLabel = $"ENTERING THE {areaName.ToUpper()}...";
			UiEngine.DrawCentered(plainLabel.Pastel(hexColor), centerY - 2);

			// 2. The Bar
			for (int i = 0; i <= 100; i++)
			{
				int barLeft = centerX - (barWidth / 2);
				// Use the hexColor passed into the method for the bar!
				ProgressBarRenderer.Draw(barLeft, centerY, i / 100.0, barWidth, hexColor, "#333333");

				Thread.Sleep(15); // Stores usually load fast!
			}

			Thread.Sleep(400);
			Console.Clear();
			//Store.LoadShop();
			Console.CursorVisible = true;
		}
	}
}
