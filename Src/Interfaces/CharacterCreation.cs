using DungeonAdventures.Src.Game.MainInterfaces;
using DungeonAdventures.Src.Interfaces.UI;
using Maries_Ui;
using Pastel;
using static System.Console;
using DungeonAdventures.Src.GameData.Components;
using static Maries_DataModels.PlayerData;
using Maries_DataModels;
using Maries_Loaders;

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

			// 2. ARCHETYPE SELECTION
			string[] coreArchetypes = { "Warrior", "Rogue", "Mage" };
			int selectedIndex = 0;
			bool pickingBase = true;

			int boxW = 60;
			int boxX = (screenWidth / 2) - (boxW / 2);
			int boxY = 4;

			while (pickingBase)
			{
				Clear();
				DrawOuterBox(boxX, boxY, boxW, 12, "#B8860B");

				string greeting = Story.Line("creation.greeting", ("name", player.PlayerName));
				UiEngine.DrawCentered(greeting.Pastel("#FFD700"), boxY + 1);

				MouseInput.ClearZones();
				for (int i = 0; i < coreArchetypes.Length; i++)
				{
					MouseInput.AddZone(boxY + 4 + i, boxX, boxW, i);
					string rawText = (i == selectedIndex) ? $">> {coreArchetypes[i].ToUpper()} <<" : $"    {coreArchetypes[i]}    ";

					if (i == selectedIndex)
						UiEngine.DrawCentered(rawText.Pastel("#00FF00"), boxY + 4 + i);
					else
						UiEngine.DrawCentered(rawText.Pastel("#555555"), boxY + 4 + i);
				}

				string hint = "[UP/DOWN] to navigate | [ENTER] or click to choose";
				UiEngine.DrawCentered(hint.Pastel("#333333"), 17);

				var key = MouseInput.ReadMenuKey(ref selectedIndex, coreArchetypes.Length);
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow) selectedIndex = MenuNavigator.HandleVerticalNav(key, selectedIndex, coreArchetypes.Length);
				if (key == ConsoleKey.Enter) pickingBase = false;
			}

			player.PlayerClass = coreArchetypes[selectedIndex];

			// Trigger the Evolution Sub-Menu
			CharSubMenus.ChooseSubClass();

			// The hero's appearance, drawn from the LPC character catalog (skipped when it isn't installed)
			MakeCharacterScreen.Run(player);

			// 3. CLASS STATS AND STARTING GEAR (before training, so its preview shows the real hero)
			InitializeClassStats(player);
			AssignStarterKit(player);

			// 4. ATTRIBUTE POINTS
			TrainHeroScreen.Run(player);

			// 5. FINAL MANIFEST DISPLAY
			Clear();
			int centerX = (screenWidth / 2);

			string welcomeMsg = Story.Line("creation.welcome", ("name", player.PlayerName.Pastel("#FFD700")));
			UiEngine.DrawCentered(welcomeMsg, 8);

			string journeyMsg = $"Class: {player.PlayerClass.Pastel("#FFD700")}";
			UiEngine.DrawCentered(journeyMsg.Pastel("#DCDCDC"), 10);

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

			Transitions.ShowCreationAnimation(player.PlayerName, player.PlayerClass, player.Health, (int)player.GetTotalMaxHealth(), player.Coins, GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);
		}

		private static void DrawOuterBox(int x, int y, int w, int h, string color)
		{
			string topBorder = "╔" + new string('═', w - 2) + "╗";
			string bottomBorder = "╚" + new string('═', w - 2) + "╝";
			string side = "║";

			SetCursorPosition(x, y);
			Write(topBorder.Pastel(color));
			for (int i = 1; i < h - 1; i++)
			{
				SetCursorPosition(x, y + i);
				Write(side.Pastel(color));
				SetCursorPosition(x + w - 1, y + i);
				Write(side.Pastel(color));
			}
			SetCursorPosition(x, y + h - 1);
			Write(bottomBorder.Pastel(color));
		}

		private static void InitializeClassStats(loadPlayer player)
		{
			// Starting HP and danger based on the chosen class
			switch (player.PlayerClass)
			{
				case "Warrior":
				case "Knight":
				case "Paladin":
				case "Berserker":
					player.HitPoints = 150;
					player.DifficultyRating = 0;
					break;
				case "Rogue":
				case "Assassin":
				case "Shadowblade":
				case "Ranger":
					player.HitPoints = 100;
					player.DifficultyRating = 1;
					break;
				case "Mage":
				case "Wizard":
				case "Warlock":
				case "Battlemage":
					player.HitPoints = 120;
					player.DifficultyRating = 2;
					break;
				default:
					player.HitPoints = 100;
					break;
			}

			// Vitality points are applied in GetTotalMaxHealth (15 HP each), so base HP stays as set above.
			player.Health = (int)player.GetTotalMaxHealth();
		}

		private static void AssignStarterKit(loadPlayer player)
		{
			player.Inventory.Clear();
			switch (player.PlayerClass)
			{
				case "Warrior" or "Knight" or "Paladin" or "Berserker":
					player.AddGold(50);
					player.Inventory.Add(new WeaponData("Iron Warhammer", 18, 12.0f, 250, ItemRarity.Common, "Heavy", EffectType.Defense, 100, 0, "None", 5, 2));
					player.Inventory.Add(new ItemData("Healing Potion", 50, 0.1f, 2, "Consumable", ItemRarity.Common, EffectType.Restorative, 0, 0, false, 0));
					break;

				case "Rogue" or "Assassin" or "Shadowblade" or "Ranger":
					player.AddGold(150);
					player.Inventory.Add(new WeaponData("Whisper Dagger", 25, 1.5f, 400, ItemRarity.Uncommon, "Melee", EffectType.Speed, 50, 0, "None", 20, 1));
					player.Inventory.Add(new ItemData("Lockpick Set", 75, 0.2f, 1, "Tool", ItemRarity.Rare, EffectType.None, 1, 0, false, 0));
					break;

				case "Mage" or "Wizard" or "Warlock" or "Battlemage":
					player.AddGold(25);
					player.Inventory.Add(new WeaponData("Oak Staff", 15, 4.0f, 300, ItemRarity.Common, "Magic", EffectType.Electric, 100, 0, "None", 10, 4));
					player.Inventory.Add(new GemData { Name = "Mana Crystal", Power = 5.0f, Attributes = new List<EffectType> { EffectType.Electric } });
					break;
			}
		}
	}
}