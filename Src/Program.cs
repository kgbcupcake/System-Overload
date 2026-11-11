using DungeonAdventures.Dev;
using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using DungeonAdventures.Src.GameEngine.Interfaces;
using DungeonAdventures.Src.GameEngine.Systems;
using Maries_Loaders;
using Maries_Ui;
using System.Text;

// 1. Establish Environment
// This ensures Ubuntu/WSL renders the swords ⚔️ instead of ??
Console.OutputEncoding = Encoding.UTF8;

// Mouse support for the whole game: menus are clickable and a click works as "press any key".
// MouseInput turns it off again on exit or Ctrl+C.
MouseInput.Enable();

// Ensure game directories are set up
GameState.EnsureDirectories();

// Point the content-loading layer at the shared content root before any loader is used.
LoaderConfig.ContentRoot = GameState.MasterPath;
LoaderConfig.DevMode = GameState.IsDevMode;

// Run the content seeder at the very start of the application.
ContentSeeder.SeedInitialContent();


/// --- Game Loader's --- ///
ItemRegistry.Initialize();
EquipmentRulesLoader.LoadAll();
BossLoader.LoadAll();
FloorLoader.LoadAll();
Story.LoadAll();
QuestLoader.LoadAll();
PeopleAndPlacesLoader.LoadAll();
OriginLoader.LoadAll();

// Story lines can use {hero}, {loved} and friends anywhere without each caller passing them.
Story.GlobalTokens = StoryTokens.ForCurrentHero;

// Let the VS/WSL "path dump" finish
Thread.Sleep(100);

// 2. Configure UI
ConsoleSetup.ConsoleSize();
Console.Write("\x1b[2J\x1b[3J\x1b[H");








// 3. Set the Spicy Title
// Using your TitleBar method ensures the swords are set and Row 0 is wiped
HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "", GameState.CurrentPlayer?.Health ?? 0, (int)(GameState.CurrentPlayer?.GetTotalMaxHealth() ?? 0), GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);

// 4. Loading Sequence
Console.WriteLine("Press 'S' to skip loading, or any other key (or click) to view the loading sequence...");
// Wait for a key press or a click.
var firstInput = MouseInput.Read();

if (firstInput.Key?.Key == ConsoleKey.S)
{
    Console.WriteLine("Loading sequence skipped.");
    // Clear the console to prepare for MainMenu
    Console.Clear();
}
else
{
    // User pressed another key or just wanted to proceed with loading
    GameStartLoading.Play();
}

// 5. Launch Game
MainMenu.Show();