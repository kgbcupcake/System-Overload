using System_Overload.Dev;
using System_Overload.GameEngine.Systems.Loaders;
using System_Overload.Src.GameData.Components;
using System_Overload.Src.GameEngine.Interfaces;
using System_Overload.Src.GameEngine.Systems.Loaders;
using System_Overload.Src.Utilities.UI;
using System.Text;

// 1. Establish Environment
// This ensures Ubuntu/WSL renders the swords ⚔️ instead of ??
Console.OutputEncoding = Encoding.UTF8;

// Ensure game directories are set up
GameState.EnsureDirectories();

// Run the content seeder at the very start of the application.
ContentSeeder.SeedInitialContent();


/// --- Game Loader's --- ///
ItemRegistry.Initialize();
AdminLoader.LoadAll();
SectorLoader.LoadAll();

// Let the VS/WSL "path dump" finish
Thread.Sleep(100);

// 2. Configure UI
UiFunctions.ConsoleSize();
Console.Write("\x1b[2J\x1b[3J\x1b[H");




string contentRoot =
Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
"DungeonAdventures");





// 3. Set the Spicy Title
// Using your TitleBar method ensures the swords are set and Row 0 is wiped
UiFunctions.TitleBar();

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
    UiFunctions.StartGameLoading();
}

// 5. Launch Game
MainMenu.Show();