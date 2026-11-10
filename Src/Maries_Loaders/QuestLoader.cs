using Maries_DataModels;

namespace Maries_Loaders
{
	/// <summary>Reads quest definitions from the "quests" content folder, keyed by quest id.</summary>
	public static class QuestLoader
	{
		private static Dictionary<string, QuestData> _quests = new(StringComparer.OrdinalIgnoreCase);

		public static IReadOnlyCollection<QuestData> All => _quests.Values;

		public static void LoadAll()
		{
			_quests = new Dictionary<string, QuestData>(StringComparer.OrdinalIgnoreCase);
			foreach (var quest in UniversalLoader.LoadCollection<QuestData>("quests"))
			{
				if (!string.IsNullOrWhiteSpace(quest.Id))
					_quests[quest.Id] = quest;
			}
		}

		public static QuestData? Get(string id) => _quests.TryGetValue(id, out var quest) ? quest : null;
	}
}
