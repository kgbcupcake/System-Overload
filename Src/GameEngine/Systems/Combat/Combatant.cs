using Src.GameData.Components;

namespace System_Overload.Src.GameEngine.Combat
{
	public class Combatant
	{
		public string Name { get; }
		public int MaxHealth { get; }
		public int Health { get; set; }
		public int Attack { get; }
		public int Defense { get; }

		public List<ItemData> Inventory { get; set; } = new List<ItemData>();
		public Combatant(string name, int maxHealth, int attack, int defense)
		{
			Name = name;
			MaxHealth = maxHealth;
			Health = maxHealth;
			Attack = attack;
			Defense = defense;
		}
	}
}
