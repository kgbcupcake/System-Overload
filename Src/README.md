# ⚡ [SYSTEM_OVERLOAD]: GENESIS_UPLINK

> **“THE SYSTEM IS NOT BROKEN. IT IS SIMPLY EVOLVING BEYOND YOUR CONTROL.”**

[![Version](https://img.shields.io/badge/version-v1.0.0-bolt?style=for-the-badge\&color=yellow)](https://github.com/kgbcupcake/System-Overload)
[![Uplink](https://img.shields.io/badge/UPLINK-STABLE-green?style=for-the-badge\&logo=github)](https://github.com/kgbcupcake/System-Overload)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge\&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-red?style=for-the-badge)](https://opensource.org/licenses/MIT)

---

## ☣️ SYSTEM OVERLOAD

**System Overload** is a .NET 8.0 terminal-based RPG and experimental game engine built around a deliberately constrained terminal interface.

The game transforms a traditional adventure system into a corrupted machine simulation.

You create an operator.

You select your system architecture.

You initialize hardware.

Then you breach the sectors.

The deeper you go, the more unstable the system becomes.

> **The terminal is not merely the interface.**
>
> **It is the machine.**

---

# 🖥️ THE REALITY ANCHOR

System Overload is designed around a controlled terminal environment.

The interface relies heavily on:

* ANSI terminal control
* Cursor positioning
* ANSI/24-bit color
* Unicode box drawing
* Dynamic terminal rendering
* Flicker reduction
* Fixed-width interface elements
* Terminal-aware resizing

The current UI architecture uses a **94-column rendering anchor** for major game interfaces, while individual systems use their own controlled dimensions where appropriate.

The goal is simple:

> **Control the terminal before the terminal controls you.**

---

# ⚔️ CURRENT GAME SYSTEMS

System Overload already contains several functional gameplay systems.

## 🧬 Character Creation

Players can initialize a new operator through the character creation interface.

Current archetypes include:

```text
Aegis-Protocol
Cipher-Blade
System-Architect
```

Character initialization includes:

* Operator designation
* Archetype selection
* Sub-class selection
* Attribute allocation
* Initial statistics
* Starter equipment
* Player manifest generation

---

## ⚔️ Combat Engine

Combat is currently implemented as a turn-based terminal system.

Current combat functionality includes:

* Player attacks
* Enemy attacks
* Health tracking
* Damage calculations
* Weapon damage
* Enemy threat values
* Neural Stim-Packs
* Combat logs
* Post-combat outcomes

Combat encounters are driven through the game's enemy data and drone loaders.

Example enemies include:

```text
security_daemon
dark_wizard
rat_king
```

The combat architecture is separated into dedicated systems including:

```text
Combatant
CombatCalculator
CombatEncounter
CombatMapper
EnemyFactory
```

---

# 🔫 HARDWARE & WEAPONS

Weapons are treated as actual player hardware rather than simple inventory entries.

The current architecture supports:

* Weapon data
* Weapon templates
* Weapon damage
* Hardware modification
* Socketed Data-Gems
* Weapon upgrades

Data-Gems can be injected directly into compatible hardware.

```text
INJECTING INTO: [WEAPON]

Select Data-Gem for injection:

>> [DATA-GEM] [+POWER] <<
    [DATA-GEM] [+POWER] <<

[ESC] CANCEL INJECTION
```

Successful injection modifies the weapon's socketed gem state.

---

# 💎 DATA-GEMS

Data-Gems are part of the current equipment system.

They can be:

* Stored in inventory
* Displayed through the Memory Deck
* Selected for injection
* Socketed into weapons
* Removed from inventory after successful installation

The system is designed so future gem functionality can expand without replacing the existing weapon architecture.

---

# 🧠 MEMORY DECK

The **Memory Deck** functions as the player's inventory/data interface.

Inventory items are:

* Grouped
* Stacked
* Sorted
* Classified by type
* Displayed by rarity
* Given data readouts

The interface can display:

```text
NAME
TYPE
RARITY
EFFECT
QUANTITY
```

Players can execute or purge inventory data directly from the interface.

---

# ☣️ SECTOR BREACH SYSTEM

The **Sector Breach Terminal** is the primary gateway into the game's sector-based progression.

Sectors are loaded dynamically through the sector loader.

Each sector can contain:

* Sector ID
* Sector title
* Difficulty
* Completion state
* Quest state
* Completion credits
* Simulation content

The terminal tracks sector states such as:

```text
[STABLE]
[BREACHING]
[DELETED]
```

Successful breaches award credits and mark the sector as completed.

---

# 🏪 SYSTEM ECONOMY

System Overload contains a shop and currency system.

Current architecture includes:

* Credits
* Shop definitions
* Shop loading
* Global shop management
* Items
* Equipment
* Inventory transactions

The economy is intended to expand alongside the weapon and loot systems.

---

# 💾 SAVE SYSTEM

Player and game state can be persisted through the game's save infrastructure.

The current save architecture provides:

* Persistent game state
* Player profiles
* JSON serialization
* Directory creation
* Write-access validation
* Structured save locations

The project uses a centralized JSON writer to prevent different systems from implementing incompatible serialization behavior.

---

# 🖥️ UI ARCHITECTURE

System Overload uses a dedicated UI layer.

### `Maries_Ui`

The UI library currently contains systems for:

* HUD chrome
* Alerts
* Console setup
* Menu navigation
* Progress bars
* Terminal transitions
* Glitch text
* Game art
* Platform-specific console behavior
* Dynamic UI rendering

The game-specific UI layer remains separate from the reusable UI components.

```text
Maries_Ui/
├── GameArt/
├── Alerts.cs
├── ConsoleSetup.cs
├── GlitchText.cs
├── HudChrome.cs
├── MenuNavigator.cs
├── PlatformConsole.cs
├── ProgressBarRenderer.cs
├── Transitions.cs
└── UiEngine.cs
```

The game-specific interface layer lives separately under:

```text
Interfaces/
└── UI/
    └── GameUiEngine.cs
```

This keeps reusable UI functionality independent from System Overload game state.

---

# 🧱 PROJECT STRUCTURE

```text
System-Overload/
│
├── Adventures/
│   └── Encounters/
│
├── GameData/
│   ├── Components/
│   │   ├── ItemData.cs
│   │   ├── LootTableData.cs
│   │   ├── SectorData.cs
│   │   ├── ShopDefinition.cs
│   │   └── WeaponData.cs
│   │
│   ├── Entities/
│   │   ├── BossData.cs
│   │   ├── EnemyData.cs
│   │   ├── NewEnemyData.cs
│   │   └── PlayerData.cs
│   │
│   └── Templates/
│
├── GameEngine/
│   ├── Flow/
│   ├── Systems/
│   │   ├── Combat/
│   │   └── Loaders/
│   ├── Conductor.cs
│   ├── Loadgame.cs
│   └── SaveGame.cs
│
├── Interfaces/
│   ├── UI/
│   ├── CharacterCreation.cs
│   ├── CharacterLoadScreen.cs
│   ├── CharSubMenus.cs
│   ├── DevGui_Main.cs
│   ├── MainMenu.cs
│   └── Stats.cs
│
├── Maries_Ui/
│
├── TownSquare/
│   └── InterFaces/
│       ├── HardwareTech.cs
│       ├── MemoryDeck.cs
│       ├── SectorBreachTerminal.cs
│       └── Store.cs
│
├── Utilities/
│   └── JsonContentWriter.cs
│
└── Program.cs
```

---

# ⚙️ TECHNOLOGY STACK

| Component          | Technology       | Purpose                    |
| :----------------- | :--------------- | :------------------------- |
| **Runtime**        | .NET 8.0         | Core runtime               |
| **Language**       | C#               | Game and engine logic      |
| **Terminal Color** | Pastel           | ANSI color output          |
| **Terminal UI**    | Maries_Ui        | Custom interface framework |
| **Testing**        | NUnit            | Logic validation           |
| **Serialization**  | System.Text.Json | Persistent game data       |
| **Rendering**      | Console / ANSI   | Terminal presentation      |

---

# 🧪 DEVELOPMENT PHILOSOPHY

System Overload is deliberately experimental.

The project prioritizes:

* Modular systems
* Data-driven content
* Reusable UI
* Controlled terminal rendering
* Testable game logic
* Expandable combat
* Persistent player state
* Experimental gameplay systems

The project may change rapidly as systems are rebuilt.

---

# 🧠 NEURAL LINK

### STATUS: `NOT IMPLEMENTED`

AI integration is **not currently part of the playable System Overload engine**.

Future experimentation may investigate:

* Ollama
* OpenAI
* Local language models
* Dynamic system dialogue
* Adaptive encounters
* Procedural narrative
* Environmental reactions
* System memory

These concepts belong to the experimental development environment and should **not** be considered current game functionality.

> **The machine does not have a mind yet.**
>
> **We're still building one.**

---

# 🛠️ BUILDING

Requirements:

* .NET 8.0 SDK
* Git
* ANSI-capable terminal
* UTF-8 terminal output

Clone:

```bash
git clone https://github.com/kgbcupcake/System-Overload.git
cd System-Overload
```

Restore:

```bash
dotnet restore
```

Build:

```bash
dotnet build
```

Run:

```bash
dotnet run
```

Test:

```bash
dotnet test
```

---

# ⚠️ DEVELOPMENT WARNING

System Overload is under active reconstruction.

The current repository may contain:

* Experimental systems
* Legacy naming
* Incomplete migrations
* Development tooling
* Unfinished content
* Temporary implementations

The `dev-bench` environment is where experimental systems should be developed before being promoted into the main game.

---

# 📜 HOSTILE CODE MANIFESTO

### We do not refactor.

**We infect.**

### We do not scroll.

**We contain.**

### We do not log.

**We observe.**

### We do not hide instability.

**We weaponize it.**

---

# 👁️ THE ARCHITECT

**Created by Kgbcupcake**

Built for the terminal.

Built to evolve.

Built to survive its own corruption.

---

# 📜 LICENSE

Copyright (c) 2026 Kgbcupcake.

System Overload is released under the MIT License.

See [`LICENSE`](LICENSE) for the complete license text.

---

```text
╔══════════════════════════════════════════════════════════════════════════════╗
║                         SYSTEM_OVERLOAD                                      ║
║                                                                              ║
║  CORE ................. ONLINE                                              ║
║  COMBAT ............... ONLINE                                              ║
║  HARDWARE ............. ONLINE                                              ║
║  SECTORS .............. ONLINE                                              ║
║  MEMORY DECK .......... ONLINE                                              ║
║  ECONOMY .............. ONLINE                                              ║
║  SAVE SYSTEM .......... ONLINE                                              ║
║  NEURAL LINK .......... NOT IMPLEMENTED                                     ║
║                                                                              ║
║  > SYSTEM STATUS: OPERATIONAL                                              ║
║  > HUMAN PRESENCE: DETECTED                                                 ║
╚══════════════════════════════════════════════════════════════════════════════╝
```
