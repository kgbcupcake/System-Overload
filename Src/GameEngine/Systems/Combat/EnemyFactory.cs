using Maries_DataModels;
using DungeonAdventures.Src.GameEngine.Combat;
using Maries_Loaders; // Added to access MonsterLoader
using DungeonAdventures.Src.Lpc;

public static class EnemyFactory
{
	/// <summary>
	/// Builds the enemy for a floor stage. The id can be a monster id (monsters folder)
	/// or a boss id (bosses folder). `danger` is the hero's danger rank: each rank makes the foe
	/// tougher and its rewards bigger (see Danger).
	/// </summary>
	public static Combatant CreateEnemy(string enemyId, int danger = 0)
	{
		var monster = MonsterLoader.GetById(enemyId);
		if (monster != null)
			return FromMonster(monster, danger);

		var def = BossLoader.GetBossById(enemyId);
		if (def != null)
			return FromBoss(def, danger);

		// Unknown id: a generic enemy so the floor can still be played.
		return new Combatant($"Unknown Creature [{enemyId}]", Danger.Health(50, danger), Danger.Attack(12, danger), Danger.Defense(5, danger))
			{ Level = 2, XpReward = Danger.Reward(10, danger), CoinReward = Danger.Reward(5, danger), Look = EnemyLooks.Roll(enemyId, "") };
	}

	/// <summary>True if the id names a monster or a boss.</summary>
	public static bool Exists(string enemyId) =>
		MonsterLoader.GetById(enemyId) != null || BossLoader.GetBossById(enemyId) != null;

	private static Combatant FromMonster(MonsterData data, int danger)
	{
		return new Combatant(data.Name, Danger.Health(data.MaxHealth, danger), Danger.Attack(data.AttackPower, danger), Danger.Defense(data.Defense, danger))
		{
			Level = Math.Max(1, data.Level),
			Accuracy = data.Accuracy,
			CritChance = data.CritChance,
			AttackVerb = string.IsNullOrWhiteSpace(data.AttackVerb) ? "strikes" : data.AttackVerb,
			XpReward = Danger.Reward(data.Rewards?.Xp ?? 0, danger),
			CoinReward = Danger.Reward(data.Rewards?.Coins ?? 0, danger),
			Look = EnemyLooks.Roll(data.Id, data.Name),
		};
	}

	private static Combatant FromBoss(BossDefinition def, int danger)
	{
		string name = string.IsNullOrWhiteSpace(def.Name) ? def.Id : def.Name;

		var boss = new BossProfile
		{
			SpecialAbility = def.Moves.SpecialAbility,
			SpecialEvery = def.Moves.SpecialEvery,
			// EnrageMultiplier under 1 would make the boss weaker when enraged, so treat it as "no enrage".
			EnrageMultiplier = Math.Max(1.0, def.Stats.EnrageMultiplier),
			HexColor = string.IsNullOrWhiteSpace(def.Visuals.HexColor) ? "#B3001B" : def.Visuals.HexColor,
			OnEncounter = def.Dialogue.OnEncounter,
			OnDamage = def.Dialogue.OnDamage,
			OnDeath = def.Dialogue.OnDeath,
		};
		foreach (var attack in new[] { def.Moves.PrimaryAttack, def.Moves.SecondaryAttack })
		{
			if (!string.IsNullOrWhiteSpace(attack))
				boss.AttackNames.Add(attack);
		}

		var combatant = new Combatant(name, Danger.Health(Math.Max(1, def.Stats.Health), danger), Danger.Attack(def.Stats.Attack, danger), Danger.Defense(def.Stats.Defense, danger))
		{
			Level = Math.Max(1, def.Level),
			Accuracy = 90,
			CritChance = 8,
			XpReward = Danger.Reward(def.XpReward, danger),
			CoinReward = Danger.Reward(def.CoinReward, danger),
			Boss = boss,
			Look = EnemyLooks.Roll(def.Id, name, def.Visuals.ArtRef),
		};
		foreach (var drop in def.DropTable)
		{
			if (!string.IsNullOrWhiteSpace(drop.ItemID))
				combatant.Drops.Add((drop.ItemID, drop.Chance));
		}
		return combatant;
	}

	/// <summary>
	/// The weapon the player fights with: the one they equipped, else their highest-damage weapon.
	/// </summary>
	public static WeaponData? GetEquippedWeapon(PlayerData.loadPlayer player) => player.GetEquippedWeapon();

	/// <summary>
	/// Builds the player's side of the fight from their stats and best weapon.
	/// </summary>
	public static Combatant CreatePlayer(PlayerData.loadPlayer player)
	{
		var combatant = new Combatant(
			player.PlayerName,
			(int)player.GetTotalMaxHealth(),
			(int)player.GetTotalDamage(),
			(int)player.ArmorValue);

		// Stat and gem bonus on top of the weapon: GetTotalDamage is 10 (unarmed) + bonuses.
		int bonus = Math.Max(0, (int)player.GetTotalDamage() - 10);

		var weapon = GetEquippedWeapon(player);
		if (weapon != null)
		{
			int min = weapon.MinDamage > 0 ? weapon.MinDamage : (int)Math.Round(weapon.Damage * 0.8);
			int max = weapon.MaxDamage > 0 ? weapon.MaxDamage : (int)Math.Round(weapon.Damage * 1.2);
			combatant.MinDamage = Math.Max(1, min + bonus);
			combatant.MaxDamage = Math.Max(combatant.MinDamage, max + bonus);
			combatant.CritChance = weapon.CritChance;
			combatant.AttackVerb = $"hit with {weapon.Name} and deal";
		}
		else
		{
			combatant.MinDamage = 8 + bonus;
			combatant.MaxDamage = 12 + bonus;
			combatant.AttackVerb = "punch and deal";
		}

		// Wisdom (+1% per point) and gems in equipped gear sharpen crit (see equipment rules JSON).
		combatant.CritChance = Math.Clamp(combatant.CritChance + player.WisdomPoints + (int)Math.Round(player.GetSocketBonus(SocketStat.CritChance)), 0, 100);

		combatant.Level = player.Level;
		combatant.Accuracy = 90 + (int)Math.Round(player.GetSocketBonus(SocketStat.Accuracy));
		combatant.Inventory = player.Inventory;
		combatant.Health = player.Health;
		// A hero made before the creator existed has no look: they fight in the starting one.
		combatant.Look = player.Look ?? (LpcMeta.Available ? LpcLook.Default() : null);
		return combatant;
	}
}
