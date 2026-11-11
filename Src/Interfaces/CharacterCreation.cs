using DungeonAdventures.Src.Interfaces.UI;
using Maries_Ui;
using Pastel;
using static System.Console;
using DungeonAdventures.Src.GameData.Components;
using static Maries_DataModels.PlayerData;
using Maries_DataModels;
using Maries_Loaders;
using DungeonAdventures.Src.GameEngine.Systems;

namespace DungeonAdventures.Src.Game.Interfaces
{
	internal class CharacterCreation
	{
		public static void Start()
		{
			if (GameState.CurrentPlayer == null) GameState.CurrentPlayer = new loadPlayer();
			var player = GameState.CurrentPlayer;

			int screenWidth = Console.WindowWidth;
			Clear();

			// 1. IDENTITY INPUT
			string subHeader = Story.Line("creation.intro");
			string prompt = Story.Line("creation.name_prompt") + " ";

			UiEngine.DrawCentered(subHeader.Pastel("#FFAB00"), 5);

			UiEngine.DrawCentered(prompt.Pastel("#FFFFFF"), 7);

			CursorVisible = true;
			ForegroundColor = ConsoleColor.Yellow;
			string? nameInput;
			// Mouse mode is paused while typing so clicks can't type junk into the name.
			using (MouseInput.Suspend())
				nameInput = ReadLine();
			ResetColor();

			player.PlayerName = string.IsNullOrWhiteSpace(nameInput) ? Story.Line("creation.default_name") : nameInput;
			CursorVisible = false;

			// 2. PEOPLE AND BACKGROUND (the race shapes the look; the background adds a head start at the end)
			OriginPickScreen.Run(player);

			// 3. CALLING AND PATH (Warrior -> Knight...), with what each starts with
			ClassPickScreen.Run(player);

			// The hero's appearance, drawn from the LPC character catalog (skipped when it isn't installed)
			MakeCharacterScreen.Run(player);

			// 4. CLASS STATS AND STARTING GEAR (before training, so its preview shows the real hero)
			InitializeClassStats(player);
			AssignStarterKit(player);

			// 5. ATTRIBUTE POINTS, then the background's bonus on top of them
			TrainHeroScreen.Run(player);
			BackgroundPerks.Apply(player);
			player.Health = (int)player.GetTotalMaxHealth();

			// 6. FINAL MANIFEST DISPLAY
			Clear();
			int centerX = (screenWidth / 2);

			string welcomeMsg = Story.Line("creation.welcome", ("name", player.PlayerName.Pastel("#FFD700")));
			UiEngine.DrawCentered(welcomeMsg, 8);

			string journeyMsg = $"Class: {player.PlayerClass.Pastel("#FFD700")}";
			UiEngine.DrawCentered(journeyMsg.Pastel("#DCDCDC"), 10);
			if (StoryTokens.Origin(player) is { Length: > 0 } origin)
				UiEngine.DrawCentered(origin.Pastel("#C8B08A"), 11);

			string gearMsg = Story.Line("creation.gear");
			UiEngine.DrawCentered(gearMsg, 12);

			SetCursorPosition(centerX - 12, 14);
			WriteLine("\n    --- STARTING GEAR ---".Pastel("#FFAB00"));

			int itemRow = 16;
			foreach (var item in player.Inventory)
			{
				SetCursorPosition(centerX - 15, itemRow++);
				WriteLine($"> {item.Name} [{item.Type}]".Pastel("#DCDCDC"));
			}

			Thread.Sleep(3000);
			GameState.Sync();

			CreationAnimation.Play(player.PlayerName, player.PlayerClass, player.Health, (int)player.GetTotalMaxHealth(), player.Coins, GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);
		}

		private static void InitializeClassStats(loadPlayer player)
		{
			// Starting HP and danger come from the calling (a path shares its calling's).
			var calling = ClassBook.Of(player.PlayerClass);
			player.HitPoints = calling?.Health ?? 100;
			if (calling != null) player.DifficultyRating = calling.Danger;

			// Vitality points are applied in GetTotalMaxHealth (15 HP each), so base HP stays as set above.
			player.Health = (int)player.GetTotalMaxHealth();
		}

		private static void AssignStarterKit(loadPlayer player)
		{
			player.Inventory.Clear();
			player.AddGold(ClassBook.Of(player.PlayerClass)?.Gold ?? 0);
			player.Inventory.AddRange(ClassBook.Kit(player.PlayerClass));
		}
	}
}