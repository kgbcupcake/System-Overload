namespace Maries_DataModels
{
	/// <summary>
	/// A piece of armor or clothing worn in one equip slot. Type is "Armor" or "Clothing" (or any
	/// type the equipment rules list as a socket host); Slot is one of the rules' EquipSlots ids.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("armor")]
	public class ArmorData : ItemData
	{
		[MariesGui.Contracts.MariesGuiRef("EquipSlots.Id", "equipment")]
		public string Slot { get; set; } = "Chest";
		public int Defense { get; set; } = 0;
		public int BonusHealth { get; set; } = 0;

		/// <summary>True while the hero is wearing it. Only one item per slot is worn at a time.</summary>
		public bool IsEquipped { get; set; } = false;

		public ArmorData() : base()
		{
			Type = "Armor";
		}

		public override string ToString()
		{
			string socketText = NumSockets > 0 ? $" | Sockets: {SocketedGems.Count}/{NumSockets}" : "";
			return $"[{Rarity}] {Name} ({Type}, {Slot}) | Def: {Defense} | Val: {Value}g{socketText}";
		}
	}
}
