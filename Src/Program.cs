using System_Overload.Dev;
using System_Overload.Src.GameData.Components;
using Maries_DataModels;
using System_Overload.Src.GameEngine.Interfaces;
using Maries_Loaders;
using Maries_Ui;
using System.Text;

// 1. Establish Environment
// This ensures Ubuntu/WSL renders the swords ⚔️ instead of ??
Console.OutputEncoding = Encoding.UTF8;

// Ensure game directories are set up
GameState.EnsureDirectories();

// Point the content-loading layer at the shared content root before any loader is used.
LoaderConfig.ContentRoot = GameState.MasterPath;
LoaderConfig.DevMode = GameState.IsDevMode;

// Run the content seeder at the very start of the application.
ContentSeeder.SeedInitialContent();


/// --- Game Loader's --- ///
ItemRegistry.Initialize();
AdminLoader.LoadAll();
SectorLoader.LoadAll();
Story.LoadAll();

// Let the VS/WSL "path dump" finish
Thread.Sleep(100);

// 2. Configure UI
ConsoleSetup.ConsoleSize();
Console.Write("\x1b[2J\x1b[3J\x1b[H");








// 3. Set the Spicy Title
// Using your TitleBar method ensures the swords are set and Row 0 is wiped
HudChrome.TitleBar(GameState.CurrentPlayer != null, GameState.CurrentPlayer?.PlayerName ?? "", GameState.CurrentPlayer?.Health ?? 0, GameState.CurrentPlayer?.HitPoints ?? 0, GameState.CurrentPlayer?.Coins ?? 0, GameState.IsDevMode);

// 4. Loading Sequence
Console.WriteLine("Press 'S' to skip loading, or any other key to view the loading sequence...");
// Wait for a key press. This will block until a key is pressed.
ConsoleKeyInfo key = Console.ReadKey(true); // 'true' means don't display the key

if (key.Key == ConsoleKey.S)
{
    Console.WriteLine("Loading sequence skipped.");
    // Clear the console to prepare for MainMenu
    Console.Clear();
}
else
{
    // User pressed another key or just wanted to proceed with loading
    Transitions.StartGameLoading();
}

// 5. Launch Game
MainMenu.Show();