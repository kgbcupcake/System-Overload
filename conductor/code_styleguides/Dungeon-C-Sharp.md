# Dungeon Adventures C# Style Guide

Standard C# conventions apply. This guide only adds the rules that matter for a fixed-size terminal game.

## 1. Buffer-Safe Rendering: The 84x24 Safe Zone
- **CRITICAL:** All rendering must stay inside an `84x24` grid, with a 1-character margin from the edges of the console.
- Rendering loops terminate at `width - 1` and `height - 1`.
- Anything that can trigger a scrollbar or buffer overflow is a bug.

## 2. Static Output: No Scrolling, No Shifting
- **Preferred:** Write screens with `Console.SetCursorPosition(x, y)` and `Console.Write(...)` so every character lands
  exactly where you put it.
- **Avoid in new code:** `Console.WriteLine()` on full-screen layouts, because it can scroll the window. Some older screens
  still use it; convert them when you're working in them anyway.

## 3. Story and Content Belong in Data
- **CRITICAL:** No narrative or flavour text in C#. Put it in `GameData/Templates/story/story_core.json` and read it with
  `Story.Line`, `Story.Pick` or `Story.Lines`.
- Monsters, bosses, floors, items, weapons, gems and shops are JSON. Code should never depend on a specific item or enemy
  name unless it is a deliberate rule (for example, the store's Healing Potion).
- Menu labels and key hints (`"[ENTER] Use"`, `"BACK TO TOWN"`) may stay in code.

## 4. Rendering and Colour
- **StringBuilder for animation:** Build each frame of an animated screen in a `System.Text.StringBuilder` and write it once.
- **`Pastel` for colour:** Use the torchlight palette from `product-guidelines.md`. Measure text with `UiEngine.StripAnsi`
  before centring, because colour codes have no width on screen.
- **Comments:** Explain *why* the code does something, in plain words.

  ```csharp
  // Bosses can't be fled from, so Retreat just shows the "no escape" story line.
  if (enemy.IsBoss)
  {
      // ...
  }
  ```

## 5. Naming
- Use fantasy names that match the game: `VillageSquare`, `DungeonGate`, `Blacksmith`, `FloorRunner`, `BossLoader`,
  `MonsterData`. Avoid the old sci-fi names (sector, admin, drone, integrity, credits).
- Keep names clear and descriptive. Helper methods say what they do (`FormatFloorLine`, `GrantRewards`).
- Show HP to the player with `PlayerData.GetTotalMaxHealth()`, never the base `HitPoints`.
