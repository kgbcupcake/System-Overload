namespace DungeonAdventures.Src.GameEngine.Combat
{
	/// <summary>
	/// The hero's danger rank (PlayerData.DifficultyRating: set by the calling, raised by each Dark Pact
	/// in the General Store) makes every monster and boss tougher, and pays better for beating them.
	/// All the numbers live here, so the screens that describe danger and the fights agree.
	/// </summary>
	public static class Danger
	{
		/// <summary>Extra foe health and attack per rank (0.10 = +10%).</summary>
		public const double StrengthPerRank = 0.10;
		/// <summary>Extra foe defense per rank.</summary>
		public const int DefensePerRank = 1;
		/// <summary>Extra XP and gold from a beaten foe per rank.</summary>
		public const double RewardPerRank = 0.10;

		public static int Health(int baseHealth, int rank) => Grow(baseHealth, StrengthPerRank, rank);
		public static int Attack(int baseAttack, int rank) => Grow(baseAttack, StrengthPerRank, rank);
		public static int Defense(int baseDefense, int rank) => baseDefense + DefensePerRank * Math.Max(0, rank);
		public static int Reward(int baseReward, int rank) => Grow(baseReward, RewardPerRank, rank);

		/// <summary>"+20%": how much stronger foes are at a rank.</summary>
		public static string Percent(int rank) => $"+{(int)Math.Round(StrengthPerRank * 100 * Math.Max(0, rank))}%";

		private static int Grow(int value, double perRank, int rank) =>
			rank <= 0 ? value : (int)Math.Round(value * (1 + perRank * rank));
	}
}
