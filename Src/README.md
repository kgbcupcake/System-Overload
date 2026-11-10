# ⚔️ Dungeon Adventures – Reborn: Source Guide

A tour of the code in `Src/`. For building, running and editing content, see the [main README](../README.md).

---

## 🏰 How the game fits together

```text
Program.cs
  ├─ seed content      ContentSeeder copies GameData/Templates → ~/Documents/DungeonAdventures/Realm Data
  ├─ load content      ItemRegistry, BossLoader, FloorLoader, Story, QuestLoader
  ├─ intro             Transitions (skippable with S)
  └─ MainMenu
       ├─ new hero     StoryPages (prologue) → CharacterCreation → CharSubMenus → LifeBefore → VillageSquare
       └─ load hero    CharacterLoadScreen → VillageSquare

VillageSquare (Ashford)
  ├─ GENERAL STORE     Places/Store
  ├─ ROAD TO HOLLOWMERE Places/DungeonGate → FloorRunner → CombatEncounter
  ├─ BLACKSMITH        Places/Blacksmith (socket gems into weapons)
  ├─ HEALER            not open yet
  ├─ JOURNAL           Interfaces/QuestScreens (active and finished quests)
  ├─ CHARACTER         Interfaces/Stats
  ├─ INVENTORY         Places/InventoryScreen
  └─ LEAVE THE REALM   farewell sequence and exit
```

All content (monsters, bosses, floors, items, weapons, gems, shops) and **all story text** comes from JSON. The C# only holds menu labels, key hints and game rules.

---

## 🧙 Character creation

`Interfaces/CharacterCreation.cs` and `Interfaces/CharSubMenus.cs`.

| Class | Paths | Starting HP | Starting kit |
|:---|:---|:---|:---|
| **Warrior** | Knight, Paladin, Berserker | 150 | Iron Warhammer, 2 Healing Potions, 50 gold |
| **Rogue** | Assassin, Shadowblade, Ranger | 100 | Whisper Dagger, Lockpick Set, 150 gold |
| **Mage** | Wizard, Warlock, Battlemage | 120 | Oak Staff, Mana Crystal, 25 gold |

The hero then spends 10 attribute points:

| Attribute | Field on `PlayerData` | Effect |
|:---|:---|:---|
| Vitality | `VitalityPoints` | +15 max HP each |
| Strength | `StrengthPoints` | +1 damage per hit each |
| Fortitude | `FortitudePoints` | +2 defense each |
| Wisdom | `WisdomPoints` | +1% critical hit chance each |

Max HP is always read through `PlayerData.GetTotalMaxHealth()` (base `HitPoints` + Vitality + gem bonuses). Show that, not `HitPoints`, anywhere the player sees their max HP.

Path descriptions are story lines named `class.<path>`, for example `class.knight`.

---

## 🗝️ Dungeon floors

`Village/Places/DungeonGate.cs` lists the floors from `FloorLoader`. Picking one runs `GameEngine/Flow/FloorRunner.cs`, which:

1. shows the floor's title, description and danger rating,
2. walks through its `Stages`, printing each `StoryText` in the stage's colour,
3. starts a fight whenever a stage has an `EnemyId`,
4. pays `CompletionGold` and `CompletionExp` and shows `CompletionText` (or the `dungeon.complete` story line) when every stage is cleared.

`PlayerQuestLog` tracks each floor as uncleared, exploring or cleared.

A floor (`FloorData`) looks like this:

```json
{
  "Id": "dungeon_01_crypt",
  "Title": "FLOOR 1 // THE SUNKEN CRYPT",
  "Description": "An old burial vault beneath the village...",
  "Difficulty": "EASY",
  "CompletionGold": 60,
  "CompletionExp": 50,
  "CompletionText": "The Warden's keys fall silent. The crypt is quiet at last.",
  "Stages": [
    { "StoryText": "You squeeze past a cracked tombstone...", "EnemyId": "", "HexColor": "#7FB3A0" },
    { "StoryText": "A Giant Cave Rat...", "EnemyId": "cave_rat", "HexColor": "#7FB3A0" },
    { "StoryText": "THE CRYPT WARDEN steps out...", "EnemyId": "boss_crypt_warden", "HexColor": "#7FB3A0" }
  ]
}
```

---

## ⚔️ Combat

`GameEngine/Systems/Combat/`:

