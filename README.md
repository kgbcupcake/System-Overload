# ⚔️ Dungeon Adventures – Reborn

> **"Few who descend ever return."**

[![Version](https://img.shields.io/badge/version-v1.0.0-yellow?style=for-the-badge)](https://github.com/kgbcupcake/System-Overload)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Branch](https://img.shields.io/badge/branch-Dev-orange?style=for-the-badge)](https://github.com/kgbcupcake/System-Overload/tree/Dev)
[![License](https://img.shields.io/badge/License-MIT-red?style=for-the-badge)](LICENSE)

**Dungeon Adventures – Reborn** is a terminal fantasy dungeon crawler written in C# on .NET 10. Make a hero, gear up in the village of Hollowmere, then descend floor by floor through crypts, caverns and catacombs, fighting monsters and the dungeon lords who guard each floor, until you face the Dread King.

Everything is drawn inside a fixed **84×24** window: no scrolling, just torchlight, gold and a lot of things that want you dead.

---

## 🗡️ Features

- **Dungeon floors**: four floors, each told in short story stages, with its own monsters and a boss at the end.
- **Heroes**: three classes (Warrior, Rogue, Mage), each with three paths, and attribute points for Vitality, Strength and Fortitude.
- **Turn-based combat**: weapons, gems, potions, critical hits, and bosses who enrage at half health.
- **The village of Hollowmere**: the General Store, Dungeon Gate, Blacksmith (gem socketing), character sheet and inventory, with save and load between runs.
- **Data-driven content**: monsters, bosses, floors, weapons, items, gems, shops and every line of story text are plain JSON, editable by hand or with the MariesGui editor.
- **Colourful UI**: 24-bit colour via `Pastel` and `Spectre.Console`, with flicker-free redraws locked to the 84×24 grid.

---

## 🔌 Getting Started

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A terminal with UTF-8 and 24-bit colour support (Windows Terminal, GNOME Terminal, Konsole, kitty, etc.), sized to at least **84×24**

### Build and run

```bash
git clone https://github.com/kgbcupcake/System-Overload.git
```

```bash
cd System-Overload
```

```bash
dotnet run --project Dungeon-Adventures.csproj
```

On launch, press **S** to skip the intro or any other key to watch it. Use the arrow keys and **Enter** to move through menus.

### Install as a .NET tool (optional)

The project is set up as a .NET global tool whose command is `dungeon`:

```bash
dotnet pack Dungeon-Adventures.csproj -c Release
```

```bash
dotnet tool install --global --add-source ./bin/Release Dungeon-Adventures
```

```bash
dungeon
```

### Clean build

`dotnet clean` runs the `SpookyCleanup` target, which deletes `bin/` and `obj/` on both Windows and Linux.

---

## 💾 Game Data and Saves

On first launch the game creates its working folder and seeds it from the templates in the repo:

| What | Where |
|:---|:---|
| Default content (source of truth in the repo) | `GameData/Templates/<type>/*.json` |
| Live content and save profiles (what the game reads) | `~/Documents/DungeonAdventures/Realm Data/` |

The content types are `bosses`, `monsters`, `floors`, `weapons`, `items`, `gems`, `shops` and `story`. Editing files in the live folder changes your local game. To change the defaults everyone gets, edit `GameData/Templates/`. The seeder only copies files that don't exist yet, so delete a live file to get the new default.

### Story text

No story is written in the code. Every narrative line (intro, loading screens, class descriptions, shopkeeper chatter, combat and dungeon messages, the farewell) lives in [`GameData/Templates/story/story_core.json`](GameData/Templates/story/story_core.json) as a key with one or more lines. Lines can use tokens such as `{name}`, `{class}`, `{enemy}` and `{title}`; when a key has several lines, the game picks one at random. In dev mode a missing key shows as `<key>` on screen.

Each floor in `floors/floors_core.json` can also set its own `CompletionText`.

### Editing with MariesGui

Content classes are tagged with `[MariesGuiContent]`, and a ready-made profile ships at [`Src/DevTools/mariesgui-manifest.json`](Src/DevTools/mariesgui-manifest.json):

1. Build the game (`dotnet build`) so `bin/Debug/net10.0/Dungeon-Adventures.dll` exists.
2. In MariesGui, open **Load Profile** and select the manifest.
3. Edit content. Changes are written to `~/Documents/DungeonAdventures/Realm Data/`.

---

## 🏰 Project Layout

```
System-Overload/            (repository folder)
├── Dungeon-Adventures.sln
├── Dungeon-Adventures.csproj
├── GameData/Templates/     Default JSON content (bosses, monsters, floors, weapons, items, gems, shops, story)
├── Src/
│   ├── Program.cs          Entry point: seeding, loaders, intro, main menu
│   ├── Interfaces/         Main menu, character creation, load screen, stats, GameUiEngine
│   ├── Village/            Hollowmere: VillageSquare hub
│   │   └── Places/         Store, DungeonGate, Blacksmith, InventoryScreen
│   ├── GameEngine/
│   │   ├── Flow/           FloorRunner (plays a floor stage by stage)
│   │   └── Systems/        Combat, shops, quest log
│   ├── GameData/           Game state and templates
│   ├── Dev/                Content seeder
│   ├── DevTools/           MariesGui manifest
│   ├── Maries_Ui/          Class library: console UI (HUD, transitions, menus, alerts, art)
│   ├── Maries_DataModels/  Class library: content data classes + MariesGui attribute
│   └── Maries_Loaders/     Class library: JSON loaders (bosses, monsters, floors, shops, items, story)
├── conductor/              Design docs: product vision, tech stack, style guides, workflow
├── assets/                 Icons and installer art
└── CHANGELOG.md
```

### The stack

| Component | Library | Role |
|:---|:---|:---|
| **Engine** | .NET 10 (C#) | Game engine and logic |
| **Rendering** | `Pastel`, `StringBuilder` | 24-bit colour, pre-rendered frames |
| **Interface** | `Spectre.Console` | Panels, tables and structured layout |
| **AI (planned)** | `Ollama`, `OpenAI`, `Microsoft.Extensions.AI` | AI-driven dialogue (in progress) |
| **Tests** | `NUnit` | Test framework |

---

## 🧪 Dev Status

This is the `Dev` branch. Expect unfinished features and rough edges.

| Area | Status | Focus |
|:---|:---|:---|
| **Core loop** | 🟢 Stable | Village, floors, save/load, content loading |
| **Combat** | 🟢 Stable | Bosses, enrage, consumables, loot drops |
| **Art** | 🟡 Evolving | Monster and boss art, flicker-free buffers |
| **Loot** | 🟡 Evolving | Gem drops and loot tables |
| **Village** | 🟡 Evolving | The Healer is not open yet |

### To do

- [ ] **ARMORY**: more weapons and items
- [ ] **GEMS**: loot drop tables and gem effects
- [ ] **BESTIARY ART**: give monsters and bosses ASCII/Spectre art
- [ ] **HEALER**: open the Healer in Hollowmere
- [ ] **WISDOM**: make the Wisdom attribute do something

---

## 🤝 Contributing

Read the design docs in [`conductor/`](conductor/) first, especially [`tech-stack.md`](conductor/tech-stack.md) for the rendering rules (84×24 safe zone, `SetCursorPosition` + `Write` instead of `WriteLine`, pre-rendered `StringBuilder` frames). See [`CHANGELOG.md`](CHANGELOG.md) for history and [`SECURITY.md`](SECURITY.md) for reporting vulnerabilities.

---

## 📜 License

MIT © 2026 Kgbcupcake. See [LICENSE](LICENSE).
