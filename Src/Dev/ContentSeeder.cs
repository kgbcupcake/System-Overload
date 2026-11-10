using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DungeonAdventures.Dev
{
	public static class ContentSeeder
	{
		private static readonly JsonSerializerOptions _writeOptions = new()
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // keep quotes, apostrophes and arrows readable
		};

		private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
		{
			// Creates the target directory (e.g., .../Realm Data/floors)
			Directory.CreateDirectory(target.FullName);

			foreach (FileInfo fi in source.GetFiles("*.json"))
			{
				string targetFilePath = Path.Combine(target.FullName, fi.Name);
				if (!File.Exists(targetFilePath))
					fi.CopyTo(targetFilePath);
				else
					AddMissingEntries(fi.FullName, targetFilePath);
			}
		}

		/// <summary>
		/// The player's copy of a content file is never overwritten, so their edits survive. But when the
		/// game adds new entries to a list file (story lines, items, quests...), the copy would never see
		/// them and screens would show raw keys like &lt;blacksmith.greeting&gt;. This appends any entry
		/// whose Key / Name / Id isn't in the copy yet, leaving every existing entry exactly as it is.
		/// </summary>
		private static void AddMissingEntries(string templatePath, string targetPath)
		{
			try
			{
				if (JsonNode.Parse(File.ReadAllText(templatePath)) is not JsonArray template) return;
				if (JsonNode.Parse(File.ReadAllText(targetPath)) is not JsonArray copy) return;

				// Only lists whose every entry has an identity can be merged safely.
				var templateIds = template.Select(EntryId).ToList();
				if (templateIds.Any(id => id == null)) return;

				var existing = new HashSet<string>(copy.Select(EntryId).OfType<string>(), StringComparer.OrdinalIgnoreCase);
				var missing = template.Where((_, i) => !existing.Contains(templateIds[i]!)).ToList();
				if (missing.Count == 0) return;

				foreach (var entry in missing)
					copy.Add(entry!.DeepClone());
				File.WriteAllText(targetPath, copy.ToJsonString(_writeOptions));
				Console.WriteLine($"[SEEDER] Added {missing.Count} new entries to {Path.GetFileName(targetPath)}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[SEEDER] Couldn't update {Path.GetFileName(targetPath)}: {ex.Message}");
			}
		}

		private static string? EntryId(JsonNode? entry)
		{
			if (entry is not JsonObject obj) return null;
			foreach (var field in new[] { "Key", "Name", "Id" })
				if (obj.TryGetPropertyValue(field, out var value) && value is JsonValue v && v.TryGetValue(out string? s) && !string.IsNullOrWhiteSpace(s))
					return field + ":" + s;
			return null;
		}

		public static void SeedInitialContent()
		{
			string destRoot = GameState.MasterPath;
			string sourceRoot;


			sourceRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "GameData", "Templates");


			// Log the path so you can see if the ".." logic is reaching your WSL folder
			Console.WriteLine($"[SEEDER] Searching: {Path.GetFullPath(sourceRoot)}");

			if (!Directory.Exists(sourceRoot)) return;

			foreach (DirectoryInfo sourceSubDir in new DirectoryInfo(sourceRoot).GetDirectories())
			{
				// Force lowercase to match your GameState.GetGlobalPath logic
				string folderName = sourceSubDir.Name.ToLower();
				DirectoryInfo targetSubDir = new DirectoryInfo(Path.Combine(destRoot, folderName));

				CopyFilesRecursively(sourceSubDir, targetSubDir);
			}
		}
	}
}
