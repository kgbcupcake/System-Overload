using System.Collections.Generic;
using System.Linq;

namespace Maries_DataModels
{
	[MariesGui.Contracts.MariesGuiContent("floors")]
	public class FloorData
	{
		public string Id { get; set; } = "";
		public string Title { get; set; } = "";
		public string Description { get; set; } = "";
		public string Difficulty { get; set; } = "EASY";
		/// <summary>The room style its scenes are painted in (crypt, ember, catacombs, throne). Empty: guessed from the id and title.</summary>
		public string Room { get; set; } = "";
		/// <summary>Shown when the floor is cleared. Falls back to the "dungeon.complete" story line.</summary>
		public string CompletionText { get; set; } = "";
		public int CompletionGold { get; set; }
		public int CompletionExp { get; set; }
		public List<string> Tags { get; set; } = new();
		public List<FloorStage> Stages { get; set; } = new();

		public string BossId => Stages.LastOrDefault(s => !string.IsNullOrEmpty(s.EnemyId))?.EnemyId ?? "";
	}

	public class FloorStage
	{
		public string StoryText { get; set; } = "";
		[MariesGui.Contracts.MariesGuiRef("Id", "monsters", "bosses")]
		public string EnemyId { get; set; } = "";
		public string HexColor { get; set; } = "#FFAB00";
		/// <summary>
		/// The scene played behind the line: "walk" (the hero walks in), "squeeze" (through a cracked tombstone),
		/// "none" (plain text). Empty: "squeeze" when the line talks of squeezing, a crack or a tombstone, else "walk".
		/// </summary>
		public string Scene { get; set; } = "";
	}
}