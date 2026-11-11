using DungeonAdventures.Src.Game.Interfaces;
using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.Lpc;
using DungeonAdventures.Src.Scenes;
using Maries_DataModels;
using Maries_Ui;

namespace DungeonAdventures.Src.Village.Street
{
	/// <summary>
	/// Hollowmere as a street you walk: the hero at dusk among the houses, the forge and the well, with
	/// villagers strolling about. ← → walk to the next place, ↑ (or Enter) goes in, T talks to whoever is near,
	/// J/C/I open the journal, hero and bag, Tab goes back to the list menu, Q leaves. Clicking a building or a
	/// villager walks there and goes in or talks; the key hints are clickable too.
	///
	/// Going in calls `open` with the village menu option (the same ones the list menu has), then the street
	/// is drawn again. Returns when the player asks for the list menu.
	/// </summary>
	internal static class VillageStreet
	{
		private const int Reach = 12; // pixels: how close a villager must be to talk
		private static int? _heroX;    // where the hero was, so coming back out of a shop puts them at its door

		public static void Run(PlayerData.loadPlayer player, HeroLook look, Action titleBar, Action<string> open, string location)
		{
			var scene = new Scene(RoomStyles.Village, titleBar, VillageMap.WorldWidth);
			scene.Room.Backdrop = new DuskSky(scene.Room.FloorTop - 1);
			var places = VillageMap.Build(scene);
			var folk = VillageMap.AddVillagers(scene, player, location);
			var hero = scene.Cast(look, _heroX ?? VillageMap.Start, VillageMap.ActorRows);
			hero.Lane = 1;
			hero.Dir = Actor.Down;
			hero.Marker = Pixels.Rgb(255, 215, 0);
			var hud = new VillageHud(scene, GameState.IsDevMode);

			int? target = null;     // where the hero is walking to (their centre column)
			Action? onArrive = null; // what happens when they get there
			int step = 0;

			Scene.TryCursor(false);
			using var mouse = MouseInput.Enable();
			try
			{
				scene.Follow(hero.CenterX);
				scene.Open(hud.Page(null, null));
				hud.Invalidate();

				while (true)
				{
					// ---- input
					while (MouseInput.InputAvailable)
					{
						string? command = Command(MouseInput.Read(), scene, hud, places, folk);
						switch (command?.Split(':')[0])
						{
							case null: break;
							case "walk":
								target = Math.Clamp(int.Parse(command.Split(':')[1]), 4, VillageMap.WorldWidth - 4);
								onArrive = null;
								break;
							case "visit":
								var place = places[int.Parse(command.Split(':')[1])];
								target = place.CenterX;
								onArrive = () => GoIn(place);
								break;
							case "meet":
								var who = folk[int.Parse(command.Split(':')[1])];
								int side = who.Actor.CenterX < hero.CenterX ? Reach - 4 : -(Reach - 4);
								target = Math.Clamp(who.Actor.CenterX + side, 4, VillageMap.WorldWidth - 4);
								onArrive = () => Talk(who);
								break;
							case "left": case "right":
								// Pressed again while walking that way: on to the place after.
								int dir = command == "left" ? -1 : 1;
								int from = target is { } going && Math.Sign(going - hero.CenterX) == dir ? going : hero.CenterX;
								target = NextStop(places, from, dir);
								onArrive = null;
								break;
							case "enter":
								if (PlaceAt(places, hero.CenterX) is { } here) GoIn(here);
								else if (Near(folk, hero) is { } nearest) Talk(nearest);
								break;
							case "talk":
								if (Near(folk, hero) is { } person) Talk(person);
								else Leave("TALK TO VILLAGERS");
								break;
							case "journal": Leave("JOURNAL"); break;
							case "hero": Leave("CHARACTER"); break;
							case "bag": Leave("INVENTORY"); break;
							case "quit": Leave("LEAVE THE REALM"); break;
							case "reload" when GameState.IsDevMode: Leave("RELOAD CONTENT (DEV)"); break;
							case "menu": return;
						}
					}

					// ---- movement
					if (target is { } to)
					{
						int dx = Math.Clamp(to - hero.CenterX, -2, 2);
						hero.X += dx;
						hero.Dir = dx < 0 ? Actor.Left : Actor.Right;
						hero.Frame = 1 + step++ % HeroSprite.Frames;
						if (hero.CenterX == to)
						{
							target = null;
							hero.Frame = 0;
							var then = onArrive;
							onArrive = null;
							then?.Invoke();
						}
					}
					foreach (var w in folk) w.Update(scene.Time);

					// ---- drawing
					scene.Follow(hero.CenterX);
					var near = Near(folk, hero);
					hud.Show(hud.Page(PlaceAt(places, hero.CenterX)?.Name, (near?.Actor.Tag as NpcData)?.Name));
					scene.Render();
					scene.Advance();
					Thread.Sleep(Scene.FrameMs);
				}

				// Goes into a place: the hero turns to the door, then the place's screen opens.
				void GoIn(VillagePlace place)
				{
					hero.Dir = Actor.Up;
					hero.Frame = 0;
					scene.Render();
					Thread.Sleep(Scene.FrameMs * 4);
					Leave(place.Option);
				}

				// Talks to a villager: they stop and face the hero, the hero faces them.
				void Talk(Wanderer who)
				{
					if (who.Actor.Tag is not NpcData npc) return;
					bool left = who.Actor.CenterX < hero.CenterX;
					who.Hold(left ? Actor.Right : Actor.Left, 60);
					hero.Dir = left ? Actor.Left : Actor.Right;
					hero.Frame = 0;
					scene.Render();
					Thread.Sleep(Scene.FrameMs * 3);
					PeopleAndPlacesScreens.Talk(npc);
					Back();
				}

				// Opens another screen, then draws the street again when it closes.
				void Leave(string option)
				{
					_heroX = hero.X;
					open(option);
					Back();
				}

				void Back()
				{
					Scene.TryCursor(false);
					QuestScreens.ShowNotices();
					hud.Invalidate();
					scene.Redraw(hud.Page(PlaceAt(places, hero.CenterX)?.Name, null));
					hud.Show(hud.Page(PlaceAt(places, hero.CenterX)?.Name, null));
				}
			}
			finally
			{
				_heroX = hero.X;
				Console.Write("\x1b[0m");
				Scene.TryCursor(true);
			}
		}

