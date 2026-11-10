using Maries_DataModels;

namespace DungeonAdventures.Src.GameEngine.Combat
{
	public static class CombatantMapper
	{
		public static Combatant FromPlayer(PlayerData.loadPlayer player)
		{
			return new Combatant(
				player.PlayerName,
				(int)player.GetTotalMaxHealth(),
				(int)player.GetTotalDamage(),
				player.ArmorValue
			);
		}
	}
}
