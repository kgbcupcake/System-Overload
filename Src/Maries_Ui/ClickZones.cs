namespace Maries_Ui
{
	/// <summary>The clickable spans a screen registers while drawing: menu rows and key buttons.</summary>
	internal static class ClickZones
	{
		/// <summary>A clickable span on one screen row: either a menu row (Index) or a key button (Key).</summary>
		public sealed record Zone(int Y, int X1, int X2, int? Index, ConsoleKey? Key);
		private static readonly List<Zone> _zones = new();

		/// <summary>Forget every click zone. Screens call this before drawing their own.</summary>
		public static void Clear() => _zones.Clear();

		/// <summary>A menu row: clicking it selects <paramref name="index"/> and acts like Enter.</summary>
		public static void AddRow(int y, int x, int width, int index) =>
			_zones.Add(new Zone(y, x, x + Math.Max(1, width) - 1, index, null));

		/// <summary>
		/// A button: clicking it acts like pressing <paramref name="key"/>. With
		/// <paramref name="index"/> it also selects that menu row first (e.g. a [+] button on a row).
		/// </summary>
		public static void AddKey(int y, int x, int width, ConsoleKey key, int? index = null) =>
			_zones.Add(new Zone(y, x, x + Math.Max(1, width) - 1, index, key));

		public static Zone? At(int x, int y) =>
			_zones.LastOrDefault(z => z.Y == y && x >= z.X1 && x <= z.X2);
	}
}
