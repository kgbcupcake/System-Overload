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
