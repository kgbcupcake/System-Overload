using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine.Systems;
using DungeonAdventures.Src.Interfaces.UI;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using static System.Console;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>
	/// "A Life Before": right after character creation, the player names the person who vanished,
	/// says what they were to each other, and relives one last memory. The choices are saved on
	/// the hero and fill the {loved}, {relation} and {relation_memory} tokens in story text.
	/// </summary>
	internal static class LifeBefore
	{
		private const int MaxNameLength = 20;
		private const string GoldColor = "#FFAB00";
		private const string BodyColor = "#CBB994";
		private const string DimColor = "#777777";

		public static void Start()
		{
			var player = GameState.CurrentPlayer;
			if (player == null) return;

			player.LovedOneName = AskName();
			player.LovedOneRelation = AskRelation();

			StoryPages.Play("memory");
		}

		private static string AskName()
		{
			Clear();
			CursorVisible = false;
			UiEngine.DrawCentered(Story.Line("life_before.title").Pastel(GoldColor), 3);

			int row = 6;
			foreach (var line in Story.Lines("life_before.intro"))
				UiEngine.DrawCentered(line.Pastel(BodyColor), row++);

			string defaultName = StoryTokens.LovedOneName(null);
			UiEngine.DrawCentered(Story.Line("life_before.name_prompt").Pastel("#FFFFFF"), row + 2);
			UiEngine.DrawCentered($"(Press Enter for {defaultName})".Pastel(DimColor), row + 3);

			SetCursorPosition(94 / 2 - MaxNameLength / 2, row + 5);
			CursorVisible = true;
			ForegroundColor = ConsoleColor.Yellow;
			string? input;
			// Mouse mode is paused while typing so clicks can't type junk into the name.
			using (MouseInput.Suspend())
				input = ReadLine();
			ResetColor();
			CursorVisible = false;

			string name = (input ?? "").Trim();
			if (name.Length > MaxNameLength) name = name[..MaxNameLength].TrimEnd();
			// Empty is saved as empty: the story falls back to the default name from the data.
			return name == defaultName ? "" : name;
		}

		private static string AskRelation()
		{
			var ids = Story.Raw("life_before.relation_ids").ToList();
			if (ids.Count == 0) return "";

			const int boxWidth = 50;
			var labels = ids
				.Select(id => Story.Raw(StoryTokens.RelationKey(id)).FirstOrDefault() ?? id)
				.Select(label => UiEngine.PadAnsiStringWithCenter(label, boxWidth - 2))
				.ToList();

			int selected = 0;
			Clear();
			UiEngine.DrawCentered(Story.Line("life_before.title").Pastel(GoldColor), 3);
			UiEngine.DrawCentered(Story.Line("life_before.relation_prompt").Pastel("#FFFFFF"), 6);

			while (true)
			{
				GameUiEngine.DrawDynamicFrame("", labels, "Use arrow keys or click to choose", boxWidth: boxWidth, startY: 9, selectedIndex: selected);

				var key = MouseInput.ReadMenuKey(ref selected, labels.Count);
				switch (key)
				{
					case ConsoleKey.UpArrow:
					case ConsoleKey.DownArrow:
						selected = MenuNavigator.HandleVerticalNav(key, selected, labels.Count);
						break;
					case ConsoleKey.Enter:
						return ids[selected];
				}
			}
		}
	}
}
