# 📜 Chronicle of Changes: Dungeon Adventures – Reborn

> **"Few who descend ever return."**

Newest entries first. Older entries are kept exactly as they were written, old names and all.

---

## ⚔️ [Unreleased] - REBORN
**Status:** `IN DEVELOPMENT` | **Date:** 2026-11-09

The game returns to its fantasy roots as **Dungeon Adventures – Reborn**.

### Added
- **Heroes:** Warrior, Rogue and Mage classes, each with three paths; attribute points for Vitality, Strength and Fortitude.
- **Hollowmere:** the village hub with the General Store, Dungeon Gate, Blacksmith, Inventory and Character sheet.
- **Dungeon floors:** The Sunken Crypt, The Ember Caverns, The Lich's Catacombs and The Dread Throne, each ending in a boss
  (The Crypt Warden, Vexara the Ember Drake, Malgrath the Bone Lich, The Dread King).
- **Bestiary and loot:** fantasy monsters, potions, tonics, weapons and gems.
- **Story text in data:** every narrative line now lives in `GameData/Templates/story/story_core.json`, looked up by key
  with `{tokens}` and random variations. Floors can set their own `CompletionText`.
- **Frame titles:** menu boxes now show their title in the top border.
- **Battle screen:** fights show the foe top-right facing you and the hero bottom-left from behind, drawn from the LPC
  catalog, with a two-line battle log and a 2x2 command box. Foes flash when hit and sink out of sight when they fall;
  bosses stand tall on their intro screen.
- **Rolled foes:** every monster and boss gets a fresh LPC look each fight (rats, skeletons, wolves, goblins, orcs,
  sentinels, liches...), picked by keywords in its id and name, with random clothes, armour, weapons and colours.
- **Stage scenes:** each floor's story lines play over a painted room in the floor's style (crypt stone, lava veins,
  green-lit catacombs, the bannered throne hall) with flickering torches, as the hero walks in. In the crypt the hero
  squeezes through a cracked tombstone and drops out of sight. Any key skips the walk.
- **Race and background:** character creation now asks for your people (Human, Elf, Orc, Goblin) and the life you
  led before (Wanderer, Villager, Apprentice, Soldier, Outcast), right after your name. The race sets which heads,
  ears and skin colours the look screen offers. The background adds a point to one attribute, some gold and an item
  or two, and fills the new `{race}` and `{background}` story tokens. Both live in data (`races`, `backgrounds`) with
  their text in `story_origins.json`, and the character sheet shows them.
- **Walkable village:** Hollowmere is now a street at dusk you walk with ← →: the hero's home, the General Store,
  the healer, the well, the blacksmith's forge (glowing coals, sparks off the anvil) and the gate on the road to the
  dungeon, with smoking chimneys, lit windows and lamps, and hills that scroll slower than the street. The villagers
  stroll about; walk up and press T to talk. ↑ goes in, J/C/I open the journal, hero and bag, and clicking a building
  or villager walks there. Tab opens the old list menu, which has WALK THE STREET to come back.
- **Floor intros:** a floor opens with the hero coming down a stair into its room, with the floor's name, danger and
  description in the box below.

### Changed
- **Crisp sprites:** characters are shrunk by picking the most common colour of each area instead of blending,
  so they keep hard pixel edges in the creator, fights and scenes.
- **Scene framework:** the floor scenes are built from reusable pieces in `Src/Scenes`. A `Scene` runs a short
  script (`WalkTo`, `SinkOut`, `Pause`, `Say`) in a `Room` painted in a `RoomStyle`, dressed with `Prop`s (torches,
  lava veins, banners, skull niches, bones, a cracked tombstone, a stair), with LPC `Actor`s and a `StoryBox`. Floors
  can set `Room` and stages can set `Scene` ("walk", "squeeze", "none") in their JSON; when empty, both are guessed
  from the text.
- **Smaller classes:** combat rewards (`CombatOutcome`), the bag (`CombatBag`), the boss intro (`BossIntro`), boss looks
  (`BossLooks`) and look helpers (`LookParts`) are now their own classes. So are mouse input (`TerminalInput`, `ClickZones`,
  `WindowsConsoleMode`), the loading and goodbye screens (one class each, in place of `Transitions`), the item panels
  (`PanelMenu`, `ItemDetails`, `ItemInfo`, `PanelText`, `PanelHotspots`) and the character creator (`LookRows`,
  `LookEditor`, `MakeCharacterView`). `CLAUDE.md` sets the rule: no file over 350
  lines, and no god classes.
- **Renamed everything:** project and solution (`Dungeon-Adventures`), namespaces (`DungeonAdventures.*`), the tool
  command (`dungeon`), classes (`VillageSquare`, `DungeonGate`, `Blacksmith`, `InventoryScreen`, `FloorRunner`,
  `BossLoader`, `MonsterLoader`, `FloorLoader`, ...), content folders (`bosses`, `monsters`, `floors`) and JSON fields.
- **Live data** moved to `~/Documents/DungeonAdventures/Realm Data`.
- **Palette:** torch gold, ember and blood red replace neon cyan and green.

### Fixed
- **Max HP:** Vitality points were counted twice, and the HUD showed base HP instead of max HP.
- **Missing classes:** some subclasses fell through to default HP; all nine paths now have their class stats.

### Removed
- `MainEncounter.cs`, an unused encounter full of hardcoded text.

---

## 🟢 [v1.0.0] - PATIENT ZERO (THE SYSTEM OVERLOAD UPLINK)
**Status:** `ACTIVE_MALIGNANCY` | **Date:** 2026-01-21

This is the definitive stabilization of the System-Overload entity. The old 'Dungeon-Adventures' architecture has been fully consumed and restructured into a high-performance terminal engine.

### 💉 INFECTED LOGIC (Added)
- **Identity Overload:** Renamed all internal namespaces, assembly outputs, and solution structures to `System-Overload`.
- **Neural Core Initialization:** Integrated `Ollama` and `OpenAI` libraries into the Forge Core for sentient dialogue.
- **Protocol Update:** Migrated terminal commands to the `so-` prefix (e.g., `so-ship`, `so-pulse`).
- **84x24 Reality Anchor:** Established the fixed-coordinate grid to prevent terminal buffer overflows.
- **SpookyCleanup Integration:** Injected the cross-platform force-purge logic to ensure clean compilation cycles.

### ☣️ CORRUPTIONS (Suppressed)
- **Metadata Purge:** Successfully nuked all `Zone.Identifier` ghost files that were haunting the repository.
- **Path Stabilization:** Resolved Windows file locks through a forced system reset to enable the directory rename.
- **UI Decoupling:** Removed `ImGui` dependency to minimize the engine footprint (01/17/26).

---

## 🛠️ LOGGING PROTOCOL
1. **Newest first:** Add new entries at the top, under `[Unreleased]` until a version is tagged.
2. **No deletions:** Do not remove or rewrite old entries; they record the project's history.
3. **Categories:** `Added`, `Changed`, `Fixed`, `Removed`. (The v1.0.0 entry uses its original names: `INFECTED LOGIC` for
   added systems and `CORRUPTIONS` for fixes.)

---
**[END OF CHRONICLE]**