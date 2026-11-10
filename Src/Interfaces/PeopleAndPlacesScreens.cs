using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine;
using DungeonAdventures.Src.GameEngine.Systems;
using DungeonAdventures.Src.Interfaces.UI;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using static System.Console;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>
	/// Talking to people and examining things. Both are driven by data (the "npcs" and
	/// "points_of_interest" folders) and report "talk_to" / "examine" events to the quest system.
	/// </summary>
	internal static class PeopleAndPlacesScreens
	{
		private const int BoxWidth = 92;

		/// <summary>Lets the hero pick someone at this location to talk to.</summary>
		public static void TalkMenu(string location, string title)
		{
			var player = GameState.CurrentPlayer;
			if (player == null) return;

			while (true)
			{
				var people = PeopleAndPlacesLoader.NpcsAt(location, flag => QuestSystem.HasFlag(player, flag));
				if (people.Count == 0)
				{
					Alerts.QuickAlert("There's no one here to talk to.");
					return;
				}

				var labels = people
					.Select(n => string.IsNullOrWhiteSpace(n.Role) ? Story.Format(n.Name) : $"{Story.Format(n.Name)} - {Story.Format(n.Role)}")
					.ToList();
				int choice = Choose(title, labels, "Choose someone to talk to | [ESC] Back");
				if (choice < 0) return;

				Talk(people[choice]);
			}
		}

		/// <summary>
		/// Talks to one person. If a quest was waiting for this conversation, the hero hears that
		/// objective's story lines; otherwise the person's greeting.
		/// </summary>
		public static void Talk(NpcData npc)
		{
			var player = GameState.CurrentPlayer;
			if (player == null) return;

			var finished = QuestSystem.Notify(player, "talk_to", npc.Id);
			var lines = StoryLinesOf(finished);
			if (lines.Count == 0 && !string.IsNullOrWhiteSpace(npc.GreetingKey))
				lines = Story.Lines(npc.GreetingKey);
			if (lines.Count == 0)
				lines = new List<string> { $"{Story.Format(npc.Name)} has nothing to say right now." };

			QuestScreens.ShowPage(Story.Format(npc.Name).ToUpperInvariant(), lines);
			AfterAction();
		}

		/// <summary>
		/// Lets the hero examine things at this location. Returns straight away, without a menu,
		/// when there's nothing here.
		/// </summary>
		public static void LookAround(string location, string title)
		{
			var player = GameState.CurrentPlayer;
			if (player == null) return;

			while (true)
			{
				var places = PeopleAndPlacesLoader.PlacesAt(location, flag => QuestSystem.HasFlag(player, flag));
				if (places.Count == 0) return;

				var labels = places
					.Select(p => QuestSystem.HasFlag(player, ExaminedFlag(p)) ? $"{Story.Format(p.Name)} (examined)" : Story.Format(p.Name))
					.ToList();
				int choice = Choose(title, labels, "Choose something to examine | [ESC] Leave");
				if (choice < 0) return;

				Examine(places[choice]);
			}
		}

		/// <summary>
		/// Examines one thing: shows its lines, hands over its item the first time, and reports the
		/// "examine" event (whose objectives can add more lines).
		/// </summary>
		public static void Examine(PointOfInterest place)
		{
			var player = GameState.CurrentPlayer;
			if (player == null) return;

			var lines = string.IsNullOrWhiteSpace(place.StoryKey)
				? new List<string>()
				: Story.Lines(place.StoryKey);

			bool firstTime = !QuestSystem.HasFlag(player, ExaminedFlag(place));
			if (firstTime)
			{
				// Set before reporting anything, so the item can only ever be handed out once.
				player.StoryFlags.Add(ExaminedFlag(place));

				if (!string.IsNullOrWhiteSpace(place.GivesItem) && ItemRegistry.CreateCopy(place.GivesItem) is { } item)
				{
					PlayerInventory.Give(player, item);
					lines.Add($"» You take: {item.Name}");
				}
			}

			var finished = QuestSystem.Notify(player, "examine", place.Id);
			lines.AddRange(StoryLinesOf(finished));
			if (lines.Count == 0)
				lines.Add("Nothing more to see here.");

			QuestScreens.ShowPage(Story.Format(place.Name).ToUpperInvariant(), lines);
			AfterAction();
		}

		// Story flag remembering that the hero has examined something (so its item isn't given twice).
		private static string ExaminedFlag(PointOfInterest place) => $"examined_{place.Id}";

		private static List<string> StoryLinesOf(IEnumerable<QuestObjective> objectives) =>
			objectives
				.Where(o => !string.IsNullOrWhiteSpace(o.StoryKey))
				.SelectMany(o => Story.Lines(o.StoryKey))
				.ToList();

		// Saves the hero and shows any quest news the action caused.
		private static void AfterAction()
		{
			SaveGame.Save();
			QuestScreens.ShowNotices();
			Clear();
		}

		// A simple framed menu with a "BACK" row. Returns the chosen index, or -1 for back/escape.
		private static int Choose(string title, List<string> labels, string hint)
		{
			var rows = labels.Append("BACK").Select(l => l.PadRight(BoxWidth - 2)).ToList();
			int selected = 0;

			Clear();
			while (true)
			{
				GameUiEngine.DrawDynamicFrame(title: title, lines: rows, hint: hint,
					boxWidth: BoxWidth, startY: 5, selectedIndex: selected);

				var key = MouseInput.ReadMenuKey(ref selected, rows.Count);
				switch (key)
				{
					case ConsoleKey.UpArrow:
					case ConsoleKey.DownArrow:
						selected = MenuNavigator.HandleVerticalNav(key, selected, rows.Count);
						break;
					case ConsoleKey.Enter:
						Clear();
						return selected == rows.Count - 1 ? -1 : selected;
					case ConsoleKey.Escape:
						Clear();
						return -1;
				}
			}
		}
	}
}
