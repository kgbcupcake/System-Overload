using System_Overload.GameEngine.Systems.Loaders;
using System_Overload.Src.GameData.Entities;
using System_Overload.Src.GameEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using static System_Overload.Src.GameData.Entities.PlayerData;

namespace System_Overload.Src.GameData.Components
{
	public class GameState
	{
		private static readonly string UserDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		public static readonly string MasterPath = Path.Combine(UserDocuments, "SystemOverload", "Corrupted Data");


		// 1. Data registration
		public static readonly string[] DataFolders = {
			"profiles",
			"items",
			"weapons",
			"gems",
			"admins",
			"sectors",
			"drones",
			"settings",
			"attachments",
			"modules",

		};


		public static loadPlayer CurrentPlayer { get; set; } = null;
		public static SectorData? CurrentSector { get; set; } = null; // FIXED: Removed SectorLoader prefix
		public static AdminData? CurrentAdmin { get; set; } = null;

		// 4. System States
		public static bool IsDevMode { get; set; } = true;
		public static string BuildVersion { get; set; } = "2.0.5";
		public static bool StateDirty { get; set; } = false;
		public static string ActiveSavePath { get; set; } = "";
		public static string DevIteration { get; set; } = "ALPHA-V2";

		public static string? CurrentLocation { get; set; } = null;
		public static bool SceneChangeRequested { get; set; } = false;

		public static string GetGlobalPath(string folder) => Path.Combine(MasterPath, folder.ToLower());

		public static void EnsureDirectories()
		{
			// Create the Master Path first
			if (!Directory.Exists(MasterPath)) Directory.CreateDirectory(MasterPath);

			// Create every subfolder (sectors, profiles, etc.)
			foreach (var folder in DataFolders)
			{
				string subPath = GetGlobalPath(folder);
				if (!Directory.Exists(subPath)) Directory.CreateDirectory(subPath);
			}
		}

		public static void Sync()
		{
			if (CurrentPlayer == null || string.IsNullOrWhiteSpace(CurrentPlayer.PlayerName)) return;
			string safeName = CurrentPlayer.PlayerName.Replace(" ", "_").ToLower();
			string fileName = $"{safeName}.json";
			SaveGame.SaveData(fileName, CurrentPlayer, "profiles");
		}

		public static string GetActiveProfileFolder() => Path.Combine(MasterPath, "profiles");

		public sealed class CombatContext
		{
			public PlayerData Player { get; }
			public EnemyData Enemy { get; }
			public bool IsPlayerTurn { get; set; }

			public CombatContext(PlayerData player, EnemyData enemy)
			{
				Player = player;
				Enemy = enemy;
				IsPlayerTurn = true;
			}
		}
	}
}