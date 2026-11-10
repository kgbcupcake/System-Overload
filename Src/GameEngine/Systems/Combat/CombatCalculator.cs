﻿using System;

namespace DungeonAdventures.Src.GameEngine.Combat
{
	/// <summary>Outcome of one attack.</summary>
	public readonly record struct AttackResult(bool Hit, bool Critical, int Damage);

	public static class DamageCalculator
	{
		private static readonly Random _rng = new();

		public const double CritMultiplier = 1.5;

		/// <summary>
		/// Rolls one attack: hit chance, damage in the attacker's range, crit, then defense.
		/// </summary>
		/// <param name="damageMultiplier">Extra scaling, e.g. a boss special or enrage.</param>
		/// <param name="alwaysHit">Skip the hit roll (boss specials can't miss).</param>
		public static AttackResult Roll(Combatant attacker, Combatant defender, double damageMultiplier = 1.0, bool alwaysHit = false)
		{
			if (!alwaysHit && _rng.Next(100) >= HitChance(attacker, defender))
				return new AttackResult(false, false, 0);

			double raw = _rng.Next(attacker.MinDamage, attacker.MaxDamage + 1) * damageMultiplier;

			bool critical = _rng.Next(100) < attacker.CritChance;
			if (critical)
				raw *= CritMultiplier;

			return new AttackResult(true, critical, Mitigate((int)Math.Round(raw), defender));
		}

		/// <summary>
		/// Percent chance to hit: the attacker's accuracy, +/-3 per level of difference,
		/// kept between 50 and 98 so nothing is a guaranteed hit or miss.
		/// </summary>
		public static int HitChance(Combatant attacker, Combatant defender)
		{
			int chance = attacker.Accuracy + (attacker.Level - defender.Level) * 3;
			return Math.Clamp(chance, 50, 98);
		}

		/// <summary>
		/// Damage after defense. A hit always does at least 15% of its raw damage
		/// (a "glance"), so high defense slows fights down but never stops them.
		/// </summary>
		public static int Mitigate(int rawDamage, Combatant defender)
		{
			int defense = defender.Defense + defender.BonusDefense;
			int glance = (int)Math.Ceiling(rawDamage * 0.15);
			return Math.Max(1, Math.Max(glance, rawDamage - defense));
		}

		/// <summary>Average hit with no rolls, kept for older callers.</summary>
		public static int CalculateDamage(Combatant attacker, Combatant defender) => Mitigate(attacker.Attack, defender);
	}
}
