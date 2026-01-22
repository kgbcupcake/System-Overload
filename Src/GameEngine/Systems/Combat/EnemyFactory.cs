using System_Overload.Src.GameData.Entities;
using System_Overload.Src.GameEngine.Combat;
using System_Overload.Src.GameData.Loaders; // Added to access DronesLoader

public static class EnemyFactory
{
	/// <summary>
	/// Fetches a hostile program/drone from the Data Loaders.
	/// </summary>
	public static Combatant CreateEnemy(string enemyId)
	{
		// NO MORE PLACEHOLDERS: We now use the DronesLoader to fetch real data
		var data = DronesLoader.GetById(enemyId);

		if (data != null)
		{
			return new Combatant(
				data.Name,
				data.MaxHealth,
				data.AttackPower,
				data.Defense
			);
		}

		// Fallback for generic security protocols if ID is not found
		return new Combatant("Standard_Security_Daemon", 50, 12, 5);
	}

	/// <summary>
	/// Generates a Combatant wrapper for the Player's current state.
	/// </summary>
	public static Combatant CreatePlayer(PlayerData.loadPlayer player)
	{
		// 1. Create the combatant.
		// Use (int) casting to fix the errors since Combatant expects integers.
		var combatant = new Combatant(
			player.PlayerName,
			(int)player.GetTotalMaxHealth(), // Uses your new logic with Gems
			(int)player.GetTotalDamage(),    // Uses your new Logic with Sockets
			(int)player.ArmorValue           // Uses your GetTotalDefense() bridge
		);

		// 2. Sync Current State
		// Ensure the combatant starts with the player's current health, not just the max.
		combatant.Inventory = player.Inventory;
		combatant.Health = player.Health;

		return combatant;
	}
}