#!/usr/bin/env python3
"""
Builds GameData/Templates/wardrobe/wardrobe.json: every Clockwork Raven clothing icon (clothes, capes,
boots, gloves, belts) paired with the Universal LPC layer that draws it on the hero, the way MariesRPG's
gear_items.json pairs a Raven `tile` with an `lpc` item. The JSON holds ids, names and colour names only,
no pixels, so it can be committed; the icons themselves stay git-ignored (Src/Dev/copy-clothing.sh).

Each piece's colour is read from its icon: the average of its lit pixels, matched to the nearest LPC
cloth (or metal) ramp, or to the nearest colour file of items sold one file per colour.

Usage (from the repo root, with assets/LPC installed):
    python3 -I Src/Dev/make-wardrobe.py /path/to/Raven/Clothing
"""
import colorsys
import json
import os
import sys

from PIL import Image

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
LPC = os.path.join(REPO, "assets", "LPC")
OUT = os.path.join(REPO, "GameData", "Templates", "wardrobe", "wardrobe.json")

# The 16 shapes of the clothing sheet (one per column; the 8 rows are colourways):
# name, LPC item for a female hero, LPC item for a male hero.
SHAPES = [
    ("Shirt", "torso_clothes_longsleeve2_buttoned", "torso_clothes_longsleeve2_buttoned"),
    ("Frock", "dress_sash", "torso_clothes_longsleeve2_cardigan"),
    ("Long Coat", "torso_clothes_longsleeve2_cardigan", "torso_clothes_longsleeve2_cardigan"),
    ("Robe", "torso_clothes_robe", "torso_clothes_longsleeve2"),
    ("Tunic", "torso_clothes_tunic", "torso_clothes_tshirt_buttoned"),
    ("Pinafore", "torso_clothes_longsleeve2_scoop", "torso_clothes_longsleeve2_scoop"),
    ("Jacket", "torso_clothes_longsleeve2_polo", "torso_clothes_longsleeve2_polo"),
    ("Coat", "torso_clothes_longsleeve2_vneck", "torso_clothes_longsleeve2_vneck"),
    ("Gown", "dress_slit", "torso_clothes_longsleeve"),
    ("Harness", "torso_clothes_shortsleeve_cardigan", "torso_clothes_shortsleeve_cardigan"),
    ("Kimono", "dress_kimono", "torso_clothes_longsleeve_scoop"),
    ("Tabard", "torso_clothes_shortsleeve_polo", "torso_clothes_shortsleeve_polo"),
    ("Mage Robe", "torso_clothes_longsleeve_scoop", "torso_clothes_longsleeve_scoop"),
    ("Vest", "torso_clothes_sleeveless2_vneck", "torso_clothes_sleeveless2_vneck"),
    ("Sash Coat", "torso_clothes_longsleeve", "torso_clothes_longsleeve"),
    ("Blouse", "torso_clothes_blouse_longsleeve", "torso_clothes_tshirt_vneck"),
]
BOOT_STYLES = ["feet_boots_revised", "feet_boots_fold", "feet_boots_rim", "feet_boots_basic"]

WORDS = {"bluegray": "Blue-grey", "gray": "Grey", "forest": "Forest", "charcoal": "Charcoal"}


def load_json(*parts):
    with open(os.path.join(*parts)) as f:
        return json.load(f)


def hex_rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


CLOTH = {k: [hex_rgb(c) for c in v] for k, v in load_json(LPC, "palette_definitions", "cloth", "cloth_ulpc.json").items()}
METAL = {k: [hex_rgb(c) for c in v] for k, v in load_json(LPC, "palette_definitions", "metal", "metal_ulpc.json").items()}


def item_def(item_id):
    for root, _, files in os.walk(os.path.join(LPC, "sheet_definitions")):
        if item_id + ".json" in files:
            return load_json(root, item_id + ".json")
    raise SystemExit(f"LPC item {item_id} not found")


def icon_colour(path):
    """The icon's pixels (outline and deep shadow left out): what a ramp is matched against."""
    img = Image.open(path).convert("RGBA")
    px = [img.getpixel((x, y)) for y in range(img.height) for x in range(img.width)]
    px = [p[:3] for p in px if p[3] >= 128]
    return [p for p in px if sum(p) / 3 > 40] or px


def average(pixels):
    return tuple(sum(p[i] for p in pixels) / len(pixels) for i in range(3))


def distance(a, b):
    # Weighted RGB plus a hue term, so a dim purple doesn't land on grey.
    dr, dg, db = (a[i] - b[i] for i in range(3))
    base = (2 * dr * dr + 4 * dg * dg + 3 * db * db) ** 0.5
    ha, la, sa = colorsys.rgb_to_hls(*(c / 255 for c in a))
    hb, lb, sb = colorsys.rgb_to_hls(*(c / 255 for c in b))
    dh = min(abs(ha - hb), 1 - abs(ha - hb))
    return base + 300 * dh * min(sa, sb)


