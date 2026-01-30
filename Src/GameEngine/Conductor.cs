using System_Overload.Src.GameData.Components;
using static System_Overload.Src.GameData.Entities.PlayerData;

namespace System_Overload.Src.GameEngine
{
	/// <summary>
	/// Streamlined Conductor: Manages game data strictly in the User Documents folder.
	/// This prevents MSBuild from seeing JSON files as resources and crashing.
	/// </summary>
	public class Conductor
	{
		private readonly string _masterPath = GameState.MasterPath;
		public Conductor()
		{
			InitializeDirectories();
		}

		private void InitializeDirectories()
		{
			GameState.EnsureDirectories(); // Simply call GameState.EnsureDirectories()
			Dev.ContentSeeder.SeedInitialContent();
		}


		// Fixes the Player Creator issue
		public void CreateNewPlayer(string name, string heroClass)
		{
			var newHero = new loadPlayer
			{
				PlayerName = name,
				PlayerClass = heroClass,
				Level = 1,
				Health = 100,
				HitPoints = 100
			};

			GameState.CurrentPlayer = newHero;
			GameState.Sync(); // Immediately writes to /profiles/
		}
	}
}
        