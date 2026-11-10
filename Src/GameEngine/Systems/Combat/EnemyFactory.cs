using Maries_DataModels;
using System_Overload.Src.GameEngine.Combat;
using Maries_Loaders; // Added to access DronesLoader

public static class EnemyFactory
{
	/// <summary>
	/// Builds the enemy for a sector stage. The id can be a drone id (drones folder)
	/// or an admin id (admins folder); admins fight as bosses.
	/// </summary>
	public static Combatant CreateEnemy(string enemyId)
	{
		var drone = DronesLoader.GetById(enemyId);
		if (drone != null)
			return FromDrone(drone);

		var admin = AdminLoader.GetAdminById(enemyId);
		if (admin != null)
			return FromAdmin(admin);

		// Unknown id: a generic enemy so the sector can still be played.
		return new Combatant($"Unknown Creature [{enemyId}]", 50, 12, 5) { Level = 2, XpReward = 10, CoinReward = 5 };
	}

	/// <summary>True if the id names a drone or an admin.</summary>
	public static bool Exists(string enemyId) =>
		DronesLoader.GetById(enemyId) != null || AdminLoader.GetAdminById(enemyId) != null;

	private static Combatant FromDrone(NewEnemyData data)
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

	private static Combatant FromAdmin(AdminData admin)
	{
		string name = string.IsNullOrWhiteSpace(admin.SystemName) ? admin.AdminID : admin.SystemName;

		var boss = new BossProfile
		{
			SpecialAbility = admin.CombatLogic.SpecialAbility,
			SpecialEvery = admin.CombatLogic.SpecialEvery,
			// CorruptionRate under 1 would make the boss weaker when enraged, so treat it as "no enrage".
			EnrageMultiplier = Math.Max(1.0, admin.Stats.CorruptionRate),
			HexColor = string.IsNullOrWhiteSpace(admin.Visuals.HexColor) ? "#B3001B" : admin.Visuals.HexColor,
			OnBreach = admin.Dialogue.OnBreach,
			OnDamage = admin.Dialogue.OnDamage,
			OnDeletion = admin.Dialogue.OnDeletion,
		};
		foreach (var attack in new[] { admin.CombatLogic.PrimaryAttack, admin.CombatLogic.SecondaryAttack })
		{
			if (!string.IsNullOrWhiteSpace(attack))
				boss.AttackNames.Add(attack);
		}

		var combatant = new Combatant(name, Math.Max(1, admin.Stats.Integrity), admin.Stats.ProcessingPower, admin.Stats.Firewall)
		{
			Level = Math.Max(1, admin.Level),
			Accuracy = 90,
			CritChance = 8,
			XpReward = admin.XpReward,
			CoinReward = admin.CoinReward,
			Boss = boss,
		};
		foreach (var drop in admin.DropTable)
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
