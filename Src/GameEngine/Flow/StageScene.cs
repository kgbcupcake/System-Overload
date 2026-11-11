using DungeonAdventures.Src.Scenes;
using Maries_DataModels;
using Pastel;

namespace DungeonAdventures.Src.GameEngine.Flow
{
	/// <summary>
	/// The floor's scenes, built on <see cref="Scene"/>: the opening (the hero comes down a stair into the
	/// floor's room) and one per story line (the hero walks in, or squeezes through a cracked tombstone).
	/// The room style comes from <see cref="FloorData.Room"/> and the scene from <see cref="FloorStage.Scene"/>,
	/// both guessed from the text when empty.
	///
	/// Each returns false when it can't draw (no LPC catalog, colour off, scene "none", or an error) and the
	/// caller prints the plain text instead.
	/// </summary>
	public static class StageScene
	{
		/// <summary>The scene behind one story line.</summary>
		public static bool Play(FloorData floor, FloorStage stage, HeroLook? look, Action drawTitleBar)
		{
			string kind = KindOf(stage);
			if (kind == "none") return false;
			string colour = string.IsNullOrEmpty(stage.HexColor) ? "#CCCCCC" : stage.HexColor;

			return Run(floor, look, drawTitleBar, scene => StoryBox.Wrap(stage.StoryText, scene.Box.TextWidth).Select(l => l.Pastel(colour)).ToList(),
				(scene, hero) =>
				{
					if (kind == "squeeze")
					{
						var stone = new Tombstone(0.56);
						scene.Room.Add(stone).Dress(Layout(scene, new[] { 0.08, 0.40, 0.95 }, hero.Width));
						return s =>
						{
							s.WalkTo(hero, stone.CrackX - hero.Width / 2);
							s.SinkOut(hero);
						};
					}
					scene.Room.Dress(Layout(scene, new[] { 0.08, 0.68, 0.94 }, hero.Width));
					return s => s.WalkTo(hero, (int)(s.Width * 0.22));
				});
		}

		/// <summary>The floor's opening: down the stair, under its heading, danger and description.</summary>
		public static bool Intro(FloorData floor, string heading, HeroLook? look, Action drawTitleBar)
		{
			return Run(floor, look, drawTitleBar, scene =>
				{
					int width = scene.Box.TextWidth;
					string danger = $"DANGER: {floor.Difficulty}";
					var lines = new List<string>();
					if (heading.Length + 3 + danger.Length <= width)
						lines.Add(heading.Pastel("#FFAB00") + new string(' ', width - heading.Length - danger.Length) + danger.Pastel("#FFD700"));
					else
					{
						lines.Add(heading.Pastel("#FFAB00"));
						lines.Add(danger.Pastel("#FFD700"));
					}
					lines.AddRange(StoryBox.Wrap(floor.Description, width).Select(l => l.Pastel("#DDDDDD")));
					return lines;
				},
				(scene, hero) =>
				{
					var stair = new Stair();
					hero.X = -hero.Width / 2 - 2;
					scene.Room.Add(stair).Dress(new RoomLayout(new[] { 0.76, 0.97 }, new[] { 0.60, 0.82 }, Array.Empty<(double, double)>()));
					return s => s.WalkTo(hero, stair.EndX + 6 - hero.Width / 2);
				});
		}

		/// <summary>"walk", "squeeze" or "none": the stage's own choice, else guessed from its line.</summary>
		private static string KindOf(FloorStage stage)
		{
			string set = (stage.Scene ?? "").Trim().ToLowerInvariant();
			if (set is "walk" or "squeeze" or "none") return set;
			string text = stage.StoryText.ToLowerInvariant();
			return text.Contains("squeeze") || text.Contains("crack") || text.Contains("tombstone") ? "squeeze" : "walk";
		}

		/// <summary>Torch spots, the default banners, and the wall kept plain behind where the hero stops.</summary>
		private static RoomLayout Layout(Scene scene, double[] torches, int heroWidth) =>
			new(torches, RoomLayout.Default.Banners, new[] { (0.18, 0.22 + (double)heroWidth / scene.Width) });

		/// <summary>
		/// Sets up a scene in the floor's room with the hero just off the left edge, lets `stage` add props and
		/// pick the movement, then plays the movement and the text.
		/// </summary>
		private static bool Run(FloorData floor, HeroLook? look, Action drawTitleBar,
			Func<Scene, List<string>> text, Func<Scene, Actor, Action<Scene>> stage)
		{
			if (look == null || !Scene.CanCast(look)) return false;
			try
			{
				var style = RoomStyles.Named(floor.Room) ?? RoomStyles.Guess(floor.Id + " " + floor.Title);
				var scene = new Scene(style, drawTitleBar);
				var hero = scene.Cast(look, 0);
				hero.X = -hero.Width + 2;
				var move = stage(scene, hero);
				var pages = StoryBox.Pages(text(scene));
				scene.Play(pages[0], s =>
				{
					move(s);
					s.Say(pages);
				});
				return true;
			}
			catch (Exception)
			{
				// A broken sheet or a tiny window shouldn't lose the story: the caller prints it plain.
				Console.Write("\x1b[0m");
				try { Console.CursorVisible = true; } catch (Exception) { }
				return false;
			}
		}
	}
}
