using System_Overload.Src.GameData.Entities;

namespace System_Overload.Src.GameEngine.Combat
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
