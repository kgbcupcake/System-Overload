using Maries_DataModels;
using static DungeonAdventures.Src.Lpc.LookParts;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>
	/// Looks for the village's people, dressed by their role: the elder in a long robe with grey hair, the
	/// tavern keeper in a scooped top and skirt, the guard in leather and a kettle helmet with a halberd,
	/// and everyone else in plain working clothes. The same seed always gives the same person.
	/// </summary>
	public static class VillagerLooks
	{
		public static HeroLook? Roll(string role, int seed)
		{
			if (!LpcMeta.Available) return null;
			var r = new Random(seed);
			string words = role.ToLowerInvariant();
			bool woman = words.Contains("keeper") || words.Contains("healer") || words.Contains("maid")
				|| (!words.Contains("elder") && !words.Contains("guard") && r.Next(2) == 0);
			var l = new HeroLook { BodyType = woman ? "female" : "male" };
			Human(l, r);

			if (words.Contains("elder"))
			{
				l.Colours["hair"] = Pick(r, "white", "gray", "ash");
				string robe = Pick(r, "brown", "forest", "navy", "maroon");
				Wear(l, r, "clothes", "torso_clothes_longsleeve", robe);
				Wear(l, r, "legs", "legs_skirts_plain", robe);
				Wear(l, r, "shoes", "feet_shoes_basic", "brown");
				if (l.BodyType == "male") Set(l, "beard", "beards_beard");
			}
			else if (words.Contains("guard"))
			{
				Wear(l, r, "armour", "torso_armour_leather", Pick(r, "brown", "black"));
				Wear(l, r, "legs", "legs_pants", Drab(r));
				Wear(l, r, "shoes", "feet_boots_basic", "brown");
				Wear(l, r, "hat", "hat_helmet_kettle", "iron", "metal");
				Hold(l, r, "weapon_polearm_halberd");
			}
			else if (words.Contains("keeper"))
			{
				Wear(l, r, "clothes", "torso_clothes_longsleeve2_scoop", Pick(r, "white", "tan", "sky"));
				Wear(l, r, "legs", "legs_skirts_plain", Pick(r, "maroon", "brown", "forest"));
				Wear(l, r, "shoes", "feet_shoes_basic", "black");
			}
			else
			{
				Wear(l, r, "clothes", Pick(r, "torso_clothes_longsleeve", "torso_clothes_shortsleeve", "torso_clothes_tshirt",
					"torso_clothes_longsleeve2_vneck", "torso_clothes_longsleeve2_buttoned"), Drab(r));
				if (l.BodyType == "female" && r.Next(2) == 0) Wear(l, r, "legs", "legs_skirts_plain", Drab(r));
				else Wear(l, r, "legs", Pick(r, "legs_pants", "legs_pants2", "legs_cuffed"), Drab(r));
				Wear(l, r, "shoes", Pick(r, "feet_shoes_basic", "feet_boots_basic"), Pick(r, "brown", "black"));
			}
			return l;
		}
	}
}
