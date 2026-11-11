using Maries_DataModels;
using static DungeonAdventures.Src.Lpc.LookParts;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>The set builds of the dungeon lords: the Lich, the Dread King, the Crypt Warden, the drake.</summary>
	internal static class BossLooks
	{
		internal static void Lich(HeroLook l, Random r)
		{
			Set(l, "body", "body_skeleton");
			Set(l, "head", "heads_skeleton");
			string robe = Pick(r, "purple", "black", "navy");
			Wear(l, r, "legs", "legs_skirts_plain", robe);
			Wear(l, r, "cape", "cape_tattered", robe);
			Wear(l, r, "hat", "hat_magic_wizard", "");
			Hold(l, r, "weapon_magic_gnarled");
		}

		internal static void DreadKing(HeroLook l, Random r)
		{
			Set(l, "body", "body_skeleton");
			Set(l, "head", "heads_skeleton");
			Wear(l, r, "armour", "torso_armour_plate", "iron", "metal");
			Wear(l, r, "legs", "legs_armour", "iron", "metal");
			Wear(l, r, "shoes", "feet_armour", "iron", "metal");
			Wear(l, r, "cape", "cape_solid", "maroon");
			Variant(l, "hat", "hat_formal_crown", "crown_gold");
			Hold(l, r, "weapon_sword_longsword");
		}

		internal static void Warden(HeroLook l, Random r)
		{
			Set(l, "body", "body_zombie");
			Set(l, "head", "heads_frankenstein");
			Skin(l, "zombie_green");
			Wear(l, r, "armour", "torso_armour_legion", "bronze", "metal");
			Wear(l, r, "legs", "legs_armour", "iron", "metal");
			Wear(l, r, "cape", "cape_tattered", "charcoal");
			Hold(l, r, "weapon_polearm_halberd");
		}

		internal static void Drake(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "amber", "bronze", "brown"));
			Set(l, "head", l.BodyType == "male" ? "heads_lizard_male" : "heads_lizard_female");
			Set(l, "tail", "tail_lizard");
			Set(l, "wings", "wings_lizard");
			Wear(l, r, "legs", "legs_skirts_legion", "red");
		}
	}
}
