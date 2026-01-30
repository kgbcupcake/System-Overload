using System_Overload.Src.GameData.Components;
using Src.GameData.Components;
using Src.GameEngine.Systems;

namespace System_Overload.Src.GameEngine.Systems
{
	public class ModdingStation
	{
		/// <summary>
		/// Logic for attaching mods to weapons. 
		/// This satisfies the call in your Blacksmith interface.
		/// </summary>
		public void InstallAttachment(WeaponData weapon, AttachmentData mod)
		{
			if (weapon.Attachments.Count < weapon.NumSockets)
			{
				weapon.Attachments.Add(mod);

				// Example logic: Apply stat bonuses from the mod
				// weapon.Damage += mod.PowerBonus; 
			}
		}

		// Overload for WeaponTemplate if needed for master data migration
		public void InstallAttachment(WeaponTemplate weapon, AttachmentData mod)
		{
			// Handle template-level logic here
		}
	}
}