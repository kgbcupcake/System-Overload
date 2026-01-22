using System_Overload.Src.Game.MainInterfaces;
using System_Overload.Src.Utilities.UI;
using Pastel;
using static System.Console;
using System_Overload.Src.GameData.Components;
using static System_Overload.Src.GameData.Entities.PlayerData;
using Src.GameData.Components;

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
			string subHeader = "--- NEURAL UPLINK INITIALIZED ---";
			string prompt = "Input Consciousness Designation (Name): ";

			SetCursorPosition((screenWidth / 2) - (subHeader.Length / 2), 5);
			WriteLine(subHeader.Pastel("#00FFFF"));

			int promptX = (screenWidth / 2) - (prompt.Length / 2);
			SetCursorPosition(promptX, 7);
			Write(prompt.Pastel("#FFFFFF"));

			CursorVisible = true;
			ForegroundColor = ConsoleColor.Cyan;
			string? nameInput = ReadLine();
			ResetColor();

			player.PlayerName = string.IsNullOrWhiteSpace(nameInput) ? "Subject_Zero" : nameInput;
			CursorVisible = false;

			// 2. ARCHETYPE SELECTION
			string[] coreArchetypes = { "Aegis-Protocol", "Cipher-Blade", "System-Architect" };
			int selectedIndex = 0;
			bool pickingBase = true;

			int boxW = 60;
			int boxX = (screenWidth / 2) - (boxW / 2);
			int boxY = 4;

			while (pickingBase)
			{
				Clear();
				DrawOuterBox(boxX, boxY, boxW, 12, "#00FFFF");

				string greeting = $"Syncing Profile: {player.PlayerName}";
				SetCursorPosition((screenWidth / 2) - (greeting.Length / 2), boxY + 1);
				Write(greeting.Pastel("#FFD700"));

				for (int i = 0; i < coreArchetypes.Length; i++)
				{
					string rawText = (i == selectedIndex) ? $">> {coreArchetypes[i].ToUpper()} <<" : $"    {coreArchetypes[i]}    ";
					int classX = (screenWidth / 2) - (rawText.Length / 2);
					SetCursorPosition(classX, boxY + 4 + i);

					if (i == selectedIndex)
						Write(rawText.Pastel("#00FF00"));
					else
						Write(rawText.Pastel("#555555"));
				}

				string hint = "[UP/DOWN] to navigate | [ENTER] to confirm link";
				SetCursorPosition((screenWidth / 2) - (hint.Length / 2), 17);
				Write(hint.Pastel("#333333"));

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow) selectedIndex = (selectedIndex == 0) ? coreArchetypes.Length - 1 : selectedIndex - 1;
				if (key == ConsoleKey.DownArrow) selectedIndex = (selectedIndex == coreArchetypes.Length - 1) ? 0 : selectedIndex + 1;
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

			string welcomeMsg = $"Link Confirmed, {player.PlayerName.Pastel("#00FF00")}!";
			int welcomeVisualLength = UiEngine.StripAnsi(welcomeMsg).Length;
			SetCursorPosition(centerX - (welcomeVisualLength / 2), 8);
			WriteLine(welcomeMsg);

			string journeyMsg = $"Simulation Profile: {player.PlayerClass.Pastel("#FFD700")}";
			int journeyVisualLength = UiEngine.StripAnsi(journeyMsg).Length;
			SetCursorPosition(centerX - (journeyVisualLength / 2), 10);
			WriteLine(journeyMsg.Pastel("#DCDCDC"));

			string gearMsg = "Initial data manifests added to buffer.";
			SetCursorPosition(centerX - (gearMsg.Length / 2), 12);
			WriteLine(gearMsg);

			SetCursorPosition(centerX - 12, 14);
			WriteLine("\n    --- INITIAL MANIFEST ---".Pastel("#00FFFF"));

			int itemRow = 16;
			foreach (var item in player.Inventory)
			{
				SetCursorPosition(centerX - 15, itemRow++);
				WriteLine($"> {item.Name} [{item.Type}]".Pastel("#DCDCDC"));
			}

			Thread.Sleep(3000);
			GameState.Sync();

			UiFunctions.ShowCreationAnimation(player.PlayerName, player.PlayerClass);
		}

		private static void PointDistribution(loadPlayer player, int screenWidth)
		{
			int bandwidthLeft = 10;
			string[] modules = { "Integrity Protocol", "Throughput Ratio", "Firewall Uplink", "Buffer Optimization" };
			int selectedModule = 0;
			bool allocating = true;

			// Reset boost values to 0 before starting allocation
			player.IntegrityPoints = 0;
			player.ThroughputPoints = 0;
			player.FirewallPoints = 0;

			while (allocating)
			{
				Clear();
				WriteLine("=== SYSTEM BANDWIDTH ALLOCATION ===".Pastel("#00FFFF"));
				WriteLine(new string('-', screenWidth).Pastel("#125874"));

				SetCursorPosition(4, 3);
				WriteLine(" ALLOCATION OVERVIEW ".Pastel("#000000").PastelBg("#00FFFF"));
				SetCursorPosition(4, 4);
				WriteLine($"Synchronize your Neural Link via bandwidth distribution.".Pastel("#DCDCDC"));

				// Update live health during allocation
				player.Health = (int)player.GetTotalMaxHealth();

				SetCursorPosition(4, 7);
				string progressBar = new string('■', 10 - bandwidthLeft).Pastel("#00FF00") +
									 new string('?', bandwidthLeft).Pastel("#333333");
				WriteLine($"Bandwidth Pool: [{progressBar}] | {bandwidthLeft} GHz Available");

				for (int i = 0; i < modules.Length; i++)
				{
					SetCursorPosition(8, 11 + i);
					int currentVal = i switch
					{
						0 => player.IntegrityPoints,
						1 => player.ThroughputPoints,
						2 => player.FirewallPoints,
						3 => 0, // Visual placeholder for Buffer
						_ => 0
					};

					string line = (i == selectedModule)
						? $"> {modules[i].PadRight(22)} [+{currentVal}] <"
						: $"  {modules[i].PadRight(22)} [+{currentVal}]  ";

					Write(i == selectedModule ? line.Pastel("#00FF00") : line.Pastel("#555555"));
				}

				// The high-tech attribute box provides live feedback on the right
				UiEngine.DrawAttributeBox(player, 52, 11, selectedModule);

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow) selectedModule = (selectedModule == 0) ? modules.Length - 1 : selectedModule - 1;
				if (key == ConsoleKey.DownArrow) selectedModule = (selectedModule == modules.Length - 1) ? 0 : selectedModule + 1;

				if (key == ConsoleKey.RightArrow && bandwidthLeft > 0)
				{
					if (selectedModule == 0) player.IntegrityPoints++;
					else if (selectedModule == 1) player.ThroughputPoints++;
					else if (selectedModule == 2) player.FirewallPoints++;
					bandwidthLeft--;
				}
				if (key == ConsoleKey.LeftArrow)
				{
					bool canReduce = selectedModule switch { 0 => player.IntegrityPoints > 0, 1 => player.ThroughputPoints > 0, 2 => player.FirewallPoints > 0, _ => false };
					if (canReduce)
					{
						if (selectedModule == 0) player.IntegrityPoints--;
						else if (selectedModule == 1) player.ThroughputPoints--;
						else if (selectedModule == 2) player.FirewallPoints--;
						bandwidthLeft++;
					}
				}
				if (key == ConsoleKey.Enter && bandwidthLeft == 0) allocating = false;
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
			// Setting starting parameters based on the chosen Profile
			switch (player.PlayerClass)
			{
				case "Aegis-Protocol":
				case "Titan-Shell":
				case "Bastion-Core":
					player.HitPoints = 150;
					player.DifficultyRating = 0;
					break;
				case "Cipher-Blade":
				case "Ghost-Code":
				case "Neural-Reaper":
					player.HitPoints = 100;
					player.DifficultyRating = 1;
					break;
				case "System-Architect":
				case "Root-Admin":
					player.HitPoints = 120;
					player.DifficultyRating = 2;
					break;
				default:
					player.HitPoints = 100;
					break;
			}

			// Apply the integrity points spent in allocation to base HP
			player.HitPoints += (player.IntegrityPoints * 10);
			player.Health = (int)player.GetTotalMaxHealth();
		}

		private static void AssignStarterKit(loadPlayer player)
		{
			player.Inventory.Clear();
			switch (player.PlayerClass)
			{
				case string s when s.Contains("Aegis") || s.Contains("Shell") || s.Contains("Bastion"):
					player.AddGold(50);
					player.Inventory.Add(new WeaponData("Kinetic Breacher", 18, 12.0f, 250, ItemRarity.Common, "Heavy", EffectType.Defense, 100, 10, "12ga", 5, 2));
					player.Inventory.Add(new ItemData("Neural Stim-Pack", 50, 0.1f, 2, "Consumable", ItemRarity.Common, EffectType.Restorative, 0, 0, false, 0));
					break;

				case string s when s.Contains("Cipher") || s.Contains("Ghost") || s.Contains("Reaper"):
					player.AddGold(150);
					player.Inventory.Add(new WeaponData("Mono-Molecular Edge", 25, 1.5f, 400, ItemRarity.Uncommon, "Melee", EffectType.Speed, 50, 0, "None", 20, 1));
					player.Inventory.Add(new ItemData("Logic-Spike", 75, 0.2f, 1, "Tool", ItemRarity.Rare, EffectType.None, 1, 0, false, 0));
					break;

				case string s when s.Contains("Architect") || s.Contains("Admin"):
					player.AddGold(25);
					player.Inventory.Add(new WeaponData("Feedback Staff", 15, 4.0f, 300, ItemRarity.Common, "Exotic", EffectType.Electric, 100, 20, "Energy", 10, 4));
					player.Inventory.Add(new GemData { Name = "Clock-Cycle Gem", Power = 5.0f, Attributes = new List<EffectType> { EffectType.Electric } });
					break;
			}
		}
	}
}