def nearest(pixels, ramps, allowed=None):
    """The ramp whose shades sit closest to the icon's pixels (each pixel to its nearest shade, averaged).
    The pixels' most common colours count most, so a gold trim doesn't outvote the cloth."""
    counts = {}
    for p in pixels:
        counts[p] = counts.get(p, 0) + 1
    names = [n for n in ramps if allowed is None or n in allowed]

    def cost(name):
        shades = ramps[name][1:]
        return sum(c * min(distance(p, s) for s in shades) for p, c in counts.items())
    return min(names, key=cost)


def colour_for(item_id, colour, prefer=None):
    """{material: ramp} for an item: its recolour material, or the nearest file of a one-file-per-colour item.
    `prefer` (the ramp the other body's cut got) wins when this item can take it, so both cuts match."""
    d = item_def(item_id)
    rec = d.get("recolors")
    if prefer and isinstance(rec, dict) and prefer in CLOTH:
        return {rec.get("material", "cloth"): prefer}
    if prefer and not rec and prefer in d.get("variants", []):
        return {"cloth": prefer}
    rec = d.get("recolors")
    if isinstance(rec, dict) and rec.get("material") == "metal":
        both = {**METAL, **CLOTH}  # metal items here also take the cloth ramps
        return {"metal": nearest(colour, both)}
    if rec:
        return {"cloth": nearest(colour, CLOTH)}
    variants = [v for v in d.get("variants", []) if v in CLOTH]
    return {"cloth": nearest(colour, CLOTH, variants)} if variants else {}


def roman(n):
    out = ""
    for value, letters in ((10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")):
        while n >= value:
            out, n = out + letters, n - value
    return out


def word(ramp):
    return WORDS.get(ramp, ramp.capitalize())


def main():
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    pack = sys.argv[1]
    pieces, names = [], {}

    def add(slot, folder, tile, label, lpc_f, lpc_m):
        path = os.path.join(pack, folder, f"tile{tile:03d}.png")
        colour = icon_colour(path)
        colours = colour_for(lpc_f, colour)
        ramp = next(iter(colours.values()), "")
        name = f"{word(ramp)} {label}".strip()
        names[name] = names.get(name, 0) + 1
        if names[name] > 1:
            name += " " + roman(names[name])
        piece = {
            "id": f"{slot}_{tile:03d}",
            "name": name,
            "slot": slot,
            "icon": f"{folder}/tile{tile:03d}.png",
            "lpc": {"female": lpc_f, "male": lpc_m},
            "colours": colours,
        }
        if lpc_m != lpc_f:  # the male item may sell its colours another way
            male = colour_for(lpc_m, colour, ramp)
            if male != colours:
                piece["colours_male"] = male
        pieces.append(piece)

    for tile in range(128):
        label, f, m = SHAPES[tile % 16]
        add("outfit", "clothes_32x32", tile, label, f, m)
    for tile in range(208, 240):
        add("cape", "capes_32x32", tile, "Cloak", "cape_solid", "cape_solid")
    for i, tile in enumerate(range(240, 272)):
        path = os.path.join(pack, "boots_32x32", f"tile{tile:03d}.png")
        h, l, s = colorsys.rgb_to_hls(*(c / 255 for c in average(icon_colour(path))))
        if s < 0.18 and l > 0.45:  # pale grey: plate boots
            add("boots", "boots_32x32", tile, "Sabatons", "feet_armour", "feet_armour")
        else:
            style = BOOT_STYLES[i % len(BOOT_STYLES)]
            add("boots", "boots_32x32", tile, "Boots", style, style)
    for tile in range(272, 320):
        add("gloves", "gloves_32x32", tile, "Gloves", "arms_gloves", "arms_gloves")
    leather = set(item_def("belt_leather")["variants"])
    for i, tile in enumerate(range(416, 448)):
        path = os.path.join(pack, "belts_32x32", f"tile{tile:03d}.png")
        col = icon_colour(path)
        # Leathery colours keep a real belt; bright ones (the pack has pink, teal, gold...) a narrow sash.
        if nearest(col, CLOTH) in leather:
            item = "belt_leather" if i % 2 == 0 else "belt_double"
            add("belt", "belts_32x32", tile, "Belt", item, item)
        else:
            add("belt", "belts_32x32", tile, "Sash", "belt_sash_narrow", "belt_sash_narrow")

    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w") as f:
        json.dump({
            "_help": "Made by Src/Dev/make-wardrobe.py. Raven clothing icons (assets/icons/clothing/<icon>) paired with "
                     "the LPC item each body type wears and its colour ({material: ramp}).",
            "pieces": pieces,
        }, f, indent=1)
    print(f"Wrote {len(pieces)} pieces to {OUT}")


if __name__ == "__main__":
    main()
