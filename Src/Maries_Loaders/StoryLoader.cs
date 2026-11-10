using Maries_DataModels;

namespace Maries_Loaders
{
	/// <summary>
	/// Reads story text from the "story" content folder. Code asks for a key and gets the lines
	/// back with {tokens} filled in. A missing key shows as &lt;key&gt; in dev mode so gaps are easy
	/// to spot, and as nothing in a normal build.
	/// </summary>
	public static class Story
	{
		private static readonly Random _rng = new();
		private static Dictionary<string, List<string>> _entries = new(StringComparer.OrdinalIgnoreCase);

		public static void LoadAll()
		{
			_entries = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (var entry in UniversalLoader.LoadCollection<StoryEntry>("story"))
			{
				if (!string.IsNullOrWhiteSpace(entry.Key))
					_entries[entry.Key] = entry.Lines ?? new List<string>();
			}
		}

		/// <summary>All lines for a key, with tokens filled in.</summary>
		public static List<string> Lines(string key, params (string Token, object? Value)[] tokens)
		{
			if (!_entries.TryGetValue(key, out var lines) || lines.Count == 0)
				return LoaderConfig.DevMode ? new List<string> { $"<{key}>" } : new List<string>();
			return lines.Select(l => Fill(l, tokens)).ToList();
		}

		/// <summary>The first line for a key.</summary>
		public static string Line(string key, params (string Token, object? Value)[] tokens)
			=> Lines(key, tokens).FirstOrDefault() ?? "";

		/// <summary>A random line for a key.</summary>
		public static string Pick(string key, params (string Token, object? Value)[] tokens)
		{
			var lines = Lines(key, tokens);
			return lines.Count == 0 ? "" : lines[_rng.Next(lines.Count)];
		}

		private static string Fill(string line, (string Token, object? Value)[] tokens)
		{
			foreach (var (token, value) in tokens)
				line = line.Replace("{" + token + "}", value?.ToString() ?? "");
			return line;
		}
	}
}
