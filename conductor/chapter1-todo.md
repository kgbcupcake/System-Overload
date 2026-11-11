# Chapter 1 "The Road to the Unknown": to-do list for agents

This list compares the Chapter 1 design doc (prologue, character creation, A Life Before, Ashford, *A Place Left
Behind*, *The Northern Trail*, *Beyond Home*, side quests) with the code as of 2026-11-10. Each task stands on its
own: give an agent the **Rules** section and one task.

## Rules for every agent

- Read `CLAUDE.md` first. **No file over 350 lines** (aim for 300). **No god classes**: one job per class, one class
  per file, and no `partial` classes used to dodge the limit.
- **Don't commit.** Leave the changes in the working tree and finish with a suggested commit message.
- Story and dialogue text goes in `GameData/Templates/story/*.json` (looked up with `Story.Line(key)`), never in C#.
  Put a new chapter's text in a **new** story file. The seeder copies new files and appends new keys to the
  player's copy in `~/Documents/DungeonAdventures/Realm Data`.
- Content is data: quests (`quests`), NPCs (`npcs`), things to examine (`points_of_interest`), places (`floors`),
  races, backgrounds and items. Quests react to events through `QuestSystem.Notify(player, type, target)`. The event
  types are `talk_to`, `examine`, `collect_item`, `discover_location` and `defeat_enemy`. Add a new event type rather
  than writing quest-specific code.
- Keep each file's line endings. Several files are CRLF, so check `git diff --stat` for whole-file rewrites.
- Check your work with `dotnet build Dungeon-Adventures.sln`. There's no test project. For game logic, write a small
  console project in your scratch space that references `Dungeon-Adventures.csproj` and sets
  `LoaderConfig.ContentRoot` to `GameData/Templates`.
- Add a line to the `[Unreleased]` section of `CHANGELOG.md`.

## Already done

| Doc section | State |
|---|---|
| Prologue, "The Vanishing" | Done: `story_prologue.json`, `StoryPages` |
| Character creation | **Done (Phase 2):** name; race (Human, Elf, Orc, Goblin), which sets head, ears and skin; background (Wanderer, Villager, Apprentice, Soldier, Outcast), which gives +1 attribute, gold and items; calling and path; look; body type (female/male) as gender; attribute points. Data lives in `races/`, `backgrounds/` and `story_origins.json`. Tokens: `{race}`, `{background}`. Not yet checked by playing the creator in a real terminal. |
| A Life Before | Done: loved one's name, relationship (4 choices), the fireside memory (`LifeBefore`, `story_chapter1.json`) |
| Quest engine and data | Done: `QuestSystem`, `PlayerQuestLog`, `quests_chapter1.json`, story flags |
| *A Place Left Behind* | Done as data: belongings and keepsake, Edrin/Mara/Tomas, the last place (markings, tracks), return, second disappearance |
| *The Northern Trail* | Done as data: Edrin's news, trail, wolf, camp, journal, report back |
| Edrin's closing speech | Done: `ch1.beyond_home` |

## Tasks

Tasks marked **[parallel]** touch their own files and can run alongside each other. The others list what they
depend on.

### 1. Gate places behind story flags
Right now the Dungeon Gate lists every floor from the start, so a new hero can walk into the Northern Trail or
the Dread Throne before the story sends them there.
- Add `RequiresFlag` to `FloorData` (as `NpcData` and `PointOfInterest` already have), and hide or lock floors in
  `DungeonGate` until the hero has that flag.
- Quest stages need flags too. Add an optional per-stage `CompletionFlag` to `QuestData` stages and set it in
  `QuestSystem`, so that, for example, `last_known_location` opens when `ask_around` is done. Use the doc's flag
  names: `home_examined`, `elder_spoken`, `innkeeper_spoken`, `guard_spoken`, `personal_item_found`,
  `strange_markings_found`, `camp_discovered`, `journal_found`, `enemies_defeated`.
- The four dungeon floors require `open_world_unlocked`.
- **Done when** a new hero sees only what the current quest stage allows, and old saves still load.
- Files: `FloorData.cs`, `QuestData.cs`, `QuestSystem.cs`, `DungeonGate.cs`, `floors_core.json`, `quests_chapter1.json`.

### 2. Settle "Hollowmere" vs "Ashford" **[parallel]**
The village is Ashford, but the village menu still says "ROAD TO HOLLOWMERE" and `DungeonGate` says "return to
Hollowmere". The doc makes Hollowmere a later region ("The Bell Beneath Hollowmere"), and the existing bell story
lines (`story.bell.*`) and dungeon floors already fit that.
- Rename the village menu option (e.g. "THE ROADS OUT") and the return text to Ashford. Keep Hollowmere as the
  name of the dungeon region.
- **Ask the user** before renaming any story text that changes the lore.
- Files: `VillageSquare.cs`, `DungeonGate.cs`, the story JSON.

### 3. Open the Healer **[parallel]**
`HEALER` is on the village menu but does nothing. Add a Healer place (restore HP for gold, sell Healing Potions)
in the two-panel layout used by `Store` and `Blacksmith`, with its own class in `Src/Village/Places/Healer.cs`. It
could also host the herbalist from task 10.

