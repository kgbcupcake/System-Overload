using Maries_DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Maries_Loaders
{
public class WeaponTemplate
{
	public string ID { get; set; } = Guid.NewGuid().ToString();
	public string Name { get; set; } = "New Weapon";
	public int Damage { get; set; } = 10;
	public string Description { get; set; } = "No Description";
	public ItemRarity Rarity { get; set; } = ItemRarity.Common;

	private static List<WeaponTemplate>? _cache;

	public static List<WeaponTemplate> LoadWeaponTemplates()
	{
		if (_cache != null) return _cache;

		_cache = UniversalLoader.LoadCollection<WeaponTemplate>("weapon_templates");
		return _cache;
	}

	public static void SaveToMaster(WeaponTemplate weapon)
	{
		var list = LoadWeaponTemplates();
		list.Add(weapon);

		string path = Path.Combine(LoaderConfig.GetGlobalPath("weapon_templates"), "master_weapons.json");
		JsonContentWriter.WriteJson(path, list, overwrite: true);

		ClearCache();
	}

	public static void ClearCache() => _cache = null;
}
}
