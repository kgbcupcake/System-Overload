using System_Overload.Src.Game.MainInterfaces;
using System_Overload.Src.Interfaces.UI;
using Maries_Ui;
using Pastel;
using static System.Console;
using System_Overload.Src.GameData.Components;
using static Maries_DataModels.PlayerData;
using Maries_DataModels;
using Maries_Loaders;

namespace System_Overload.Src.Game.Interfaces
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
			string? nameInput = ReadLine();
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

				for (int i = 0; i < coreArchetypes.Length; i++)
				{
					string rawText = (i == selectedIndex) ? $">> {coreArchetypes[i].ToUpper()} <<" : $"    {coreArchetypes[i]}    ";

					if (i == selectedIndex)
						UiEngine.DrawCentered(rawText.Pastel("#00FF00"), boxY + 4 + i);
					else
						UiEngine.DrawCentered(rawText.Pastel("#555555"), boxY + 4 + i);
				}

				string hint = "[UP/DOWN] to navigate | [ENTER] to choose";
				UiEngine.DrawCentered(hint.Pastel("#333333"), 17);

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow) selectedIndex = MenuNavigator.HandleVerticalNav(key, selectedIndex, coreArchetypes.Length);
				if (key == ConsoleKey.Enter) pickingBase = false;
			}

			player.PlayerClass = coreArchetypes[selectedIndex];

			// Trigger the Evolution Sub-Menu
			CharSubMenus.ChooseSubClass();

			// 3. SYSTEM ALLOCATION (The New Attribute Spending System)
			PointDistribution(player, screenWidth);

			// 4. GEAR INITIALIZATION
			InitializeSimulationStats(player);
			AssignStarterKit(player);

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

			Transitions.ShowCreationAnimation(player.PlayerName, player.PlayerClass, player.Health, player.HitPoints, player.Coins, GameState.IsDevMode, GameState.DevIteration, GameState.BuildVersion);
		}

		private static void PointDistribution(loadPlayer player, int screenWidth)
		{
			int pointsLeft = 10;
			string[] modules = { "Vitality", "Strength", "Fortitude", "Wisdom" };
			int selectedModule = 0;
			bool allocating = true;

			// Reset boost values to 0 before starting allocation
			player.IntegrityPoints = 0;
			player.ThroughputPoints = 0;
			player.FirewallPoints = 0;

			while (allocating)
			{
				Clear();
				WriteLine("=== ATTRIBUTE POINTS ===".Pastel("#FFAB00"));
				WriteLine(new string('-', screenWidth).Pastel("#5A3A1A"));

				SetCursorPosition(4, 3);
				WriteLine(" TRAIN YOUR HERO ".Pastel("#000000").PastelBg("#FFAB00"));
				SetCursorPosition(4, 4);
				WriteLine(Story.Line("creation.attributes").Pastel("#DCDCDC"));

				// Update live health during allocation
				player.Health = (int)player.GetTotalMaxHealth();

				SetCursorPosition(4, 7);
				string progressBar = new string('■', 10 - pointsLeft).Pastel("#FFD700") +
									 new string('□', pointsLeft).Pastel("#333333");
				WriteLine($"Points: [{progressBar}] | {pointsLeft} left to spend");

				for (int i = 0; i < modules.Length; i++)
				{
					SetCursorPosition(8, 11 + i);
					int currentVal = i switch
					{
						0 => player.IntegrityPoints,
						1 => player.ThroughputPoints,
						2 => player.FirewallPoints,
						3 => 0, // Wisdom does nothing yet
						_ => 0
					};

					string line = (i == selectedModule)
						? $"> {modules[i].PadRight(22)} [+{currentVal}] <"
						: $"  {modules[i].PadRight(22)} [+{currentVal}]  ";

					Write(i == selectedModule ? line.Pastel("#00FF00") : line.Pastel("#555555"));
				}

				// The hero stat box gives live feedback on the right
				GameUiEngine.DrawAttributeBox(player, 52, 11, selectedModule);

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow) selectedModule = (selectedModule == 0) ? modules.Length - 1 : selectedModule - 1;
				if (key == ConsoleKey.DownArrow) selectedModule = (selectedModule == modules.Length - 1) ? 0 : selectedModule + 1;

				if (key == ConsoleKey.RightArrow && pointsLeft > 0)
				{
					if (selectedModule == 0) player.IntegrityPoints++;
					else if (selectedModule == 1) player.ThroughputPoints++;
					else if (selectedModule == 2) player.FirewallPoints++;
					pointsLeft--;
				}
				if (key == ConsoleKey.LeftArrow)
				{
					bool canReduce = selectedModule switch { 0 => player.IntegrityPoints > 0, 1 => player.ThroughputPoints > 0, 2 => player.FirewallPoints > 0, _ => false };
					if (canReduce)
					{
						if (selectedModule == 0) player.IntegrityPoints--;
						else if (selectedModule == 1) player.ThroughputPoints--;
						else if (selectedModule == 2) player.FirewallPoints--;
						pointsLeft++;
					}
				}
				if (key == ConsoleKey.Enter && pointsLeft == 0) allocating = false;
			}
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

		private static void InitializeSimulationStats(loadPlayer player)
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