| File | Role |
|:---|:---|
| `EnemyFactory.cs` | Turns an `EnemyId` into a `Combatant`. Monster ids come from `MonsterLoader`, boss ids from `BossLoader`. An unknown id becomes a placeholder "Unknown Creature" so the floor stays playable. |
| `Combatant.cs` | One side of a fight. Bosses also carry a `BossProfile` (moves, special ability, enrage, dialogue). |
| `CombatCalculator.cs` | Hit chance, damage rolls and critical hits (`DamageCalculator`). |
| `CombatEncounter.cs` | The fight loop: Attack, Inventory, Special (locked) and Retreat. You can't retreat from a boss. |
| `CombatMapper.cs` | Maps player data into a combatant. |

Bosses:

- speak their `OnEncounter` line when the fight starts,
- use their special ability every `SpecialEvery` turns (1.5× damage, can't miss),
- **enrage** below half health: they speak `OnDamage` and hit `EnrageMultiplier` times harder,
- speak `OnDeath` when they fall, then roll their `DropTable`.

Consumables in combat: `Restorative` heals by the item's `Value`, `Defense` gives +5 DEF for the fight, and `Speed` makes the enemy lose its next turn.

### Monster JSON (`monsters/`, `MonsterData`)

```json
{ "id": "cave_rat", "name": "Giant Cave Rat", "maxHealth": 30, "attackPower": 6, "defense": 1,
  "level": 1, "accuracy": 85, "critChance": 5, "attackVerb": "bites",
  "rewards": { "xp": 10, "coins": 5 } }
```

### Boss JSON (`bosses/`, `BossDefinition`)

```json
{
  "Id": "boss_crypt_warden",
  "Name": "THE CRYPT WARDEN",
  "Rank": "DUNGEON_LORD",
  "Level": 3, "XpReward": 80, "CoinReward": 50,
  "Stats": { "Health": 110, "Attack": 14, "Defense": 6, "EnrageMultiplier": 1.2 },
  "Moves": { "PrimaryAttack": "Grave Chill", "SecondaryAttack": "Rattling Flail",
             "SpecialAbility": "Rise of the Restless", "SpecialEvery": 3 },
  "Visuals": { "HexColor": "#7FB3A0", "AuraIntensity": 0.3, "ArtRef": "crypt_warden" },
  "DropTable": [ { "ItemID": "Healing Potion", "Chance": 0.9 } ],
  "Dialogue": { "OnEncounter": "...", "OnDamage": "...", "OnDeath": "..." }
}
```

`AuraIntensity` and `ArtRef` are loaded but not used yet.

---

## 🔨 Weapons, gems and the Blacksmith

`WeaponData` and `GemData` live in `Maries_DataModels/Components/`. Weapons have sockets (`NumSockets`). At the **Blacksmith** (`Village/Places/Blacksmith.cs`) the hero picks a weapon, then a gem from their pack; the gem moves into `SocketedGems`. Socketed gems with a Fire, Ice, Poison or Electric attribute add their `Power` to the hero's damage; Restorative or Defense gems add it to max HP. Other attributes (such as LifeLeech) have no effect yet.

Weapon types used by the default content: `Heavy`, `Melee`, `Ranged` and `Magic`.

---

## 🎒 Inventory and the store

- **Inventory** (`Village/Places/InventoryScreen.cs`) groups items by name and shows type, rarity and effect. Enter uses an item, X or Delete drops it.
- **General Store** (`Village/Places/Store.cs`) sells Healing Potions from a stock that refills every 30 minutes and gets pricier with each purchase, and the **Dark Pact**, which raises the hero's danger rank (harder foes, better sell prices). Tab switches to selling, where prices depend on rarity and danger rank.

---

## 📜 Story text

`Maries_Loaders/StoryLoader.cs` (`Story`) reads `story/*.json`, a list of `StoryEntry` (`Key` + `Lines`).

```csharp
Story.Line("dungeon.enter", ("title", floor.Title));   // first line
Story.Pick("combat.victory", ("enemy", enemy.Name));    // random line
Story.Lines("loading.labels");                          // all lines
```

Rules:

- No narrative or flavour text goes in C#. Add a key to a file in `GameData/Templates/story/` and look it up.
- Tokens are written `{name}` and filled by the caller.
- Some tokens work everywhere without being passed (`GameEngine/Systems/StoryTokens.cs`): `{hero}`, `{loved}` (the
  name chosen in "A Life Before", or `life_before.default_name`), `{relation}` and `{relation_memory}`.
- In dev mode a missing key shows as `<key>`, so gaps are easy to spot.

Paged scenes such as the prologue use `Interfaces/StoryPages.cs`: `StoryPages.Play("prologue")` reads
`prologue.title`, `prologue.page.1`, `prologue.page.2`, ... and `prologue.final`. Add a page by adding the next key.

---

## 🧭 Quests and story flags

`quests/*.json` holds `QuestData` (`Maries_DataModels/Components/QuestData.cs`), run by
`GameEngine/Systems/QuestSystem.cs`. Quests never run gameplay themselves. The game reports events and sets flags:

```csharp
QuestSystem.Notify(player, "defeat_enemy", enemy.Name);   // also: talk_to, examine, discover_location, collect_item
QuestSystem.SetFlag(player, "relationship_established");
```

- A quest starts as soon as all its `Prerequisites` flags are set (unless `AutoStart` is false).
- Each stage finishes when its required objectives are done; an objective of type `flag` is done once that flag is set.
- Finishing a quest pays its `Rewards` and sets its `CompletionFlags`, which can start the next quest.
- Targets match loosely: `"Forest Wolf"` matches `"forest_wolf"`.
- Progress and flags are saved on the hero (`Quests`, `StoryFlags`). Notices are shown by `Interfaces/QuestScreens.cs`,
  which also draws the JOURNAL.

Hooked up today: `defeat_enemy` and `collect_item` (combat and loot), `discover_location` (entering a floor uses the
floor's `Id`). `talk_to` and `examine` are waiting on NPCs and the home investigation.

---

## 💾 Saves and content folders

| Folder (under `Realm Data/`) | Class | Loader |
|:---|:---|:---|
| `bosses/` | `BossDefinition` | `BossLoader` |
| `monsters/` | `MonsterData` | `MonsterLoader` |
| `floors/` | `FloorData` | `FloorLoader` |
| `items/`, `weapons/`, `gems/` | `ItemData`, `WeaponData`, `GemData` | `ItemRegistry` |
| `shops/` | `ShopDefinition` | `ShopLoader` |
| `story/` | `StoryEntry` | `Story` |
| `profiles/` | `PlayerData.loadPlayer` | `SaveGame` / `LoadGame` |

`GameState.MasterPath` sets the root, and `LoaderConfig.ContentRoot` passes it to the loader library. `ContentSeeder` only copies template files that don't already exist, so it never overwrites edits.

---

## 🖥️ UI layers

| Layer | Where | What |
|:---|:---|:---|
| `Maries_Ui` | `Src/Maries_Ui/` | Reusable console UI: `UiEngine` (frames with titled borders, centring), `HudChrome` (title bar, footer), `Transitions` (intro, load, creation and farewell sequences), `MenuNavigator`, `Alerts`, `ProgressBarRenderer`, `SparkText`, `PlatformConsole`, `ConsoleSetup`, `GameArt` |
| Game UI | `Src/Interfaces/UI/GameUiEngine.cs` | Screens that need game state, such as the hero stat box |

`Interfaces/DevGui_Main.cs` is an old ImGui dev panel. It is excluded from the build.

---

## 🧱 Source layout

```text
Src/
├── Program.cs
├── Dev/ContentSeeder.cs
├── DevTools/mariesgui-manifest.json
├── GameData/
│   ├── Components/GameState.cs
│   └── Templates/ModdingStation.cs
├── GameEngine/
│   ├── Conductor.cs, Loadgame.cs, SaveGame.cs
│   ├── Flow/FloorRunner.cs
│   └── Systems/
│       ├── Combat/        Combatant, CombatCalculator, CombatEncounter, CombatMapper, EnemyFactory
│       ├── GlobalShopManager.cs
│       └── PlayerQuestLog.cs
├── Interfaces/
│   ├── MainMenu.cs, CharacterCreation.cs, CharSubMenus.cs, CharacterLoadScreen.cs, Stats.cs
│   └── UI/GameUiEngine.cs
├── Village/
│   ├── VillageSquare.cs
│   └── Places/            Store, DungeonGate, Blacksmith, InventoryScreen
├── Maries_DataModels/     Data classes (FloorData, MonsterData, PlayerData, ItemData, WeaponData, GemData, StoryEntry, ...)
├── Maries_Loaders/        BossLoader, MonsterLoader, FloorLoader, ShopLoader, StoryLoader, UniversalLoader, JsonContentWriter
└── Maries_Ui/             Console UI library
```

`DungeonData`, `EnemyData`, `BossData`, `BossAttribute` and `BossTraits` in `Maries_DataModels/Entities/` are older models that nothing uses at the moment.

---

## ⚙️ Technology

| Component | Technology |
|:---|:---|
| Runtime | .NET 10 |
| Language | C# |
| Colour | Pastel (24-bit ANSI) |
| Layout | Spectre.Console |
| Data | System.Text.Json |
| Tests | NUnit (no test project yet) |
| AI (not implemented) | Ollama, OpenAI, Microsoft.Extensions.AI |

---

## 📜 License

Copyright (c) 2026 Kgbcupcake. Released under the MIT License; see [`LICENSE`](../LICENSE).
