using System_Overload.Src.GameData.Components;
using System_Overload.Src.GameData.Loaders;
using System_Overload.Src.Utilities.UI;
using Pastel;
using Src.Adventures.Encounters.MainEncounters;
using static System.Console;

namespace System_Overload.Src.Adventures.Encounters.CumstomEncounters
{
	internal static class Bosses
	{
		public static void WizardBoss()
		{
			var player = GameState.CurrentPlayer;

			if (player.Health <= 15)
			{
				WriteLine("You are too weak to face what lies beyond this door...");
				ReadKey(true);
				return;
			}

			Clear();
			WriteLine("The door rips open!");
			WriteLine("Dark Wizard: You dare interrupt my research?");
			ReadKey(true);

			var enemy = DronesLoader.GetById("dark_wizard");
			MainEncounter.Combat(enemy);
		}

		public static void RatBoss()
		{
			var player = GameState.CurrentPlayer;

			Clear();
			UiEngine.DrawCentered("BOSS ENCOUNTER".Pastel("#D60B18"), 10);
			WriteLine("A massive plague-ridden rat emerges...");
			ReadKey(true);

			var enemy = DronesLoader.GetById("rat_king");
			MainEncounter.Combat(enemy);
		}
	}
}
