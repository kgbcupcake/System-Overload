using Maries_DataModels;
using static DungeonAdventures.Src.Lpc.LookParts;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>
	/// Rolls a look for a foe from the LPC catalog, so every fight shows a sprite. The monster's id, name
	/// and art ref pick a kind (rat, skeleton, wolf, goblin...) by keyword; the kind fixes the head, body
	/// and skin, and the rest (clothes, armour, weapon, colours) is rolled fresh each fight, so two Cave
	/// Rats don't look alike. A foe no keyword matches gets a kind picked from its name, the same every time.
	/// </summary>
	public static class EnemyLooks
	{
		private delegate void Builder(HeroLook look, Random rng);

		/// <summary>Keyword -> kind. The first keyword found in "id name artRef" wins, so put specific ones first.</summary>
		private static readonly (string Keyword, Builder Build)[] Kinds =
		{
			("lich", BossLooks.Lich), ("dread", BossLooks.DreadKing), ("king", BossLooks.DreadKing), ("warden", BossLooks.Warden),
			("alpha", Wolf), ("drake", BossLooks.Drake), ("dragon", BossLooks.Drake), ("wyrm", BossLooks.Drake),
			("skeleton", Skeleton), ("bone", Skeleton), ("zombie", Zombie), ("ghoul", Zombie), ("vampire", Vampire),
			("rat", Rat), ("mouse", Mouse), ("wolf", Wolf), ("hound", Wolf), ("dog", Wolf),
			("goblin", Goblin), ("orc", Orc), ("troll", Troll), ("ogre", Troll),
			("sentinel", Sentinel), ("golem", Sentinel), ("stone", Sentinel), ("knight", Sentinel), ("guard", Sentinel),
			("minotaur", Minotaur), ("bull", Minotaur), ("boar", Boar), ("pig", Boar),
			("lizard", Lizard), ("serpent", Lizard), ("snake", Lizard), ("bat", Bat),
			("bandit", Bandit), ("thief", Bandit), ("cultist", Cultist), ("witch", Cultist), ("mage", Cultist),
		};

		/// <summary>Kinds an unmatched foe can be.</summary>
		private static readonly Builder[] Anything = { Rat, Skeleton, Zombie, Goblin, Orc, Wolf, Troll, Bandit, Lizard, Boar };

		/// <summary>A new look for a foe, or null when the LPC catalog isn't installed.</summary>
		public static HeroLook? Roll(string id, string name, string artRef = "", Random? rng = null)
		{
			if (!LpcMeta.Available) return null;
			rng ??= Random.Shared;
			string words = $"{id} {name} {artRef}".ToLowerInvariant();

			Builder build = Kinds.FirstOrDefault(k => words.Contains(k.Keyword)).Build
				?? Anything[(int)((uint)StableHash(id + name) % Anything.Length)];

			var look = new HeroLook { BodyType = rng.Next(2) == 0 ? "male" : "female" };
			build(look, rng);
			return look;
		}

		// ------------------------------------------------------------------ kinds

		private static void Rat(HeroLook l, Random r) => Critter(l, r, "heads_rat");
		private static void Mouse(HeroLook l, Random r) => Critter(l, r, "heads_mouse");

		/// <summary>A furry little thing in rags with a bare tail.</summary>
		private static void Critter(HeroLook l, Random r, string head)
		{
			Skin(l, Pick(r, "fur_grey", "fur_brown", "fur_tan", "fur_black"));
			Set(l, "head", head);
			Set(l, "tail", "tail_lizard");
			if (r.Next(3) > 0) Wear(l, r, "clothes", Pick(r, "torso_clothes_sleeveless1", "torso_clothes_shortsleeve", "torso_clothes_tshirt"), Drab(r));
			Wear(l, r, "legs", Pick(r, "legs_shorts", "legs_shorts_short", "legs_pants"), Drab(r));
			if (r.Next(2) == 0) Hold(l, r, "weapon_sword_dagger");
		}

		private static void Wolf(HeroLook l, Random r)
		{
			string fur = Pick(r, "fur_grey", "fur_black", "fur_brown", "fur_white", "fur_tan");
			Skin(l, fur);
			Set(l, "head", l.BodyType == "male" ? "heads_wolf_male" : "heads_wolf_female");
			Variant(l, "tail", Pick(r, "tail_wolf", "tail_wolf_fluffy"), fur);
			if (r.Next(2) == 0) Wear(l, r, "armour", "torso_armour_leather", "");
			else Wear(l, r, "clothes", "torso_clothes_sleeveless2", Drab(r));
			Wear(l, r, "legs", "legs_pants", Drab(r));
		}

		private static void Skeleton(HeroLook l, Random r)
		{
			Set(l, "body", "body_skeleton");
			Set(l, "head", "heads_skeleton");
			if (r.Next(2) == 0) Wear(l, r, "hat", Pick(r, "hat_helmet_nasal", "hat_helmet_kettle", "hat_helmet_spangenhelm", "hat_helmet_bascinet"), Rust(r), "metal");
			if (r.Next(3) == 0) Wear(l, r, "armour", "torso_armour_legion", Rust(r), "metal");
			if (r.Next(2) == 0) Wear(l, r, "legs", "legs_skirts_legion", Drab(r));
			Hold(l, r, Pick(r, "weapon_sword_longsword", "weapon_sword_scimitar", "weapon_polearm_spear", "weapon_blunt_mace"));
			if (r.Next(3) == 0) Hold(l, r, "shield_heater_wood", "shield");
		}

		private static void Zombie(HeroLook l, Random r)
		{
			Set(l, "body", "body_zombie");
			Set(l, "head", "heads_zombie");
			Skin(l, Pick(r, "zombie", "zombie_green"));
			Wear(l, r, "clothes", Pick(r, "torso_clothes_longsleeve", "torso_clothes_shortsleeve", "torso_clothes_sleeveless1"), Drab(r));
			Wear(l, r, "legs", Pick(r, "legs_pants", "legs_shorts", "legs_pantaloons"), Drab(r));
		}

		private static void Vampire(HeroLook l, Random r)
		{
			Skin(l, "light");
			Set(l, "head", "heads_vampire");
			Wear(l, r, "clothes", "torso_clothes_longsleeve2_buttoned", Pick(r, "black", "maroon", "charcoal"));
			Wear(l, r, "legs", "legs_formal", "black");
			Wear(l, r, "shoes", "feet_boots_basic", "black");
			Wear(l, r, "cape", "cape_solid", Pick(r, "red", "maroon", "black"));
		}

		private static void Goblin(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "green", "dark_green", "bright_green", "pale_green", "olive"));
			Set(l, "head", "heads_goblin");
			if (r.Next(2) == 0) Wear(l, r, "armour", "torso_armour_leather", "");
			else Wear(l, r, "clothes", Pick(r, "torso_clothes_sleeveless1", "torso_clothes_tshirt"), Drab(r));
			Wear(l, r, "legs", Pick(r, "legs_shorts", "legs_pants", "legs_shorts_short"), Drab(r));
			if (r.Next(2) == 0) Wear(l, r, "hat", "hat_hood_cloth", Drab(r));
			Hold(l, r, Pick(r, "weapon_sword_dagger", "weapon_blunt_mace", "weapon_polearm_spear", "tool_axe"));
		}

		private static void Orc(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "green", "dark_green", "olive", "taupe"));
			Set(l, "head", l.BodyType == "male" ? "heads_orc_male" : "heads_orc_female");
			Wear(l, r, "armour", Pick(r, "torso_armour_leather", "torso_armour_legion"), Rust(r), "metal");
			Wear(l, r, "legs", Pick(r, "legs_pants", "legs_armour"), Drab(r));
			Wear(l, r, "shoes", "feet_boots_basic", Drab(r));
			if (r.Next(2) == 0) Wear(l, r, "hat", "hat_helmet_horned", Rust(r), "metal");
			Hold(l, r, Pick(r, "weapon_blunt_waraxe", "tool_axe", "weapon_blunt_flail", "weapon_sword_scimitar"));
		}

		private static void Troll(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "olive", "taupe", "pale_green", "dark_green", "blue"));
			Set(l, "head", "heads_troll");
			if (r.Next(2) == 0) Wear(l, r, "clothes", "torso_clothes_sleeveless1", Drab(r));
			Wear(l, r, "legs", Pick(r, "legs_shorts", "legs_pantaloons"), Drab(r));
			Hold(l, r, Pick(r, "tool_hammer", "weapon_blunt_mace", "tool_axe"));
		}

		/// <summary>A suit of armour that walks: helm shut, nothing of the wearer showing.</summary>
		private static void Sentinel(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "taupe", "olive", "black"));
			Set(l, "head", "heads_human_male");
			string metal = Pick(r, "iron", "steel", "ceramic", "bronze");
			Wear(l, r, "armour", "torso_armour_plate", metal, "metal");
			Wear(l, r, "legs", "legs_armour", metal, "metal");
			Wear(l, r, "shoes", "feet_armour", metal, "metal");
			Wear(l, r, "hat", Pick(r, "hat_helmet_greathelm", "hat_helmet_armet", "hat_helmet_close", "hat_helmet_barbuta"), metal, "metal");
			Hold(l, r, Pick(r, "weapon_polearm_halberd", "weapon_sword_longsword", "weapon_blunt_flail"));
			if (r.Next(2) == 0) Hold(l, r, Pick(r, "shield_crusader", "shield_kite", "shield_spartan"), "shield");
		}

		private static void Minotaur(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "fur_brown", "fur_black", "fur_copper", "brown"));
			Set(l, "head", l.BodyType == "male" ? "heads_minotaur" : "heads_minotaur_female");
			Set(l, "shoes", "feet_hoofs");
			Wear(l, r, "legs", "legs_skirts_legion", Drab(r));
			Hold(l, r, Pick(r, "weapon_blunt_waraxe", "tool_axe", "weapon_polearm_halberd"));
		}

		private static void Boar(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "fur_brown", "fur_tan", "fur_black", "taupe"));
			Set(l, "head", Pick(r, "heads_boarman", "heads_wartotaur"));
			Wear(l, r, "armour", "torso_armour_leather", "");
			Wear(l, r, "legs", "legs_pants", Drab(r));
			Hold(l, r, Pick(r, "weapon_polearm_spear", "tool_axe", "weapon_blunt_mace"));
		}

		private static void Lizard(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "green", "dark_green", "olive", "bronze", "amber"));
			Set(l, "head", l.BodyType == "male" ? "heads_lizard_male" : "heads_lizard_female");
			Set(l, "tail", "tail_lizard");
			Wear(l, r, "legs", Pick(r, "legs_skirts_legion", "legs_shorts"), Drab(r));
			if (r.Next(2) == 0) Wear(l, r, "armour", "torso_armour_leather", "");
			Hold(l, r, Pick(r, "weapon_polearm_spear", "weapon_sword_scimitar"));
		}

		private static void Bat(HeroLook l, Random r)
		{
			string fur = Pick(r, "fur_black", "fur_brown", "fur_grey");
			Skin(l, fur);
			Set(l, "head", "heads_mouse");
			Variant(l, "wings", "wings_bat", fur);
			Wear(l, r, "legs", "legs_shorts_short", "black");
		}

		/// <summary>A human cutthroat: hooded, cheap clothes, a short blade.</summary>
		private static void Bandit(HeroLook l, Random r)
		{
			Human(l, r);
			Wear(l, r, "clothes", Pick(r, "torso_clothes_longsleeve", "torso_clothes_longsleeve2_vneck", "torso_clothes_shortsleeve"), Drab(r));
			Wear(l, r, "legs", Pick(r, "legs_pants", "legs_pants2", "legs_cuffed"), Drab(r));
			Wear(l, r, "shoes", "feet_boots_basic", Drab(r));
			Wear(l, r, "hat", "hat_hood_cloth", Pick(r, "black", "charcoal", "maroon", "forest"));
			Hold(l, r, Pick(r, "weapon_sword_dagger", "weapon_sword_saber", "weapon_sword_scimitar"));
		}

		private static void Cultist(HeroLook l, Random r)
		{
			Human(l, r);
			string robe = Pick(r, "purple", "maroon", "black", "navy");
			Wear(l, r, "clothes", "torso_clothes_longsleeve", robe);
			Wear(l, r, "legs", "legs_skirts_plain", robe);
			Wear(l, r, "hat", "hat_hood_cloth", robe);
			Hold(l, r, Pick(r, "weapon_magic_gnarled", "weapon_magic_simple"));
		}
	}
}
