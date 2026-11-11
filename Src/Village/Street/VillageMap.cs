using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine.Systems;
using DungeonAdventures.Src.Lpc;
using DungeonAdventures.Src.Scenes;
using Maries_DataModels;
using Maries_Loaders;
using static DungeonAdventures.Src.Scenes.Pixels;

namespace DungeonAdventures.Src.Village.Street
{
	/// <summary>A spot on the street the hero can go into: the village menu option it opens, and where it stands.</summary>
	internal sealed record VillagePlace(string Option, string Name, int CenterX, Func<int, bool> Contains);

	/// <summary>
	/// Hollowmere laid out along one street, left to right: the hero's home, the General Store, the healer,
	/// the well in the square, the blacksmith's forge, and the old gate on the road to the dungeon. Also the
	/// villagers who stroll up and down it.
	/// </summary>
	internal static class VillageMap
	{
		public const int WorldWidth = 300;
		/// <summary>Terminal rows tall the people in the street are (smaller than in the floor scenes, so the houses fit).</summary>
		public const int ActorRows = 7;
		/// <summary>Where the hero first stands: by the well.</summary>
		public const int Start = 128;

		/// <summary>Puts the buildings and decor in the scene and returns the places the hero can enter.</summary>
		public static List<VillagePlace> Build(Scene scene)
		{
			var home = new Building(6, 30, 18, Rgb(214, 196, 156), Rgb(124, 52, 42)) { Id = "HOME", Name = "Your home", Sign = SignIcon.House };
			var store = new Building(48, 36, 21, Rgb(204, 176, 132), Rgb(64, 84, 120)) { Id = "GENERAL STORE", Name = "General Store", Sign = SignIcon.Coin };
			var healer = new Building(94, 30, 19, Rgb(220, 216, 196), Rgb(64, 104, 66)) { Id = "HEALER", Name = "The Healer", Sign = SignIcon.Cross };
			var smith = new Building(164, 40, 20, Rgb(120, 108, 100), Rgb(56, 48, 50)) { Id = "BLACKSMITH", Name = "Blacksmith", Sign = SignIcon.Anvil, Stone = true, Forge = true };
			var gate = new Gateway(244) { Id = "ROAD TO HOLLOWMERE", Name = "Road to the dungeon" };

			scene.Add(new Tree(40, 7)).Add(home).Add(store).Add(new LampPost(88)).Add(healer)
				.Add(new Tree(134, 9)).Add(new Well(140)).Add(new LampPost(158)).Add(smith)
				.Add(new Tree(214, 8)).Add(new LampPost(228)).Add(gate)
				.Add(new Tree(276, 7, dead: true)).Add(new Tree(292, 6, dead: true));

			return new List<VillagePlace>
			{
				Place(home), Place(store), Place(healer), Place(smith),
				new(gate.Id, gate.Name, gate.CenterX, gate.Contains),
			};
		}

		private static VillagePlace Place(Building b) => new(b.Id, b.Name, b.CenterX, b.Contains);

		/// <summary>
		/// The village's people from the NPC data (those whose quests let them be here), plus a few townsfolk,
		/// each strolling along part of the street. A villager's actor carries its <see cref="NpcData"/> as its
		/// tag (townsfolk carry none).
		/// </summary>
		public static List<Wanderer> AddVillagers(Scene scene, PlayerData.loadPlayer player, string location)
		{
			var people = PeopleAndPlacesLoader.NpcsAt(location, flag => QuestSystem.HasFlag(player, flag));
			var cast = people.Select(n => (Role: n.Role, Data: (NpcData?)n, Seed: StableSeed(n.Id))).ToList();
			for (int i = 0; i < 3; i++) cast.Add(("townsfolk", null, 1000 + i * 37));

			var walkers = new List<Wanderer>();
			for (int i = 0; i < cast.Count; i++)
			{
				var look = VillagerLooks.Roll(cast[i].Role, cast[i].Seed);
				if (look == null) continue;
				int home = 20 + (i * 97 + 31) % (WorldWidth - 90);
				var actor = scene.Cast(look, home, ActorRows);
				actor.Lane = new[] { -2, 2, -1, 3, 0, -2 }[i % 6];
				actor.Tag = cast[i].Data;
				walkers.Add(new Wanderer(actor, Math.Max(4, home - 60), Math.Min(WorldWidth - 50, home + 60), cast[i].Seed));
			}
			return walkers;
		}

		private static int StableSeed(string s)
		{
			unchecked
			{
				int h = 17;
				foreach (char c in s) h = h * 31 + c;
				return h & 0x7FFFFFFF;
			}
		}
	}
}
