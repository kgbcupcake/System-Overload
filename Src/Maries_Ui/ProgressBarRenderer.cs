using Pastel;

namespace Maries_Ui
{
	public static class ProgressBarRenderer
	{
		public static string Render(double percent, int width, string filledColor, string emptyColor, bool showPercentLabel = true)
		{
			percent = Math.Clamp(percent, 0.0, 1.0);
			int filledBlocks = (int)(percent * width);

			string filled = new string('█', filledBlocks).Pastel(filledColor);
			string empty = new string('░', width - filledBlocks).Pastel(emptyColor);
			string label = showPercentLabel ? $" {(int)(percent * 100)}%".Pastel("#FFFFFF") : "";

			return filled + empty + label;
		}

		public static void Draw(int left, int top, double percent, int width, string filledColor, string emptyColor, bool showPercentLabel = true)
		{
			Console.SetCursorPosition(left, top);
			Console.Write(Render(percent, width, filledColor, emptyColor, showPercentLabel));
		}
	}
}
