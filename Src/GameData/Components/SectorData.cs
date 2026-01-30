using System.Collections.Generic;
using System.Linq;

namespace System_Overload.Src.GameData.Entities
{
	public class SectorData
	{
		public string Id { get; set; } = "";
		public string Title { get; set; } = "";
		public string Description { get; set; } = "";
		public string Difficulty { get; set; } = "STABLE";
		public int CompletionCredits { get; set; }
		public int CompletionExp { get; set; }
		public List<string> Tags { get; set; } = new();
		public List<SectorStage> Stages { get; set; } = new();

		public string BossId => Stages.LastOrDefault(s => !string.IsNullOrEmpty(s.EnemyId))?.EnemyId ?? "";
	}

	public class SectorStage
	{
		public string StoryText { get; set; } = "";
		public string EnemyId { get; set; } = "";
		public string HexColor { get; set; } = "#00FFFF";
	}
}