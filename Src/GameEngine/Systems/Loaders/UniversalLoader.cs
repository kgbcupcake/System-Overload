using Pastel;
using Src.GameData.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System_Overload.Src.GameData.Components;

namespace System_Overload.Src.GameEngine.Systems.Loaders
{
	// FIX 1: Wrap the method in a static class named UniversalLoader
	public static class UniversalLoader
	{
		private static readonly JsonSerializerOptions _jsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter() }
		};

		public static List<T> LoadCollection<T>(string folderName)
		{
			List<T> masterList = new List<T>();

			// Live content lives in Documents/SystemOverload/Corrupted Data/<folder>, seeded from
			// GameData/Templates on first run and editable with MariesGui.
			string path = GameState.GetGlobalPath(folderName);

			if (!Directory.Exists(path)) return masterList;

			foreach (var file in Directory.GetFiles(path, "*.json"))
			{
				try
				{
					string json = File.ReadAllText(file);
					var items = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
					masterList.AddRange(items);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[LOAD ERROR] {Path.GetFileName(file)} in {folderName}: {ex.Message}");
				}
			}
			return masterList;
		}
	}

	// FIX 2: Now ItemRegistry can "see" UniversalLoader because it's a class above it
	public static class ItemRegistry
	{
		public static List<ItemData> AllItems { get; private set; } = new List<ItemData>();
		public static List<WeaponData> AllWeapons { get; private set; } = new List<WeaponData>();
		public static List<GemData> AllGems { get; private set; } = new List<GemData>();

		public static void Initialize()
		{
			// Add these strings inside the parentheses:
			AllItems = UniversalLoader.LoadCollection<ItemData>("Items");
			AllWeapons = UniversalLoader.LoadCollection<WeaponData>("Weapons");
			AllGems = UniversalLoader.LoadCollection<GemData>("Gems");

			Console.WriteLine($"[SYSTEM] Database Synchronized: {AllItems.Count} Items Loaded.");

		}
		/// <summary>
		/// Finds an item, weapon or gem by name and returns a fresh copy of it, or null.
		/// Used for loot drops, which can name anything in the registry.
		/// </summary>
		public static ItemData? CreateCopy(string name)
		{
			if (AllItems.Count == 0 && AllWeapons.Count == 0 && AllGems.Count == 0)
				Initialize();

			ItemData? template =
				(ItemData?)AllWeapons.FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				?? AllGems.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				?? AllItems.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

			if (template == null)
				return null;

			// Round-trip through JSON so the copy keeps its real type (weapon, gem) and all fields.
			var type = template.GetType();
			var copy = (ItemData?)JsonSerializer.Deserialize(JsonSerializer.Serialize(template, type, _copyOptions), type, _copyOptions);
			if (copy != null)
				copy.Amount = 1;
			return copy;
		}

		private static readonly JsonSerializerOptions _copyOptions = new()
		{
			Converters = { new JsonStringEnumConverter() }
		};

		public static ItemData? GetItem(string name)
		{
			// 1. Try to find the item (Case-Insensitive)
			var match = AllItems.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

			// 2. DEBUG: If it fails, tell us why
			if (match == null)
			{
				// This prints to your debug console so you can see the state of the game
				Console.WriteLine($"[DEBUG] Lookup for '{name}' failed. Registry count: {AllItems.Count}");
			}

			return match;
		}
	}
}