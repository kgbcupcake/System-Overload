# Initial Concept
> Dungeon Adventures – Reborn is a classic fantasy dungeon crawler played entirely in the terminal.
A hero leaves the village of Hollowmere and descends, floor by floor, into the dungeon beneath it: a flooded crypt,
volcanic caverns, a lich's catacombs and finally the Dread King's throne room. Each floor is told in short story beats,
fought through turn by turn, and guarded by a dungeon lord. The terminal is the whole world: a fixed 84x24 window lit in
torch gold, ember orange and old blood red.

# Product Vision

## Target Audience & Goals
- **Target Audience:** Players who enjoy old-school RPGs and roguelikes, text adventures and terminal games, and who like
   reading a bit of story between fights.
- **User Goals:**
    - To build a hero (class, path and attribute points) and feel them grow stronger.
    - To clear every floor of the dungeon and defeat the Dread King.
    - To collect better weapons and gems, and make choices at the store and the Blacksmith that change how fights go.
    - To enjoy short, flavourful story text that changes a little each time.

## Core Features & Functionality
- **Heroes:**
    - Three classes (Warrior, Rogue, Mage), each with three paths.
    - Ten attribute points across Vitality, Strength and Fortitude (Wisdom is planned).
    - Starting gear that fits the class.
- **Dungeon Floors:**
    - Hand-written floors made of story stages, with monsters along the way and a boss at the end.
    - Bosses with named moves, a special ability, an enraged phase and their own dialogue.
    - Loot drops, gold and experience, with level-ups.
- **The Village of Hollowmere:**
    - General Store (potions, the Dark Pact difficulty option, selling loot).
    - Dungeon Gate (pick a floor), Blacksmith (socket gems), Inventory and Character sheet.
    - A Healer is planned.
- **Data-Driven World:**
    - Monsters, bosses, floors, items, weapons, gems, shops and all story text are JSON files.
    - Content can be edited by hand or with the MariesGui editor, without touching code.
- **Atmosphere:**
    - Warm torchlight colours (gold, amber, ember) against dark reds and purples for danger.
    - Falling embers, flickering banners and simple ASCII art instead of a GUI.
