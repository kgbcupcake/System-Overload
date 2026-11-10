# Project Tracks

This file tracks the major pieces of work on the project.

---

## [x] Track: Fantasy re-theme ("Dungeon Adventures – Reborn")
Turned the game into a fantasy dungeon crawler: Warrior, Rogue and Mage classes with three paths each; the village of
Hollowmere; four dungeon floors with monsters and a boss on each; fantasy items, weapons and gems; the torchlight palette.
Moved all story and flavour text into `GameData/Templates/story/`, renamed the project, namespaces, classes, files,
content folders and JSON fields to their fantasy names, and moved live data to `~/Documents/DungeonAdventures/Realm Data`.

## [x] Track: Library extraction
Split reusable code into the `Maries_Ui`, `Maries_DataModels` and `Maries_Loaders` class libraries.

## [x] Track: Terminal UI engine and loading sequence (superseded)
The original "Hostile Terminal" pass on the UI engine and boot sequence. Its visual rules were replaced by the fantasy
re-theme; the 84x24 safe zone and cursor-positioned rendering were kept.

---

## [ ] Track: Next up
- Open the Healer in Hollowmere.
- Give the Wisdom attribute an effect.
- Add ASCII art for monsters and bosses (`ArtRef`), and use `AuraIntensity`.
- Unlock the SPECIAL combat action per class.
