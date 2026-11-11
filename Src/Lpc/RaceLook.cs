using Maries_DataModels;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>
	/// Keeps a look true to the hero's race: the heads it may wear, its ears and its skin colours (see
	/// <see cref="RaceData"/>). The "Make your character" screen sets <see cref="Active"/> while it runs;
	/// with no race (an old hero) every choice stays open.
	/// </summary>
	public static class RaceLook
	{
		/// <summary>The race the creator is shaping, or null for no limits.</summary>
		public static RaceData? Active { get; set; }

		/// <summary>The heads the active race allows for the look's body, or null when any head goes.</summary>
		public static List<string>? Heads(HeroLook look) =>
			Active != null && Active.Heads.TryGetValue(look.BodyType, out var heads) && heads.Count > 0 ? heads : null;

		/// <summary>The skin ramps the active race allows, or null when any skin goes.</summary>
		public static List<string>? Skins => Active is { Skins.Count: > 0 } race ? race.Skins : null;

		/// <summary>Puts the race's head, ears and skin on the look where it doesn't have them already.</summary>
		public static void Fit(HeroLook look)
		{
			if (Active == null) return;
			if (Heads(look) is { } heads && !heads.Contains(LpcLook.Part(look, "head")))
				look.Parts["head"] = heads[0];
			look.Parts["ears"] = Active.Ears.Length > 0 ? Active.Ears : LpcMeta.None;
			if (Skins is { } skins && !skins.Contains(LpcLook.ColourOf(look, "body")))
				look.Colours["body"] = skins[0];
		}
	}
}
