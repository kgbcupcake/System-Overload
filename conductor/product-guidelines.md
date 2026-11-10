# Product Guidelines

## Tone: Classic Fantasy Adventure
> The narrative voice is that of a storyteller by the fire: vivid, a little grim, sometimes wry. The dungeon is dangerous and
old, but the game is on the hero's side. Text talks *to* the player as a hero setting out on a quest.

## Style: Short and Flavourful
> Every line should be short enough to fit the 84x24 window and quick to read between fights. Prefer concrete images
(cracked tombstones, green torches, a ring of keys) over abstract words. Humour is welcome in the village; dread belongs in
the dungeon.

## Narrative & Messaging
- **Story lives in data:** All narrative and flavour text belongs in `GameData/Templates/story/` (and in floor, monster and
  boss JSON), never in C#. Code only holds menu labels and key hints.
- **Variety:** Give common events (victories, defeats, shop greetings) several lines so the game can pick one at random.
- **Tokens:** Use `{name}`, `{class}`, `{enemy}`, `{title}` and similar tokens instead of building sentences in code.
- **Bosses speak:** Every boss has an encounter line, an enraged line and a death line.

## Visual Identity
- **Color Palette:**
  - **Torch gold** (`#FFAB00`, `#FFD700`) for titles, selections and the hero.
  - **Old gold / leather** (`#B8860B`, `#8B5A2B`) for borders and secondary text.
  - **Ember** (`#FF4500`, `#FF8C00`) for fire, warriors and loading effects.
  - **Blood red** (`#8B0000`, `#B3001B`) for danger, bosses and defeat.
  - Each floor and boss can set its own colour in its JSON (`HexColor`).
- **UI & Typography:**
  - Unicode box frames with the title set into the top border.
  - Decorative effects (embers, ash) stay light and never cover menus or text the player needs to read.
  - Menus must be easy to navigate: arrow keys and Enter everywhere, Esc to go back.
  - **Containment:** Everything stays inside the 84x24 safe zone so nothing scrolls.

## Brand Messaging
- **Name:** Dungeon Adventures – Reborn
- **Keywords:** Fantasy, Dungeon Crawler, Terminal, Heroes, Loot, Story.
- **Tagline:** "Few who descend ever return."