### 4. Ashford's places: tavern, elder's house, outskirts, Bram
The doc's Ashford has a home, square, tavern, general merchant, blacksmith, elder's house, outskirts and a small
forest. The code has a single "TALK TO VILLAGERS" list holding Edrin, Mara and Tomas.
- Add the Tavern (Mara, plus rumours from task 9), the Elder's house (Edrin) and the Outskirts (Tomas, and the
  road to the last place) as locations. NPCs and points of interest already filter by `Location`.
- Add **Bram the blacksmith** as an NPC with a greeting and an intro to equipment. Have the Blacksmith screen
  greet with his name.
- **Ask the user** whether the village should stay a menu or become a painted map built from `Src/Scenes` (Room,
  Props, Actors). The doc says "starting village map".
- Files: `VillageSquare.cs` (keep it under 350 lines by moving each place into its own class),
  `npcs_core.json`, `points_of_interest_core.json`, a new story file.

### 5. Waking up at home
The doc's Stage 1 says to leave the bedroom, explore the house and examine her belongings, opening with the "Morning
light filters through the window..." narration.
- Play `ch1.wake_up` as a short scene with the `Src/Scenes` framework (a home `RoomStyle`, the hero walking from the
  bed), then open the home's look-around.
- Add home points of interest: her book beside the bed, the clothes still hanging, and the keepsake (already there).
- Files: a new `Src/Scenes/Props/*` if needed, `points_of_interest_core.json`, `story_chapter1.json`.

### 6. Gathering
The doc has "gather useful resources" on the Northern Trail and a small forest for gathering, and the Blacksmith's
Request and Herbalist side quests need it. Nothing gathers yet.
- Add gatherable nodes as data (herbs, wood, ore) at a location, a `gather` quest event, and items in `items_core.json`.
- Add a `Small Forest` location near Ashford (repeatable, wolves and slimes, gathering).
- Add an optional "gather 3 herbs" objective to `travel_to_camp`.
- Depends on task 1 for gating the forest.

### 7. Chapter 1 rewards and the open world
When `main_vanishing_northern_trail` completes, the doc says the hero receives an **old regional map**, a **basic
travel supply package**, their first main-story journal entry, and access to the wider world.
- Add the items (`Old Regional Map`, a quest item; `Travel Supplies`, a bundle of potions and rations) and put them
  in the quest's `Rewards.Items`.
- Show a "Beyond Home" chapter-end screen (`ch1.beyond_home`, plus a "Chapter 1 complete" card), then open the
  roads out of Ashford.
- Add a region list (the doc suggests Hollowmere, Ancient Kingdom, Northern Highlands, Sunken Coast and The Veil)
  with only Hollowmere open. The others show as locked "rumoured" places.
- Depends on task 1 (`open_world_unlocked`) and task 2 (naming).

### 8. First fight tutorial **[parallel]**
The first wolf on the Northern Trail should teach the basic attack, health and damage, enemy drops, loot pickup,
and equipping what dropped. Show one-time hint lines (story keys `tutorial.*`) the first time each happens, and save
which ones the hero has seen as story flags. Keep the hints in their own class (e.g. `CombatTutorial`), not inside
`BattleScene` or `CombatEncounter`.

### 9. Background- and race-aware dialogue **[parallel]**
Backgrounds are meant to "unlock unique dialogue".
- Add a story lookup that tries `key.<background>`, then `key.<race>`, then `key` (e.g. `npc.tomas.greeting.soldier`).
  Put it in a small class beside `StoryTokens`, not in the loader.
- Use it for NPC greetings and quest talk lines, and write variants: Tomas for soldiers, Edrin for apprentices,
  Mara for wanderers and goblins, and so on.
- Also use the relationship (`LovedOneRelation`) for a few lines, since the doc says it shapes emotional responses.
- Files: `PeopleAndPlacesScreens.cs`, a new helper class, a new story file.

### 10. Side quests (one agent each once tasks 1, 4 and 6 are in)
Each is a quest in a new `quests_chapter1_side.json` plus its story file. None of them blocks the main quest.
| Quest | Teaches | Needs |
|---|---|---|
| Missing Livestock | tracking, exploring | task 4 (outskirts) |
| A Blacksmith's Request | gathering, crafting | tasks 4 (Bram) and 6; a crafting recipe at the Blacksmith |
| Trouble on the Road | combat, drops | task 1 |
| The Old Well | hidden places | task 4; a point of interest that opens a hidden location |
| A Traveler's Tale | lore, rumours | task 4 (tavern); hints toward Hollowmere and the distortions |
| Helping the Herbalist | gathering, consumables | tasks 3 or 4, and 6 |
Rewards: gold, gear, materials, XP. Reputation doesn't exist yet, so leave it out or ask.

### 11. Race and background icons **[parallel]**
The pick screens look for `assets/icons/race_<name>.png` and `background_<name>.png` (16x16, see
`assets/icons/README.md`) and show nothing when they're missing. Draw or source 9 icons, and credit them in the README.

### 12. Full Chapter 1 playthrough (last)
Play from New Game to the open-world unlock with each relationship choice and a few race/background mixes. Check
that every `{token}` fills in, that no raw `<story.key>` shows, that no step soft-locks, and that old saves (no race,
no flags) still load. Aim for the doc's 30 to 60 minutes. Report the problems you find; don't redesign.

## Story promise to keep
The loved one is alive and will be found. No fake death, and no ending that takes her away again. The difficulty
comes from the world, its dangers and its mysteries.
