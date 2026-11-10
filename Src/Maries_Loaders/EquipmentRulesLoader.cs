using Maries_DataModels;

namespace Maries_Loaders
{
	/// <summary>
	/// Loads gear slots and socket rules from the "equipment" content folder into
	/// EquipmentRules.Current. If several files exist their lists are merged, and the last
	/// file read sets the costs.
	/// </summary>
	public static class EquipmentRulesLoader
	{
		public static EquipmentRules LoadAll()
		{
			var files = UniversalLoader.LoadCollection<EquipmentRules>("equipment");
			var rules = new EquipmentRules();

			foreach (var file in files)
			{
				rules.EquipSlots.AddRange(file.EquipSlots);
				rules.SocketHosts.AddRange(file.SocketHosts);
				rules.GemEffects.AddRange(file.GemEffects);
				rules.SetGemCost = file.SetGemCost;
				rules.RemoveGemCost = file.RemoveGemCost;
				rules.RemoveGemDestroysGem = file.RemoveGemDestroysGem;
				rules.PunchBaseCost = file.PunchBaseCost;
				rules.PunchCostPerSocket = file.PunchCostPerSocket;
			}

			if (files.Count == 0 && LoaderConfig.DevMode)
				Console.WriteLine("[DEBUG] No equipment rules found: gear sockets are disabled.");

			EquipmentRules.Current = rules;
			return rules;
		}
	}
}
