# Workflow

This document describes how work on Dungeon Adventures is done.

## 1. Testing
- **Build before you commit:** `dotnet build Dungeon-Adventures.csproj` must succeed with no errors.
- **Play-test the change:** Run the game and walk through the screens you touched (new hero → village → a dungeon floor is a
  good default path). Use a throwaway Documents folder if you don't want to touch your own saves.
- **Unit tests:** NUnit is referenced but there is no test project yet. Game rules (damage, hit chance, rewards) are the best
  first candidates.

## 2. Commits
- **Frequency:** Commit after each finished task.
- **Message style:** Plain and descriptive: a short summary line, then a few bullet points on what changed and why.
  - **Example:** `Add the Healer to Hollowmere`
  - **Example:** `Bosses: speak their enraged line once when below half health`

## 3. Content Changes
- Edit defaults in `GameData/Templates/`. The game only seeds files that don't exist yet, so delete the matching file in
  `~/Documents/DungeonAdventures/Realm Data/` (or the whole folder) to pick up new defaults.
- New story text goes in `GameData/Templates/story/story_core.json`. In dev mode a missing key shows on screen as `<key>`.
- Keep each line short enough to fit the 84x24 window.

## 4. Verification
- **Visual check:** The lead developer signs off on how screens look. Check them against `product-guidelines.md`
  (palette, tone) and `code_styleguides/Dungeon-C-Sharp.md` (rendering rules).