		/// <summary>
		/// Turns a key or click into a command. A click in the street gives "meet:i" (a villager), "visit:i"
		/// (a place) or "walk:x" (anywhere else).
		/// </summary>
		private static string? Command(InputEvent input, Scene scene, VillageHud hud, List<VillagePlace> places, List<Wanderer> folk)
		{
			if (input.Key is { } k)
			{
				switch (k.Key)
				{
					case ConsoleKey.LeftArrow or ConsoleKey.A: return "left";
					case ConsoleKey.RightArrow or ConsoleKey.D: return "right";
					case ConsoleKey.UpArrow or ConsoleKey.W or ConsoleKey.Enter or ConsoleKey.Spacebar: return "enter";
					case ConsoleKey.T: return "talk";
					case ConsoleKey.J: return "journal";
					case ConsoleKey.C: return "hero";
					case ConsoleKey.I: return "bag";
					case ConsoleKey.Tab or ConsoleKey.Escape: return "menu";
					case ConsoleKey.Q: return "quit";
					case ConsoleKey.R: return "reload";
				}
				return null;
			}
			if (input.Mouse is not { Action: MouseAction.LeftClick } m) return null;
			if (hud.HintAt(m.X, m.Y) is { } hint) return hint;
			if (m.Y < 1 || m.Y > scene.Room.H / 2) return null;

			// A click in the street: on a villager walks over and talks, on a place walks to its door and goes in.
			int wx = m.X + scene.CameraX;
			int who = folk.FindIndex(f => f.Actor.Tag is NpcData && wx >= f.Actor.X && wx < f.Actor.X + f.Actor.Width);
			if (who >= 0) return $"meet:{who}";
			int place = places.FindIndex(p => p.Contains(wx));
			return place >= 0 ? $"visit:{place}" : $"walk:{wx}";
		}

		/// <summary>The next place's door past x going `dir`, or the end of the street.</summary>
		private static int NextStop(List<VillagePlace> places, int x, int dir)
		{
			var ahead = places.Select(p => p.CenterX).Where(c => dir > 0 ? c > x + 2 : c < x - 2);
			return dir > 0 ? ahead.DefaultIfEmpty(VillageMap.WorldWidth - 8).Min() : ahead.DefaultIfEmpty(8).Max();
		}

		private static VillagePlace? PlaceAt(List<VillagePlace> places, int x) =>
			places.FirstOrDefault(p => Math.Abs(p.CenterX - x) <= 6);

		/// <summary>The nearest villager with something to say, if one is in reach.</summary>
		private static Wanderer? Near(List<Wanderer> folk, Actor hero) =>
			folk.Where(f => f.Actor.Tag is NpcData && Math.Abs(f.Actor.CenterX - hero.CenterX) <= Reach)
				.OrderBy(f => Math.Abs(f.Actor.CenterX - hero.CenterX)).FirstOrDefault();
	}
}
