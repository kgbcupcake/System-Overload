using Maries_DataModels;
using DungeonAdventures.Src.GameEngine.Combat;
using Maries_Loaders; // Added to access MonsterLoader

public static class EnemyFactory
{
	/// <summary>
	/// Builds the enemy for a floor stage. The id can be a monster id (monsters folder)
	/// or a boss id (bosses folder).
	/// </summary>
	public static Combatant CreateEnemy(string enemyId)
	{
		var monster = MonsterLoader.GetById(enemyId);
		if (monster != null)
			return FromMonster(monster);

		var def = BossLoader.GetBossById(enemyId);
		if (def != null)
			return FromBoss(def);

		// Unknown id: a generic enemy so the floor can still be played.
		return new Combatant($"Unknown Creature [{enemyId}]", 50, 12, 5) { Level = 2, XpReward = 10, CoinReward = 5 };
	}

	/// <summary>True if the id names a monster or a boss.</summary>
	public static bool Exists(string enemyId) =>
		MonsterLoader.GetById(enemyId) != null || BossLoader.GetBossById(enemyId) != null;

	private static Combatant FromMonster(MonsterData data)
	{
		return new Combatant(data.Name, data.MaxHealth, data.AttackPower, data.Defense)
		{
			Level = Math.Max(1, data.Level),
			Accuracy = data.Accuracy,
			CritChance = data.CritChance,
			AttackVerb = string.IsNullOrWhiteSpace(data.AttackVerb) ? "strikes" : data.AttackVerb,
			XpReward = data.Rewards?.Xp ?? 0,
			CoinReward = data.Rewards?.Coins ?? 0,
		};
	}

	private static Combatant FromBoss(BossDefinition def)
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

		var combatant = new Combatant(name, Math.Max(1, def.Stats.Health), def.Stats.Attack, def.Stats.Defense)
		{
			Level = Math.Max(1, def.Level),
			Accuracy = 90,
			CritChance = 8,
			XpReward = def.XpReward,
			CoinReward = def.CoinReward,
			Boss = boss,
		};
		foreach (var drop in def.DropTable)
		{
			if (!string.IsNullOrWhiteSpace(drop.ItemID))
				combatant.Drops.Add((drop.ItemID, drop.Chance));
		}
		return combatant;
	}

	/// <summary>
	/// The weapon the player fights with: the highest-damage weapon in their inventory.
	/// </summary>
	public static WeaponData? GetEquippedWeapon(PlayerData.loadPlayer player) =>
		player.Inventory.OfType<WeaponData>().OrderByDescending(w => w.Damage).FirstOrDefault();

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

		combatant.Level = player.Level;
		combatant.Accuracy = 90;
		combatant.Inventory = player.Inventory;
		combatant.Health = player.Health;
		return combatant;
	}
}
