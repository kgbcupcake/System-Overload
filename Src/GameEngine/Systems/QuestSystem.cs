using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using static Maries_DataModels.PlayerData;

namespace DungeonAdventures.Src.GameEngine.Systems
{
	/// <summary>Something the player should be told about: a quest started, moved on or finished.</summary>
	public sealed record QuestNotice(string Title, List<string> Lines);

	/// <summary>
	/// Runs the data-driven quests in the "quests" folder against the hero's saved progress.
	///
	/// Gameplay code never manages a quest directly. It reports what happened with
	/// <see cref="Notify"/> (an enemy defeated, a place discovered...) or sets a story flag with
	/// <see cref="SetFlag"/>, and every active quest waiting for that moves forward on its own.
	/// Quests start once all their prerequisite flags are set, and finishing one sets its
	/// completion flags, which can start the next.
	///
	/// This class doesn't draw anything: it queues <see cref="Notices"/> for a screen to show,
	/// and the hero is saved by whoever called it, as with every other change to the hero.
	/// </summary>
	public static class QuestSystem
	{
		public static Queue<QuestNotice> Notices { get; } = new();

		// ------------------------------------------------------------- flags

		public static bool HasFlag(loadPlayer player, string flag) =>
			player.StoryFlags.Contains(flag, StringComparer.OrdinalIgnoreCase);

		/// <summary>Sets a story flag, then lets quests react: flag objectives and new quests.</summary>
		public static void SetFlag(loadPlayer player, string flag)
		{
			if (string.IsNullOrWhiteSpace(flag) || HasFlag(player, flag)) return;
			player.StoryFlags.Add(flag);

			Notify(player, "flag", flag);
			StartAvailable(player);
		}

		// ------------------------------------------------------------ events

		/// <summary>
		/// Reports a game event. <paramref name="type"/> is an objective type (talk_to, examine,
		/// discover_location, collect_item, defeat_enemy, flag); <paramref name="target"/> is
		/// matched loosely, so "Forest Wolf" matches the target "forest_wolf".
		/// Returns the objectives this event just finished, so a screen can show their StoryKey
		/// (for example what someone says when you talk to them).
		/// </summary>
		public static List<QuestObjective> Notify(loadPlayer player, string type, string target, int amount = 1)
		{
			var finished = new List<QuestObjective>();
			if (amount <= 0) return finished;

			// A snapshot, because finishing a quest can start new ones and add to the list.
			foreach (var progress in player.Quests.Where(q => !q.Completed).ToList())
			{
				var quest = QuestLoader.Get(progress.QuestId);
				var stage = CurrentStage(quest, progress);
				if (quest == null || stage == null) continue;

				bool changed = false;
				for (int i = 0; i < stage.Objectives.Count; i++)
				{
					var objective = stage.Objectives[i];
					if (!Matches(objective.Type, type) || !Matches(objective.Target, target)) continue;
					if (objective.Type.Equals("flag", StringComparison.OrdinalIgnoreCase)) { changed = true; continue; }

					int current = Count(progress, i);
					if (current >= objective.Count) continue;
					progress.ObjectiveCounts[i] = Math.Min(objective.Count, current + amount);
					if (progress.ObjectiveCounts[i] >= objective.Count) finished.Add(objective);
					changed = true;
				}

				if (changed) TryAdvance(player, quest, progress);
			}
			return finished;
		}

		// ------------------------------------------------------------ quests

		/// <summary>Starts every auto-start quest whose prerequisites are all met.</summary>
		public static void StartAvailable(loadPlayer player)
		{
			foreach (var quest in QuestLoader.All.Where(q => q.AutoStart).ToList())
			{
				if (IsStarted(player, quest.Id)) continue;
				if (quest.Prerequisites.All(flag => HasFlag(player, flag)))
					Start(player, quest.Id);
			}
		}

		/// <summary>Starts a quest, whether or not it auto-starts. Does nothing if already started.</summary>
		public static void Start(loadPlayer player, string questId)
		{
			var quest = QuestLoader.Get(questId);
			if (quest == null || quest.Stages.Count == 0 || IsStarted(player, quest.Id)) return;

			var progress = new QuestProgress { QuestId = quest.Id };
			player.Quests.Add(progress);

			Notices.Enqueue(new QuestNotice(
				Story.Line("quest.started", ("quest", Name(quest))),
				StageLines(quest.Stages[0], quest.Description)));

			EnterStage(player, quest, progress, 0);
		}

		public static bool IsStarted(loadPlayer player, string questId) =>
			player.Quests.Any(q => q.QuestId.Equals(questId, StringComparison.OrdinalIgnoreCase));

