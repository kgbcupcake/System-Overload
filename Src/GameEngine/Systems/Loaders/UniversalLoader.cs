using Pastel;
using Src.GameData.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace System_Overload.Src.GameEngine.Systems.Loaders
{
	// FIX 1: Wrap the method in a static class named UniversalLoader
	public static class UniversalLoader
	{
		public static List<T> LoadCollection<T>(string folderName)
		{
			List<T> masterList = new List<T>();

			// This looks in the build folder (bin/Debug/net8.0/Data/...)
			// This works on WSL, Windows, and for anyone who clones your repo!
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", folderName);

			if (!Directory.Exists(path)) return masterList;

			foreach (var file in Directory.GetFiles(path, "*.json"))
			{
				try
				{
					string json = File.ReadAllText(file);
					var items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
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
			// Add these strings inside the parentheses:
			AllItems = UniversalLoader.LoadCollection<ItemData>("Items");
			AllWeapons = UniversalLoader.LoadCollection<WeaponData>("Weapons");
			AllGems = UniversalLoader.LoadCollection<GemData>("Gems");

			Console.WriteLine($"[SYSTEM] Database Synchronized: {AllItems.Count} Items Loaded.");

		}
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