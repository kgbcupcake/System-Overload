using System_Overload.Src.GameData.Components;
using Maries_Ui;
using Maries_Loaders;
using Pastel;
using Maries_DataModels;
using static System.Console;

namespace Src.TownSquare.InterFaces
{
	internal class HardwareTech
	{
		public static void Open()
		{
			var player = GameState.CurrentPlayer;
			bool inHardwareMenu = true;

			// Kill the cursor so it doesn't leave a blinky trail
			CursorVisible = false;

			while (inHardwareMenu)
			{
				Clear(); // Wipe the screen before drawing the forge

				UiEngine.DrawCentered("=== CHOOSE A WEAPON TO SOCKET ===".Pastel("#FFAB00"), 4);

				var weapons = player.Inventory.OfType<WeaponData>().ToList();

				if (weapons.Count == 0)
				{
					UiEngine.DrawCentered("You have no weapons to work on.".Pastel("#FF0000"), 10);
					UiEngine.DrawCentered("[Press any key to return]".Pastel("#555555"), 12);
					ReadKey(true);
					inHardwareMenu = false;
					break;
				}

				int selected = 0;
				bool selecting = true;

				while (selecting)
				{
					// Draw the list
					for (int i = 0; i < weapons.Count; i++)
					{
						int used = weapons[i].SocketedGems.Count;
						int total = weapons[i].NumSockets;

						string weaponLine = (i == selected)
							? $">> {weapons[i].Name.ToUpper()} [{used}/{total} Sockets Used] <<".Pastel("#00FF00")
							: $"    {weapons[i].Name} [{used}/{total} Sockets Used]    ".Pastel("#555555");

						UiEngine.DrawCentered(weaponLine, 8 + i);
					}

					UiEngine.DrawCentered("[ESC] BACK TO TOWN".Pastel("#FFD700"), 14 + weapons.Count);

					var key = ReadKey(true).Key;

					if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow) selected = MenuNavigator.HandleVerticalNav(key, selected, weapons.Count);

					if (key == ConsoleKey.Enter)
					{
						SocketingSubMenu(weapons[selected]);
						// After returning from sub-menu, we break 'selecting' to re-clear and re-draw the main list
						selecting = false;
					}

					if (key == ConsoleKey.Escape)
					{
						selecting = false;
						inHardwareMenu = false;
					}
				}
			}

			// THE NUKE: Clear the screen one last time before exiting the method
			// This ensures the hardware text is GONE before the main menu even starts to think about redrawing.
			Clear();
		}

		private static void SocketingSubMenu(WeaponData weapon)
		{
			var player = GameState.CurrentPlayer;
			var gems = player.Inventory.OfType<GemData>().ToList();

			Clear();

			if (weapon.SocketedGems.Count >= weapon.NumSockets)
			{
				UiEngine.DrawCentered("Every socket on this weapon is already filled!".Pastel("#FF0000"), 10);
				Thread.Sleep(1500);
				return;
			}

			if (gems.Count == 0)
			{
				UiEngine.DrawCentered("You have no gems to set.".Pastel("#FF0000"), 10);
				Thread.Sleep(1500);
				return;
			}

			int selected = 0;
			bool socketing = true;

			while (socketing)
			{
				// We don't Clear() every frame here to prevent flickering, just move the cursor
				SetCursorPosition(0, 4);
				UiEngine.DrawCentered($"SOCKETING: {weapon.Name.Pastel("#FFAB00")}", 4);
				UiEngine.DrawCentered("Choose a gem:", 6);

				for (int i = 0; i < gems.Count; i++)
				{
					string gemLine = (i == selected)
						? $">> {gems[i].Name} [+{gems[i].Power} Power] <<".Pastel("#00FF00")
						: $"    {gems[i].Name} [+{gems[i].Power} Power]    ".Pastel("#555555");
					UiEngine.DrawCentered(gemLine, 10 + i);
				}

				UiEngine.DrawCentered("[ESC] CANCEL".Pastel("#FFD700"), 14 + gems.Count);

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow) selected = MenuNavigator.HandleVerticalNav(key, selected, gems.Count);

				if (key == ConsoleKey.Enter)
				{
					var gem = gems[selected];
					weapon.SocketedGems.Add(gem);
					player.Inventory.Remove(gem);
					Clear();
					UiEngine.DrawCentered(Story.Pick("blacksmith.socketed", ("gem", gem.Name), ("weapon", weapon.Name)).Pastel("#00FF00"), 10);
					Thread.Sleep(1000);
					socketing = false;
				}

				if (key == ConsoleKey.Escape) socketing = false;
			}
		}
	}
}