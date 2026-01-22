using System.Linq;

namespace Src.GameData.Components
{
	public class WeaponData : ItemData
	{

		public int Damage { get; set; } = 10;
		public int MagSize { get; set; } = 0;
		public string AmmoType { get; set; } = "None";
		public int CritChance { get; set; } = 5;
		public int MaxDamage { get; set; }
		public int MinDamage { get; set; }
		public List<AttachmentData> Attachments { get; set; } = new List<AttachmentData>();

		public WeaponData() : base() { }

		public WeaponData(string name, int damage, float weight, int price, ItemRarity rarity, string type, EffectType effect, int durability, int magSize, string ammoType, int critChance, int numSockets)
			: base(name, price, weight, 1, type, rarity, effect, durability, 0, false, 0)
		{
			Damage = damage;
			MagSize = magSize;
			AmmoType = ammoType;
			CritChance = critChance;
			this.NumSockets = numSockets;
		}

		public float GetModifiedDamage()
		{
			float totalDamageMod = 1.0f;
			foreach (var attachment in Attachments)
			{
				totalDamageMod *= attachment.DamageMod;
			}
			return Damage * totalDamageMod;
		}

		public override string ToString()
		{
			string extraStats = "";
			if (Type == "Melee" && Durability > 0)
			{
				extraStats = $" | Integrity: {Durability}/{Durability}";
			}
			else if ((Type == "Firearm" || Type == "Ranged" || Type == "Heavy") && MagSize > 0)
			{
				string effectStr = Effect != EffectType.None ? $" [{Effect}]" : "";
				extraStats = $" | Mag: {MagSize}{effectStr} | Caliber: {AmmoType}";
			}
			else if (Type == "Exotic" && MagSize > 0)
			{
				string effectStr = Effect != EffectType.None ? $" [{Effect}]" : "";
				extraStats = $" | Charge: {MagSize}{effectStr} | Energy: {AmmoType}";
			}

			string socketInfo = NumSockets > 0 ? $" | Sockets: {SocketedGems.Count}/{NumSockets}" : "";
			return $"[{Rarity.ToString()}] {Name} ({Type}){extraStats} | Dmg: {Damage} | Wgt: {Weight} | Val: {Value}g | Crit: {CritChance}%{socketInfo}";
		}
	}
}