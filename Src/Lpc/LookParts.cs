using Maries_DataModels;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>Small steps for building a foe's look: a human face, skin, putting on items in colours, picking at random.</summary>
	internal static class LookParts
	{
		internal static void Human(HeroLook l, Random r)
		{
			Skin(l, Pick(r, "light", "amber", "olive", "taupe", "bronze", "brown"));
			Set(l, "head", l.BodyType == "male" ? Pick(r, "heads_human_male", "heads_human_male_gaunt") : "heads_human_female");
			Set(l, "expression", Pick(r, "face_neutral", "face_angry", "face_angry2"));
			Set(l, "hair", Pick(r, "hair_plain", "hair_buzzcut", "hair_messy1", "hair_long"));
			l.Colours["hair"] = Pick(r, "black", "dark_brown", "chestnut", "ginger", "gray");
			if (l.BodyType == "male" && r.Next(2) == 0) Set(l, "beard", "beards_beard");
		}

		/// <summary>Skin (and every head drawn in the body material) in a body ramp.</summary>
		internal static void Skin(HeroLook l, string ramp) => l.Colours["body"] = ramp;

		/// <summary>Puts an item on a part when the catalog has it (unknown ids are skipped, not drawn wrong).</summary>
		internal static void Set(HeroLook l, string type, string id)
		{
			if (LpcCatalog.Items.ContainsKey(id)) l.Parts[type] = id;
		}

		/// <summary>An item in a colour of `material` (cloth unless said). "" leaves its own colours.</summary>
		internal static void Wear(HeroLook l, Random r, string type, string id, string colour, string material = "cloth")
		{
			if (!LpcCatalog.Items.TryGetValue(id, out var item)) return;
			l.Parts[type] = id;
			if (colour == "") return;
			if (item.Recolors.Count == 0 && item.Variants.Count > 0)
			{
				// Sold as one file per colour: take the named one when it exists, else any.
				string v = item.Variants.Contains(colour) ? colour : item.Variants[r.Next(item.Variants.Count)];
				l.PartColours[type] = new() { ["variant"] = v };
			}
			else if (item.Recolors.Count > 0)
				l.PartColours[type] = new() { [item.Recolors.Contains(material) ? material : item.Recolors[0]] = colour };
		}

		/// <summary>An item drawn from its file named `variant` (tails and wings come one file per fur).</summary>
		internal static void Variant(HeroLook l, string type, string id, string variant)
		{
			if (!LpcCatalog.Items.TryGetValue(id, out var item)) return;
			l.Parts[type] = id;
			if (item.Variants.Contains(variant)) l.PartColours[type] = new() { ["variant"] = variant };
		}

		/// <summary>A weapon or shield in one of its own colours.</summary>
		internal static void Hold(HeroLook l, Random r, string id, string type = "weapon") => Wear(l, r, type, id, "steel");

		internal static string Drab(Random r) => Pick(r, "brown", "leather", "walnut", "tan", "gray", "charcoal", "slate", "forest", "maroon", "bluegray");
		internal static string Rust(Random r) => Pick(r, "iron", "bronze", "copper", "steel");

		internal static string Pick(Random r, params string[] options) => options[r.Next(options.Length)];

		/// <summary>string.GetHashCode changes per run; this doesn't, so an unknown foe keeps its kind.</summary>
		internal static int StableHash(string s)
		{
			unchecked
			{
				int h = 23;
				foreach (char c in s) h = h * 31 + c;
				return h;
			}
		}
	}
}
