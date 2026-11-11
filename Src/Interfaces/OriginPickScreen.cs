using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Village.Places;
using Maries_DataModels;
using Maries_Loaders;
using Pastel;
using static DungeonAdventures.Src.Village.Places.ItemPanels;
using static DungeonAdventures.Src.Village.Places.PanelText;
using static DungeonAdventures.Src.Village.Places.ItemDetails;
using static DungeonAdventures.Src.Village.Places.PanelMenu;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>
	/// Character creation's "choose your people" and "choose your background" screens, in the same
	/// two-panel look as the calling screens. Escape on the background screen goes back to the races.
	/// Both lists come from data (races, backgrounds); when a list is empty its screen is skipped.
	/// </summary>
	internal static class OriginPickScreen
	{
		private const string PlaceColor = "#3A4A2A";

		/// <summary>Picks a race, then a background, and saves both ids on the hero.</summary>
		public static void Run(PlayerData.loadPlayer player)
		{
			var races = OriginLoader.Races;
			int race = 0;
			string message = Story.Line("creation.race_prompt", ("name", player.PlayerName)).Pastel(Amber);
			while (true)
			{
				if (races.Count > 0)
				{
					var rows = races.Select(r => new PanelRow(r.Name, r.Color)).ToList();
					var choice = Choose("CHOOSE YOUR PEOPLE", PlaceColor, player, rows, i => RaceDetail(races[i]),
						message, "", "Choose", ref race);
					if (choice.Back)
					{
						message = Story.Line("creation.race_must_pick").Pastel(Amber);
						continue;
					}
					player.Race = races[choice.Index].Id;
				}

				if (PickBackground(player)) return;
				message = "";
			}
		}

		/// <summary>The background screen; false when the player goes back to the races.</summary>
		private static bool PickBackground(PlayerData.loadPlayer player)
		{
			var backgrounds = OriginLoader.Backgrounds;
			if (backgrounds.Count == 0) return true;

			int selected = 0;
			var rows = backgrounds.Select(b => new PanelRow(b.Name, b.Color, Right: Bonus(b), RightColor: Muted)).ToList();
			var choice = Choose("CHOOSE YOUR BACKGROUND", PlaceColor, player, rows, i => BackgroundDetail(backgrounds[i]),
				Story.Line("creation.background_prompt").Pastel(Amber), Summary(), "Choose", ref selected);
			if (choice.Back) return OriginLoader.Races.Count == 0;
			player.Background = backgrounds[choice.Index].Id;
			return true;
		}

		private static List<string> RaceDetail(RaceData r)
		{
			var d = new Detail("race " + r.Name);
			d.Line(r.Name.ToUpper(), r.Color);
			d.Line("People", Muted);
			if (Story.Has(r.StoryKey)) d.Wrapped(Story.Line(r.StoryKey), Flavour);
			d.Blank();
			d.Line("Your look is shaped on the next screens.", Dim);
			return d.Build();
		}

		private static List<string> BackgroundDetail(BackgroundData b)
		{
			var d = new Detail("background " + b.Name);
			d.Line(b.Name.ToUpper(), b.Color);
			d.Line("Background", Muted);
			if (Story.Has(b.StoryKey)) d.Wrapped(Story.Line(b.StoryKey), Flavour);
			d.Blank();
			if (b.Points > 0 && b.Attribute.Length > 0) d.Stat(b.Attribute, $"+{b.Points}", Good);
			if (b.Gold > 0) d.Stat("Gold", $"+{b.Gold}", Gold);
			foreach (string item in b.Items) d.Stat("Also", Fit(item, DetailInner - 12), Text);
			return d.Build();
		}

		private static string Bonus(BackgroundData b) =>
			b.Points > 0 && b.Attribute.Length > 0 ? $"+{b.Points} {b.Attribute}" : "";

		private static string Summary() =>
			$" {"Background".Pastel(Dim)} {"adds a small head start on top of your calling, and people remember it".Pastel(Muted)}";
	}
}
