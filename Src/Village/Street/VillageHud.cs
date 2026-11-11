using DungeonAdventures.Src.Scenes;
using Maries_Loaders;
using Pastel;

namespace DungeonAdventures.Src.Village.Street
{
	/// <summary>
	/// The box under the village street: what the hero stands at (and who's in reach to talk to) on the first
	/// line, and the keys on the second. Each key hint is clickable. Only redrawn when its text changes.
	/// </summary>
	internal sealed class VillageHud(Scene scene, bool dev)
	{
		private const string Gold = "#FFD700", Muted = "#9A8A70", Key = "#C8A060";
		private const string Gap = "  ";
		private string _shown = "";

		/// <summary>The key hints: label and the command a click on it gives.</summary>
		private readonly List<(string Label, string Command)> _hints = new()
		{
			("←→ Walk", "right"), ("↑ Go in", "enter"), ("T Talk", "talk"), ("J Journal", "journal"),
			("C Hero", "hero"), ("I Bag", "bag"), ("Tab Menu", "menu"), ("Q Leave", "quit"),
		};

		/// <summary>The two lines for where the hero is: a place they're at and a villager in reach, either may be null.</summary>
		public string[] Page(string? place, string? villager)
		{
			string line;
			if (place != null)
				line = place.ToUpperInvariant().Pastel(Gold) + "   [↑] Go in".Pastel(Muted)
					+ (villager != null ? $"   [T] Talk to {villager}".Pastel(Muted) : "");
			else if (villager != null)
				line = villager.Pastel(Gold) + "   [T] Talk".Pastel(Muted);
			else
				line = Story.Line("town.title").Pastel("#FFAB00") + "   The lamps are lit along the street.".Pastel(Muted);
			return new[] { line, HintLine() };
		}

		/// <summary>Draws the page when it differs from what's in the box.</summary>
		public void Show(string[] page)
		{
			string key = string.Join("\n", page);
			if (key == _shown) return;
			_shown = key;
			scene.Box.Draw(page, more: false);
		}

		/// <summary>Forget what's drawn (after the screen was cleared).</summary>
		public void Invalidate() => _shown = "";

		/// <summary>The command of the key hint at a terminal cell, if there is one.</summary>
		public string? HintAt(int col, int row)
		{
			if (row != scene.Height - StoryBox.Rows + 2) return null;
			int x = 3;
			foreach (var (label, command) in Hints())
			{
				if (col >= x && col < x + label.Length) return col == x && command == "right" ? "left" : command; // ← of ←→
				x += label.Length + Gap.Length;
			}
			return null;
		}

		private IEnumerable<(string Label, string Command)> Hints() =>
			dev ? _hints.Append(("R Reload", "reload")) : _hints;

		private string HintLine() =>
			string.Join(Gap, Hints().Select(h => h.Label[..h.Label.IndexOf(' ')].Pastel(Key) + h.Label[h.Label.IndexOf(' ')..].Pastel(Muted)));
	}
}
