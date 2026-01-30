using System;

namespace System_Overload.Src.GameEngine.Combat
{
	public static class DamageCalculator
	{
		public static int CalculateDamage(Combatant attacker, Combatant defender)
		{
			// 1. Calculate the raw difference
			int baseDamage = attacker.Attack - defender.Defense;

			// 2. Calculate a 'Glance' hit (15% of attacker's power)
			// This ensures high-defense players still take some damage
			int minimumThreshold = (int)Math.Ceiling(attacker.Attack * 0.15f);

			// 3. Return the higher of the two, but never less than 1
			return Math.Max(minimumThreshold, baseDamage);
		}
	}
}