using System_Overload.Src.Utilities.UI;
using Pastel;
using System;

namespace System_Overload.Src.Utilities.GameArt
{
	#region// Main Menu Art
	public static class MainMenuArt
	{
		public static void DrawMainHeader()
		{
			string separator = "[" + new string('=', Console.WindowWidth - 4) + "]";
			string titleText = "S Y S T E M   O V E R L O A D";
			string subText = "[ RESTORING CORE DATA... ]";
			Console.ForegroundColor = ConsoleColor.Cyan;
			UiEngine.DrawCentered(separator, 5);
			UiEngine.DrawCentered(titleText.Pastel("#00FFFF"), 6);
			UiEngine.DrawCentered(subText.Pastel("#008B8B"), 8);
			UiEngine.DrawCentered(separator, 10);
			Console.ResetColor();
		}
	}
	#endregion
	#region// Game Over Art
	public static class GameOver 
	{
		public static readonly string[] CynicalSkull = new string[]
		{
			"         YOU WERE JUST RENTED DATA         ",
			"___________________________________________",
			"         oooo$$$$$$$$oooo                  ",
			"      oo$$$$$$$$$$$$$$$$$$oo               ",
			"     od$$$$$$$$$$$$$$$$$$$$$bo             ",
			"     oo$$$$$$$$$$$$$$$$$$$$$$oo            ",
			"     $$$$$$$$$$$$$$$$$$$$$$$$$$            ",
			"     $$$$$$$$$$$$$$$$$$$$$$$$$$            ",
			"     $$$   $$$$$$$$$$$$   $$$$$            ",
			"     \"$$$   $$$$$$$$$$   $$$$$\"            ",
			"      \"$$$oooo$$$$$$$oooo$$$\"              ",
			"        \"$$$$$$$$$$$$$$$$$\"                ",
			"    oooo$$$$$$$$$$$$$$$$$$oooo             ",
			"   $$$$$$$$\"\"$$$$$$$$$??$$$$$$$            ",
			"    \"\"$$$$\"  \"$$$$$$$\"  \"$$$$\"             ",
			"       \"$$$    \"$$$\"    $$$\"               ",
			"___________________________________________",
			"          [ REALITY TERMINATED ]           "
		};
	}


	#endregion


}