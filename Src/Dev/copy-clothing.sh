#!/usr/bin/env bash
# Copies the Clockwork Raven "120+ Fantasy Clothing" icons (clothes, capes, boots, gloves, belts) into
# assets/icons/clothing, where the character creator's CLOTHES tab draws them. Licensed art, git-ignored.
# The pieces themselves (which LPC layer and colour each icon is worn as) are in
# GameData/Templates/wardrobe/wardrobe.json, made by Src/Dev/make-wardrobe.py.
# Usage: Src/Dev/copy-clothing.sh /path/to/Raven/Clothing
set -euo pipefail
pack="${1:?usage: $0 /path/to/Raven/Clothing}"
dest="$(cd "$(dirname "$0")/../.." && pwd)/assets/icons/clothing"
for folder in clothes_32x32 capes_32x32 boots_32x32 gloves_32x32 belts_32x32; do
	mkdir -p "$dest/$folder"
	cp "$pack/$folder"/*.png "$dest/$folder/"
done
echo "Copied $(find "$dest" -name '*.png' | wc -l) clothing icons to $dest"
