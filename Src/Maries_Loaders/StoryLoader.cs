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

		/// <summary>
		/// Tokens every line can use without the caller passing them, such as {hero} or {loved}.
		/// The game sets this once at startup; it's read on every lookup so it always reflects
		/// the current hero. Tokens the caller passes win over these.
		/// </summary>
		public static Func<IEnumerable<(string Token, object? Value)>>? GlobalTokens { get; set; }

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

		/// <summary>
		/// The lines for a key exactly as written, with no tokens filled in and no dev-mode
		/// placeholder. Global token providers use this, since filling would call them again.
		/// </summary>
		public static IReadOnlyList<string> Raw(string key) =>
			_entries.TryGetValue(key, out var lines) ? lines : Array.Empty<string>();

		/// <summary>
		/// Fills {tokens} in text that comes from other content, such as a quest name, the same
		/// way story lines are filled.
		/// </summary>
		public static string Format(string text, params (string Token, object? Value)[] tokens) =>
			string.IsNullOrEmpty(text) ? "" : Fill(text, tokens);

		/// <summary>True if the key exists and has at least one line. Use it for optional text.</summary>
		public static bool Has(string key) => _entries.TryGetValue(key, out var lines) && lines.Count > 0;

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

			// Globals go second so a caller can override one for a single line.
			if (GlobalTokens != null && line.Contains('{'))
			{
				foreach (var (token, value) in GlobalTokens())
					line = line.Replace("{" + token + "}", value?.ToString() ?? "");
			}
			return line;
		}
	}
}
