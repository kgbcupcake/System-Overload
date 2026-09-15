namespace Maries_Ui
{
	public static class ConsoleSetup
	{
		public static void ConsoleSize()
		{
			int width = 94; // Adhering to 84x24 reality anchor
			int height = 24;

			PlatformConsole.TryResizeConsole(width, height);
			Console.CursorVisible = false;
		}
	}
}
