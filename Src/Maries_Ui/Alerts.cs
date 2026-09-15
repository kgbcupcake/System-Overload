using Pastel;

namespace Maries_Ui
{
	public static class Alerts
	{
		public static void QuickAlert(string message)
		{
			int y = 12;
			Console.Clear();
			// Center it
			Console.SetCursorPosition((85 / 2) - (message.Length / 2), y);
			Console.WriteLine(message.Pastel("#72071C"));
			Console.SetCursorPosition((85 / 2) - 10, y + 2);
			Console.Write("Press any key...".Pastel("#555555"));

			// This stops the code right here until a key is pressed!
			Console.ReadKey(true);
		}
	}
}
