using Maries_DataModels;
using Maries_Loaders;
using static Maries_DataModels.PlayerData;

namespace DungeonAdventures.Src.GameEngine.Systems
{
	/// <summary>
	/// Gives a new hero their background's head start: its bonus attribute points (on top of the ones spent
	/// in training), its gold and its items. Called once, at the end of character creation.
	/// </summary>
	public static class BackgroundPerks
	{
		public static void Apply(loadPlayer player)
		{
			var background = OriginLoader.Background(player.Background);
			if (background == null) return;

			AddPoints(player, background.Attribute, background.Points);
			player.AddGold(background.Gold);
			foreach (string name in background.Items)
				if (ItemRegistry.CreateCopy(name) is { } item)
					PlayerInventory.Give(player, item);
		}

		private static void AddPoints(loadPlayer player, string attribute, int points)
		{
			switch (attribute.ToLowerInvariant())
			{
				case "vitality": player.VitalityPoints += points; break;
				case "strength": player.StrengthPoints += points; break;
				case "fortitude": player.FortitudePoints += points; break;
				case "wisdom": player.WisdomPoints += points; break;
			}
		}
	}
}
