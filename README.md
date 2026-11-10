# ⚡ [SYSTEM_OVERLOAD]

> **"THE SYSTEM IS NOT BROKEN. IT IS SIMPLY EVOLVING BEYOND YOUR CONTROL."**

[![Version](https://img.shields.io/badge/version-v1.0.0-yellow?style=for-the-badge)](https://github.com/kgbcupcake/System-Overload)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Branch](https://img.shields.io/badge/branch-Dev-orange?style=for-the-badge)](https://github.com/kgbcupcake/System-Overload/tree/Dev)
[![License](https://img.shields.io/badge/License-MIT-red?style=for-the-badge)](LICENSE)

**System Overload** is a terminal dungeon crawler written in C# on .NET 10. You play inside a dying machine intelligence, breaching its corrupted memory sectors, fighting rogue drones and sector admins, and scavenging loot before the hardware collapses for good.

Everything is drawn inside a fixed **84×24 "Reality Anchor"**: no scrolling, no safety, no recovery. If your terminal starts flickering in Toxic Green and Emergency Red, the infection is going as planned.

---

## ☣️ Features

- **Sector breaches**: push through corrupted memory sectors, each with its own hostile drones and an admin boss guarding the exit.
- **Turn-based combat**: weapons, damage scaling and enemy stats all come from data files.
- **The Town Square hub**: the Store, Hardware Tech, Memory Deck and Sector Breach Terminal, with save and load between runs.
- **Data-driven content**: drones, admins, sectors, weapons, items, gems and shops are plain JSON, editable by hand or with the MariesGui editor.
- **Hostile UI**: 24-bit colour via `Pastel` and `Spectre.Console`, with flicker-free redraws locked to the 84×24 grid.

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
dotnet run
```

On launch, press **S** to skip the boot sequence or any other key to watch it. Use the arrow keys and **Enter** to move through menus.

### Install as a .NET tool (optional)

The project is set up as a .NET global tool whose command is `so`:

```bash
dotnet pack -c Release
```

```bash
dotnet tool install --global --add-source ./bin/Release System-Overload
```

```bash
so
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

The content types are `admins`, `drones`, `sectors`, `weapons`, `items`, `gems`, `shops` and `story`. Editing files in the live folder changes your local game. To change the defaults everyone gets, edit `GameData/Templates/`.

### Editing with MariesGui

Content classes are tagged with `[MariesGuiContent]`, and a ready-made profile ships at [`Src/DevTools/mariesgui-manifest.json`](Src/DevTools/mariesgui-manifest.json):

1. Build the game (`dotnet build`) so `bin/Debug/net10.0/System-Overload.dll` exists.
2. In MariesGui, open **Load Profile** and select the manifest.
3. Edit content. Changes are written to `~/Documents/DungeonAdventures/Realm Data/`.

---

## 💀 Project Layout

```
System-Overload/
├── GameData/Templates/     Default JSON content (admins, drones, sectors, weapons, items, gems, shops, story)
├── Src/
│   ├── Program.cs          Entry point: seeding, loaders, boot sequence, main menu
│   ├── Interfaces/         Main menu, character creation, load screen, stats, GameUiEngine
│   ├── TownSquare/         Hub screens (Store, Hardware Tech, Memory Deck, Sector Breach Terminal)
│   ├── Adventures/         Encounters, including custom boss encounters
│   ├── GameEngine/
│   │   ├── Flow/           Simulation executor
│   │   └── Systems/        Combat, content loaders, shops, modding station, quest log
│   ├── GameData/           Data components and entities (player, enemies, sectors, gear)
│   ├── Dev/                Content seeder
│   ├── DevTools/           MariesGui manifest
│   ├── Maries_Ui/          Class library: console UI (HUD, transitions, menus, alerts, art)
│   ├── Maries_DataModels/  Class library: content data classes + MariesGui attribute
│   └── Maries_Loaders/     Class library: JSON content loaders, item registry, story text
├── conductor/              Design docs: product vision, tech stack, style guides, workflow
├── assets/                 Icons and installer art
└── CHANGELOG.md
```

### The stack

| Component | Library | Role |
|:---|:---|:---|
| **Forge Core** | .NET 10 (C#) | Game engine and logic |
| **Visual Pulse** | `Pastel`, `StringBuilder` | 24-bit colour, pre-rendered frames |
| **Interface** | `Spectre.Console` | Panels, tables and structured layout |
| **Neural Link** | `Ollama`, `OpenAI`, `Microsoft.Extensions.AI` | AI-driven dialogue and hostility (in progress) |
| **Logic Guard** | `NUnit` | Test framework |

---

## 🧪 Dev Status: SECTOR_02_CALIBRATION

This is the `Dev` branch, the active lab. Expect unshielded logic, experimental weapon protocols and raw drone assets.

| Component | Status | Focus |
|:---|:---|:---|
| **Forge Core** | 🟢 Stable | Core loop, save/load, content loading |
| **Visual Pulse** | 🟡 Evolving | Drone art and flicker-free buffers |
| **Neural Link** | 🟡 Testing | AI-driven environmental hostility |
| **Loot Logic** | 🔴 Critical | Gem drops and loot distribution |

### Reconstruction checklist

- [ ] **COMBAT_ENGINE**: hit detection and damage scaling
- [ ] **ARMORY_EXPANSION**: new weapons and item IDs
- [ ] **GEM_RESONANCE**: loot drop tables and currency logic
- [ ] **VISUAL_PULSE**: move drones from plain text to ASCII/Spectre entities
- [ ] **STRESS_TEST**: validate the 84×24 Reality Anchor under heavy load

### 👁️ Architect's notes

The current focus is moving combat away from plain text. Drones are being rebuilt as distinct visual entities to raise the dread, and weapon maths is being tuned for consistent lethality.

> *We do not refactor. We infect. We do not scroll. We contain.*

---

## 🤝 Contributing

Read the design docs in [`conductor/`](conductor/) first, especially [`tech-stack.md`](conductor/tech-stack.md) for the rendering rules (84×24 safe zone, `SetCursorPosition` + `Write` instead of `WriteLine`, pre-rendered `StringBuilder` frames). See [`CHANGELOG.md`](CHANGELOG.md) for history and [`SECURITY.md`](SECURITY.md) for reporting vulnerabilities.

---

## 📜 License

MIT © 2026 Kgbcupcake. See [LICENSE](LICENSE).
