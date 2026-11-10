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

### Changed
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