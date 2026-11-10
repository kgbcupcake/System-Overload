# Hero art: Universal LPC character catalog

The "Make your character" screen draws the hero from the
[Universal LPC Spritesheet Character Generator](https://github.com/LiberatedPixelCup/Universal-LPC-Spritesheet-Character-Generator)
(its `sheet_definitions/`, `palette_definitions/` and `spritesheets/`). It is about 500 MB, so it is not in this repo:
`assets/LPC` is git-ignored. Put it there in one of these ways:

```bash
git clone --depth 1 https://github.com/LiberatedPixelCup/Universal-LPC-Spritesheet-Character-Generator.git assets/LPC
```

or link an existing copy (this is how the dev machine is set up):

```bash
ln -s /mnt/storage/RiderProjects/MariesRPG/assets/LPC assets/LPC
```

The game also looks for `assets/LPC` beside the built game, or wherever the `DUNGEON_LPC` environment variable points.
Without it, character creation skips the screen and heroes have no look.

**Licences:** the art is CC-BY-SA 3.0, GPL 3.0 and OGA-BY 3.0, per item (see each sheet definition's `credits` and
`CREDITS.csv`). The game credits it on the Credits screen; images made from CC-BY-SA art stay CC-BY-SA.
