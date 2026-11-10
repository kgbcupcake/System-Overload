using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine.Systems;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using static System.Console;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>The quest pop-ups ("New quest", "Quest updated", "Quest complete") and the journal.</summary>
	internal static class QuestScreens
	{
		private const int ScreenWidth = 94;
		private const int TextWidth = 70;
		private const int TextLeft = (ScreenWidth - TextWidth) / 2;
		private const int LastTextRow = 19;

		private const string GoldColor = "#FFAB00";
		private const string LeatherColor = "#8B5A2B";
		private const string BodyColor = "#CBB994";
		private const string DimColor = "#777777";
		private const string DoneColor = "#00FF41";

		/// <summary>Shows every queued quest notice, one screen each, then clears the screen.</summary>
		public static void ShowNotices()
		{
			if (QuestSystem.Notices.Count == 0) return;

			while (QuestSystem.Notices.TryDequeue(out var notice))
				ShowPage(notice.Title, notice.Lines);
			Clear();
		}

		/// <summary>
		/// One screen of text under a gold title, wrapped, waiting for a key. Lines starting with
		/// "»" are drawn in gold. Used for quest notices, conversations and examining things.
		/// </summary>
		public static void ShowPage(string title, IEnumerable<string> lines)
		{
			Clear();
			CursorVisible = false;
			UiEngine.DrawCentered(title.Pastel(GoldColor), 5);
			UiEngine.DrawCentered(new string('─', TextWidth).Pastel(LeatherColor), 6);

			var rows = new List<string>();
			foreach (var line in lines)
			{
				if (rows.Count > 0) rows.Add("");
				rows.AddRange(StoryPages.Wrap(line, TextWidth));
			}

			int row = 8;
			foreach (var text in rows)
			{
				if (row > LastTextRow) break;
				SetCursorPosition(TextLeft, row++);
				Write(text.StartsWith('»') ? text.Pastel(GoldColor) : text.Pastel(BodyColor));
			}

			UiEngine.DrawCentered("Press any key (or click) to continue...".Pastel(DimColor), 21);
			MouseInput.WaitForAnyInput();
		}

		/// <summary>The hero's quest journal: active quests with their objectives, then finished ones.</summary>
		public static void Journal()
		{
			var player = GameState.CurrentPlayer;
			if (player == null) return;

			Clear();
			CursorVisible = false;
			UiEngine.DrawCentered("QUEST JOURNAL".Pastel(GoldColor), 1);
			UiEngine.DrawCentered(new string('─', TextWidth).Pastel(LeatherColor), 2);

			int row = 4;
			void Line(string text)
			{
				if (row > LastTextRow) return;
				SetCursorPosition(TextLeft, row++);
				Write(text);
			}

			// Main quests first, then by chapter, so the story thread is always on top.
			var entries = player.Quests
				.Select(p => (Progress: p, Quest: QuestLoader.Get(p.QuestId)))
				.Where(e => e.Quest != null)
				.OrderBy(e => e.Quest!.Type.Equals("main", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
				.ThenBy(e => e.Quest!.Chapter)
				.ToList();

			var active = entries.Where(e => !e.Progress.Completed).ToList();
			var finished = entries.Where(e => e.Progress.Completed).ToList();

			if (active.Count == 0)
				Line(Story.Line("quest.journal_empty").Pastel(DimColor));

			foreach (var (progress, quest) in active)
			{
				var stage = QuestSystem.CurrentStage(quest, progress);
				Line($"◆ {QuestSystem.Name(quest!)}".Pastel(GoldColor));
				if (stage == null) continue;

				Line($"  {QuestSystem.Describe(stage)}".Pastel(BodyColor));
				for (int i = 0; i < stage.Objectives.Count; i++)
				{
					var objective = stage.Objectives[i];
					bool done = QuestSystem.IsObjectiveDone(player, progress, objective, i);
					string count = objective.Count > 1 ? $" ({QuestSystem.Count(progress, i)}/{objective.Count})" : "";
					string optional = objective.Required ? "" : " (optional)";
					string text = $"    {(done ? "[x]" : "[ ]")} {QuestSystem.Describe(objective)}{count}{optional}";
					Line(done ? text.Pastel(DoneColor) : text.Pastel(DimColor));
				}
				row++;
			}

			if (finished.Count > 0)
			{
				Line("COMPLETED".Pastel(LeatherColor));
				foreach (var (_, quest) in finished)
					Line($"  ✓ {QuestSystem.Name(quest!)}".Pastel(DimColor));
			}

			UiEngine.DrawCentered("Press any key (or click) to return...".Pastel(DimColor), 21);
			MouseInput.WaitForAnyInput();
			Clear();
		}
	}
}
