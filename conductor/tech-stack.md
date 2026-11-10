# Dungeon Adventures Tech Stack

## Core Technologies
- **Programming Language:** C#
- **Framework:** .NET 10.0 (LTS)
- **Projects:** `Dungeon-Adventures.csproj` (the game) plus three class libraries: `Maries_Ui`, `Maries_DataModels` and
  `Maries_Loaders`.

## Key Libraries & APIs
- **`Pastel`:** 24-bit console colour for the torchlight palette (see `product-guidelines.md`).
- **`Spectre.Console`:** Panels, tables and structured layout.
- **`System.Text.Json`:** All content, story text and save files.
- **`System.Text.StringBuilder`:** Pre-rendering full frames for animated screens (loading, embers) without flicker.
- **`System.Console`:** Precise cursor control (`SetCursorPosition`) and writing (`Write`) inside a fixed viewport.

## Technical Constraints & Global Rules
1.  **Strictly Text-Based:** The game is a pure console application. No GUI libraries (ImGui, overlays, etc.).
    `Interfaces/DevGui_Main.cs` is an old ImGui panel and stays excluded from the build.
2.  **Data, Not Code:** Content and story text are JSON under `GameData/Templates/`, read through the loaders in
    `Maries_Loaders`. New content types get a data class tagged `[MariesGuiContent("<folder>")]`, a loader and a folder in
    `GameState.DataFolders`.
3.  **Cross-Platform Safe Zone:** To work on Windows and Linux without buffer scrolling, all rendering stays inside an
    `84x24` safe zone. Loops iterate up to `width - 1` and `height - 1`.
4.  **Buffer Stability:** New screens should position output with `Console.SetCursorPosition()` and `Console.Write()` rather
    than `Console.WriteLine()`, so nothing scrolls or shifts. (Some older screens, such as combat, still use `WriteLine`.)

## Architectural Guidelines
- **StringBuilder Frames:** Animated screens build each frame in a `StringBuilder` and write it in one go.
- **Centering:** Centre text with `UiEngine.DrawCentered` and `UiEngine.StripAnsi`, so colour codes don't throw off the maths.
- **Max HP:** Always show `PlayerData.GetTotalMaxHealth()` to the player, not the base `HitPoints`.
- **Story Lookups:** Use `Story.Line`, `Story.Pick` and `Story.Lines` from `Maries_Loaders`; never hardcode narrative text.
