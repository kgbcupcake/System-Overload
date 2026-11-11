using System.Runtime.InteropServices;

namespace Maries_Ui
{
	/// <summary>
	/// The Windows console only passes mouse codes through with VT input on and Quick Edit off. This turns
	/// that on and puts the console's own mode back afterwards.
	/// </summary>
	internal static class WindowsConsoleMode
	{
		private static uint? _saved;

		private const int StdInputHandle = -10;
		private const uint EnableMouseInput = 0x0010;
		private const uint EnableQuickEditMode = 0x0040;
		private const uint EnableExtendedFlags = 0x0080;
		private const uint EnableVirtualTerminalInput = 0x0200;

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool GetConsoleMode(IntPtr handle, out uint mode);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleMode(IntPtr handle, uint mode);

		/// <summary>On Windows the console only passes mouse codes through with VT input on and Quick Edit off.</summary>
		public static void EnableVtInput()
		{
			try
			{
				var handle = GetStdHandle(StdInputHandle);
				if (!GetConsoleMode(handle, out uint mode)) return;
				_saved = mode;
				uint wanted = (mode | EnableVirtualTerminalInput | EnableExtendedFlags | EnableMouseInput) & ~EnableQuickEditMode;
				SetConsoleMode(handle, wanted);
			}
			catch { /* Not a real console (redirected input): keyboard still works. */ }
		}

		public static void Restore()
		{
			if (_saved is not { } mode) return;
			try { SetConsoleMode(GetStdHandle(StdInputHandle), mode); } catch { }
			_saved = null;
		}
	}
}
