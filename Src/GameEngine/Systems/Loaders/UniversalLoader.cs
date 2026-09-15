using Pastel;
using Src.GameData.Components;
using System_Overload.Src.GameData.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System_Overload.Src.GameEngine.Systems.Loaders
{
	// FIX 1: Wrap the method in a static class named UniversalLoader
	public static class UniversalLoader
	{
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter() }
		};

		public static List<T> LoadCollection<T>(string folderName)
		{
			List<T> masterList = new List<T>();

			// GameState.MasterPath is the single source of truth for all loadable content
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
				catch { /* Handle error */ }
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
			AllItems = UniversalLoader.LoadCollection<ItemData>("items");
			AllWeapons = UniversalLoader.LoadCollection<WeaponData>("weapons");
			AllGems = UniversalLoader.LoadCollection<GemData>("gems");

			Console.WriteLine($"[SYSTEM] Database Synchronized: {AllItems.Count} Items Loaded.");

		}

		public static void ClearCache()
		{
			AllItems = new List<ItemData>();
			AllWeapons = new List<WeaponData>();
			AllGems = new List<GemData>();
		}

		public static ItemData? GetItem(string name)
		{
			// 1. Try to find the item (Case-Insensitive)
			var match = AllItems.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

			// 2. DEBUG: If it fails, tell us why (dev-mode only)
			if (match == null && GameState.IsDevMode)
			{
				Console.WriteLine($"[DEBUG] Lookup for '{name}' failed. Registry count: {AllItems.Count}");
			}

			return match;
		}
	}
}