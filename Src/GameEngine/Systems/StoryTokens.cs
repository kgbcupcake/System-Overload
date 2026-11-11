using DungeonAdventures.Src.GameData.Components;
using Maries_Loaders;

namespace DungeonAdventures.Src.GameEngine.Systems
{
	/// <summary>
	/// The tokens any story line can use, filled from the current hero:
	/// {hero}, {loved}, {relation}, {relation_memory}, {race} and {background}.
	/// </summary>
	public static class StoryTokens
	{
		public static IEnumerable<(string Token, object? Value)> ForCurrentHero()
		{
			var player = GameState.CurrentPlayer;
			string hero = player?.PlayerName ?? "";
			string loved = LovedOneName(player?.LovedOneName);

			yield return ("hero", hero);
			yield return ("loved", loved);

			// A relation entry's lines are: menu label, how the story refers to it, a memory line.
			// They're read raw (Story.Lines would call back into this provider forever) and may
			// name {loved} or {hero} themselves, so those two are filled in here.
			var relation = Story.Raw(RelationKey(player?.LovedOneRelation));
			string Fill(int i) => (relation.ElementAtOrDefault(i) ?? "")
				.Replace("{loved}", loved)
				.Replace("{hero}", hero);

			yield return ("relation", Fill(1));
			yield return ("relation_memory", Fill(2));

			yield return ("race", OriginLoader.Race(player?.Race)?.Name ?? "");
			yield return ("background", OriginLoader.Background(player?.Background)?.Name ?? "");
		}

		/// <summary>"Elf · Soldier" for the hero, or "" when they have neither (heroes made before origins).</summary>
		public static string Origin(Maries_DataModels.PlayerData.loadPlayer player) =>
			string.Join(" · ", new[] { OriginLoader.Race(player.Race)?.Name, OriginLoader.Background(player.Background)?.Name }
				.Where(n => !string.IsNullOrEmpty(n)));

		/// <summary>The chosen name, or the default from the story data when none was chosen.</summary>
		public static string LovedOneName(string? chosen) =>
			string.IsNullOrWhiteSpace(chosen) ? Story.Raw("life_before.default_name").FirstOrDefault() ?? "" : chosen;

		public static string RelationKey(string? relationId) =>
			"life_before.relation." + (string.IsNullOrWhiteSpace(relationId) ? "partner" : relationId);
	}
}