		public static bool IsCompleted(loadPlayer player, string questId) =>
			player.Quests.Any(q => q.Completed && q.QuestId.Equals(questId, StringComparison.OrdinalIgnoreCase));

		/// <summary>Whether one objective of the quest's current stage is done.</summary>
		public static bool IsObjectiveDone(loadPlayer player, QuestProgress progress, QuestObjective objective, int index) =>
			objective.Type.Equals("flag", StringComparison.OrdinalIgnoreCase)
				? HasFlag(player, objective.Target)
				: Count(progress, index) >= objective.Count;

		public static int Count(QuestProgress progress, int index)
		{
			while (progress.ObjectiveCounts.Count <= index) progress.ObjectiveCounts.Add(0);
			return progress.ObjectiveCounts[index];
		}

		public static QuestStage? CurrentStage(QuestData? quest, QuestProgress progress) =>
			quest != null && progress.StageIndex >= 0 && progress.StageIndex < quest.Stages.Count
				? quest.Stages[progress.StageIndex]
				: null;

		/// <summary>What the journal shows for an objective, with story tokens such as {loved} filled in.</summary>
		public static string Describe(QuestObjective objective) =>
			!string.IsNullOrWhiteSpace(objective.Description)
				? Story.Format(objective.Description)
				: $"{objective.Type.Replace('_', ' ')}: {objective.Target.Replace('_', ' ')}";

		/// <summary>A quest's name with story tokens filled in.</summary>
		public static string Name(QuestData quest) => Story.Format(quest.Name);

		/// <summary>A stage's journal line with story tokens filled in.</summary>
		public static string Describe(QuestStage stage) => Story.Format(stage.Description);

		// ----------------------------------------------------------- internal

		private static void EnterStage(loadPlayer player, QuestData quest, QuestProgress progress, int stageIndex)
		{
			progress.StageIndex = stageIndex;
			progress.ObjectiveCounts = new List<int>();
			// A stage can be done the moment it starts, e.g. a flag objective that's already set.
			TryAdvance(player, quest, progress);
		}

		private static void TryAdvance(loadPlayer player, QuestData quest, QuestProgress progress)
		{
			var stage = CurrentStage(quest, progress);
			if (stage == null || progress.Completed) return;

			bool done = stage.Objectives
				.Select((objective, i) => (objective, i))
				.Where(x => x.objective.Required)
				.All(x => IsObjectiveDone(player, progress, x.objective, x.i));
			if (!done) return;

			int next = progress.StageIndex + 1;
			if (next >= quest.Stages.Count)
			{
				Complete(player, quest, progress);
				return;
			}

			Notices.Enqueue(new QuestNotice(
				Story.Line("quest.updated", ("quest", Name(quest))),
				StageLines(quest.Stages[next], null)));
			EnterStage(player, quest, progress, next);
		}

		private static void Complete(loadPlayer player, QuestData quest, QuestProgress progress)
		{
			progress.Completed = true;
			progress.StageIndex = quest.Stages.Count;

			var lines = new List<string>();
			if (!string.IsNullOrWhiteSpace(quest.CompletionStoryKey)) lines.AddRange(Story.Lines(quest.CompletionStoryKey));
			var rewards = quest.Rewards;
			if (rewards.Experience > 0) { player.AddXP(rewards.Experience); lines.Add($"XP: +{rewards.Experience}"); }
			if (rewards.Gold > 0) { player.AddGold(rewards.Gold); lines.Add($"Gold: +{rewards.Gold}"); }
			foreach (var itemName in rewards.Items)
			{
				var item = ItemRegistry.CreateCopy(itemName);
				if (item == null) continue;
				player.Inventory.Add(item);
				lines.Add($"Received: {item.Name}");
			}

			Notices.Enqueue(new QuestNotice(Story.Line("quest.completed", ("quest", Name(quest))), lines));

			foreach (var flag in quest.CompletionFlags)
				SetFlag(player, flag);
		}

		/// <summary>The lines a notice shows for a stage: its story text, then what to do next.</summary>
		private static List<string> StageLines(QuestStage stage, string? intro)
		{
			var lines = new List<string>();
			if (!string.IsNullOrWhiteSpace(stage.StoryKey)) lines.AddRange(Story.Lines(stage.StoryKey));
			else if (!string.IsNullOrWhiteSpace(intro)) lines.Add(Story.Format(intro));
			if (!string.IsNullOrWhiteSpace(stage.Description)) lines.Add("» " + Describe(stage));
			return lines;
		}

		private static bool Matches(string a, string b) =>
			Normalize(a).Equals(Normalize(b), StringComparison.Ordinal);

		private static string Normalize(string s) =>
			s.Trim().ToLowerInvariant().Replace(' ', '_').Replace('-', '_').Replace("'", "");
	}
}
