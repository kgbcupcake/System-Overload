using System_Overload.Src.GameData.Components;
using System_Overload.Src.Utilities.UI;
using Pastel;
using Src.GameData.Components;
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
				Clear(); // Wipe the terminal before drawing the tech interface

				UiEngine.DrawCentered("=== SELECT HARDWARE FOR MODIFICATION ===".Pastel("#00FFFF"), 4);

				var weapons = player.Inventory.OfType<WeaponData>().ToList();

				if (weapons.Count == 0)
				{
					UiEngine.DrawCentered("No valid hardware detected in inventory buffer.".Pastel("#FF0000"), 10);
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

					UiEngine.DrawCentered("[ESC] BACK TO TERMINAL".Pastel("#FFD700"), 14 + weapons.Count);

					var key = ReadKey(true).Key;

					if (key == ConsoleKey.UpArrow) selected = (selected == 0) ? weapons.Count - 1 : selected - 1;
					if (key == ConsoleKey.DownArrow) selected = (selected == weapons.Count - 1) ? 0 : selected + 1;

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
				UiEngine.DrawCentered("CRITICAL: Hardware socket array is full!".Pastel("#FF0000"), 10);
				Thread.Sleep(1500);
				return;
			}

			if (gems.Count == 0)
			{
				UiEngine.DrawCentered("ERROR: No compatible Data-Gems found in buffer.".Pastel("#FF0000"), 10);
				Thread.Sleep(1500);
				return;
			}

			int selected = 0;
			bool injecting = true;

			while (injecting)
			{
				// We don't Clear() every frame here to prevent flickering, just move the cursor
				SetCursorPosition(0, 4);
				UiEngine.DrawCentered($"INJECTING INTO: {weapon.Name.Pastel("#00FFFF")}", 4);
				UiEngine.DrawCentered("Select Data-Gem for injection:", 6);

				for (int i = 0; i < gems.Count; i++)
				{
					string gemLine = (i == selected)
						? $">> {gems[i].Name} [+{gems[i].Power} Power] <<".Pastel("#00FF00")
						: $"    {gems[i].Name} [+{gems[i].Power} Power]    ".Pastel("#555555");
					UiEngine.DrawCentered(gemLine, 10 + i);
				}

				UiEngine.DrawCentered("[ESC] CANCEL INJECTION".Pastel("#FFD700"), 14 + gems.Count);

				var key = ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow) selected = (selected == 0) ? gems.Count - 1 : selected - 1;
				if (key == ConsoleKey.DownArrow) selected = (selected == gems.Count - 1) ? 0 : selected + 1;

				if (key == ConsoleKey.Enter)
				{
					weapon.SocketedGems.Add(gems[selected]);
					player.Inventory.Remove(gems[selected]);
					Clear();
					UiEngine.DrawCentered("LINK SUCCESSFUL: Data-Gem integrated.".Pastel("#00FF00"), 10);
					Thread.Sleep(1000);
					injecting = false;
				}

				if (key == ConsoleKey.Escape) injecting = false;
			}
		}
	}
}