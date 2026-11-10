namespace Maries_DataModels
{
	/// <summary>
	/// A quest definition, loaded from the "quests" content folder. Quests don't run any gameplay
	/// themselves: the game reports events (an enemy defeated, a place discovered, a person spoken
	/// to) and every active quest whose current stage is waiting for that event moves forward.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("quests")]
	public class QuestData
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		/// <summary>"main" or "side". Main quests are listed first in the journal.</summary>
		public string Type { get; set; } = "side";
		public int Chapter { get; set; }
		public string Description { get; set; } = "";

		/// <summary>Story flags that must all be set before the quest can start.</summary>
		public List<string> Prerequisites { get; set; } = new();
		/// <summary>True to start the quest as soon as its prerequisites are met.</summary>
		public bool AutoStart { get; set; } = true;

		public List<QuestStage> Stages { get; set; } = new();
		public QuestRewards Rewards { get; set; } = new();

		/// <summary>Optional story key whose lines are shown when the quest is completed.</summary>
		[MariesGui.Contracts.MariesGuiRef("Key", "story")]
		public string CompletionStoryKey { get; set; } = "";

		/// <summary>Story flags set when the quest is completed. Other quests can require them.</summary>
		public List<string> CompletionFlags { get; set; } = new();
	}

	public class QuestStage
	{
		public string Id { get; set; } = "";
		/// <summary>What the journal shows for this stage, e.g. "Ask around Ashford".</summary>
		public string Description { get; set; } = "";
		/// <summary>Optional story key whose lines are shown when the stage begins.</summary>
		[MariesGui.Contracts.MariesGuiRef("Key", "story")]
		public string StoryKey { get; set; } = "";
		public List<QuestObjective> Objectives { get; set; } = new();
	}

	/// <summary>
	/// One thing to do. Type is the kind of event it waits for:
	/// talk_to (an npc Id), examine (a point_of_interest Id), discover_location (a floor Id or
	/// "ashford"), collect_item (an item name), defeat_enemy (an enemy name), or flag (done once
	/// that story flag is set). Target is the id or name it waits for.
	/// </summary>
	public class QuestObjective
	{
		public string Type { get; set; } = "";
		[MariesGui.Contracts.MariesGuiRef("Name", "monsters", "bosses", When = "Type=defeat_enemy", LooseMatch = true)]
		[MariesGui.Contracts.MariesGuiRef("Name", "items", "weapons", "armor", "gems", When = "Type=collect_item", LooseMatch = true)]
		[MariesGui.Contracts.MariesGuiRef("Id", "floors", When = "Type=discover_location", LooseMatch = true, AlsoAllowed = "ashford")]
		[MariesGui.Contracts.MariesGuiRef("Id", "npcs", When = "Type=talk_to", LooseMatch = true)]
		[MariesGui.Contracts.MariesGuiRef("Id", "points_of_interest", When = "Type=examine", LooseMatch = true)]
		public string Target { get; set; } = "";
		public int Count { get; set; } = 1;
		public bool Required { get; set; } = true;
		/// <summary>What the journal shows, e.g. "Speak with Edrin". Defaults to type and target.</summary>
		public string Description { get; set; } = "";
		/// <summary>
		/// Optional story key shown when a conversation or examination completes this objective,
		/// e.g. what Edrin says when you ask him about {loved}.
		/// </summary>
		[MariesGui.Contracts.MariesGuiRef("Key", "story")]
		public string StoryKey { get; set; } = "";
	}

	public class QuestRewards
	{
		public int Experience { get; set; }
		public int Gold { get; set; }
		/// <summary>Item names from the item data, given on completion.</summary>
		[MariesGui.Contracts.MariesGuiRef("Name", "items", "weapons", "armor", "gems")]
		public List<string> Items { get; set; } = new();
	}

	/// <summary>A hero's saved progress through one quest.</summary>
	public class QuestProgress
	{
		public string QuestId { get; set; } = "";
		public int StageIndex { get; set; }
		/// <summary>Progress per objective of the current stage, by objective position.</summary>
		public List<int> ObjectiveCounts { get; set; } = new();
		public bool Completed { get; set; }
	}
}
