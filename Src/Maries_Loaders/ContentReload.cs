namespace Maries_Loaders
{
	/// <summary>
	/// Re-reads every content category from Realm Data while the game is running, so changes
	/// saved in MariesGui show up without restarting. Used by the dev-only "RELOAD CONTENT"
	/// option in the village.
	/// </summary>
	public static class ContentReload
	{
		public static void ReloadAll()
		{
			// Loaders that cache on first use: clear them so the next lookup reads from disk.
			MonsterLoader.ClearCache();
			ShopLoader.ClearCache();
			BossLoader.ClearCache();
			FloorLoader.ClearCache();

			// Loaders that the game fills once at startup: run them again now.
			ItemRegistry.Initialize();
			EquipmentRulesLoader.LoadAll();
			BossLoader.LoadAll();
			FloorLoader.LoadAll();
			Story.LoadAll();
			QuestLoader.LoadAll();
			PeopleAndPlacesLoader.LoadAll();
		}
	}
}
