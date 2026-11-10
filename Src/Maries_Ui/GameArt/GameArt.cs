using Maries_Loaders;
using Pastel;
using System;

namespace Maries_Ui
{
	#region// Main Menu Art
	public static class MainMenuArt
	{
		public static void DrawMainHeader()
		{
			string separator = "[" + new string('=', Console.WindowWidth - 4) + "]";
			string titleText = Story.Line("main_menu.title");
			string subText = Story.Line("main_menu.subtitle");
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			UiEngine.DrawCentered(separator, 5);
			UiEngine.DrawCentered(titleText.Pastel("#FFAB00"), 6);
			UiEngine.DrawCentered(subText.Pastel("#B8860B"), 8);
			UiEngine.DrawCentered(separator, 10);
			Console.ResetColor();
		}
	}
	#endregion
	#region// Game Over Art
	public static class GameOver 
	{
		// The caption above and below the skull comes from the "game_over" story lines.
		public static readonly string[] Skull = new string[]
		{
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
			"___________________________________________"
		};
	}


	#endregion


}