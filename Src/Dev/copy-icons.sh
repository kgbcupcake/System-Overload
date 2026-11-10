#!/usr/bin/env bash
# Copies the item icons out of the Clockwork Raven Studios icon pack into assets/icons.
# Usage: Src/Dev/copy-icons.sh /path/to/Raven/Icons
set -euo pipefail
pack="${1:?usage: $0 /path/to/Raven/Icons}/5600+ Ultimate Pixel Art Fantasy RPG Icon Pack/Classic/16X16"
dest="$(cd "$(dirname "$0")/../.." && pwd)/assets/icons"
mkdir -p "$dest"
while read -r id slug; do
	cp "$pack/$id.png" "$dest/$slug.png"
done <<'MAP'
3140 iron_warhammer
3105 rogues_dagger
3083 hunters_longbow
3252 flamebrand
3348 soulreaver
4545 padded_tunic
5169 travelers_hood
5265 wanderers_cloak
5297 soft_leather_boots
4871 iron_cap
4444 chainmail_hauberk
5332 studded_gauntlets
5300 plated_greaves
4501 gravewarden_plate
2056 healing_potion
2196 swiftness_draught
2120 ironskin_tonic
765 rusted_crypt_key
1050 crown_fragment
5381 loved_keepsake
785 travellers_journal
1066 ember_shard
989 frost_sapphire
997 storm_opal
927 bloodstone
1047 hawks_eye
MAP
echo "Copied $(ls "$dest"/*.png | wc -l) icons to $dest"
