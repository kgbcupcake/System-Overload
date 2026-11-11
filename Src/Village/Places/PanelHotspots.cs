using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;


namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>Clickable spans recorded while a panel screen draws, looked up when the mouse clicks.</summary>
	internal static class PanelHotspots
	{
		/// <summary>A clickable span on one screen row, recorded while drawing.</summary>
		private sealed record Hotspot(int Y, int X1, int X2, string Command);
		private static readonly List<Hotspot> _hotspots = new();

		public static void Clear() => _hotspots.Clear();

		public static void Add(int y, int x, int width, string command) =>
			_hotspots.Add(new Hotspot(y, x, x + width - 1, command));

		public static string? At(int x, int y) =>
			_hotspots.FirstOrDefault(h => h.Y == y && x >= h.X1 && x <= h.X2)?.Command;
	}
}
