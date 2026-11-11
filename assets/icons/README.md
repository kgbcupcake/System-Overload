# Item icons

The inventory and store draw a 16x16 pixel-art icon for an item when a PNG named after it is here:

| Item name        | File                 |
|------------------|----------------------|
| Rogue's Dagger   | `rogues_dagger.png`  |
| Healing Potion   | `healing_potion.png` |

Lower-case, apostrophes dropped, anything else that isn't a letter or digit becomes `_`.
Images larger than 16x16 are ignored. Items without a file show no icon.

The PNGs come from the Clockwork Raven Studios "5600+ Ultimate Pixel Art Fantasy RPG Icon Pack"
(Classic/16X16), which may be used in the game but not redistributed on its own, so they are
git-ignored. To set them up on a new machine run:

```bash
Src/Dev/copy-icons.sh /path/to/Raven/Icons
```

## Clothing (character creator)

The CLOTHES tab of "Make your character" offers the Clockwork Raven "120+ Fantasy Clothing" pack:
128 outfits, 32 cloaks, 32 boots, 48 gloves and 32 belts. Each one is worn through an LPC layer
(`GameData/Templates/wardrobe/wardrobe.json`, made by `Src/Dev/make-wardrobe.py`), so it shows on the
hero, and its 32x32 icon is drawn (shrunk to 16x16) beside the choices. The icons are git-ignored too:

```bash
Src/Dev/copy-clothing.sh /path/to/Raven/Clothing
```
