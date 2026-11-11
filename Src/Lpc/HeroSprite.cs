using Maries_DataModels;
using Maries_Ui;

namespace DungeonAdventures.Src.Lpc
{
	/// <summary>
	/// The hero's LPC sprite as terminal rows: one walk frame, cropped to the character (wings included)
	/// and shrunk to two-thirds so it fits a 24-row screen. Used by the creator and the character sheet.
	/// </summary>
	public static class HeroSprite
	{
		// The part of a 64 px frame that is drawn, and the size it's shrunk to.
		private const int CropX = 6, CropY = 8, CropW = 52, CropH = 54;

		/// <summary>Columns the sprite takes.</summary>
		public const int Width = 35;
		/// <summary>Terminal rows the sprite takes (two pixels per row).</summary>
		public const int Rows = 18;

		/// <summary>Frames in the walk cycle.</summary>
		public const int Frames = 8;

		public static readonly string[] Directions = { "up", "left", "down", "right" };

		/// <summary>True when the hero can be drawn: the LPC catalog is installed and the hero has a look.</summary>
		public static bool CanDraw(HeroLook? look) => look != null && LpcMeta.Available;

		/// <summary>The hero facing `dir` (0 up, 1 left, 2 down, 3 right), walk frame `frame` (0-7).</summary>
		public static string[] Render(HeroLook look, int dir, int frame)
		{
			var full = LpcComposer.Frame(look, "walk", Directions[(dir % 4 + 4) % 4], 1 + (frame % Frames + Frames) % Frames);
			var small = LpcComposer.ShrinkCrisp(full, CropX, CropY, CropW, CropH, Width, Rows * 2);
			return PixelIcon.Render(small, Width, Rows * 2);
		}

		/// <summary>Columns a sprite `rows` tall takes (the crop keeps its shape).</summary>
		public static int WidthFor(int rows) => (int)Math.Round(rows * 2.0 * CropW / CropH);

		/// <summary>
		/// Any look standing still, `rows` terminal rows tall, facing `dir` (0 up = seen from behind, 2 down = facing
		/// you). Used by fights, where the hero has their back to you and the foe faces you.
		/// </summary>
		public static string[] RenderSized(HeroLook look, int dir, int rows)
		{
			var full = LpcComposer.Frame(look, "walk", Directions[(dir % 4 + 4) % 4], 0);
			int w = WidthFor(rows);
			return PixelIcon.Render(LpcComposer.ShrinkCrisp(full, CropX, CropY, CropW, CropH, w, rows * 2), w, rows * 2);
		}

		/// <summary>
		/// The raw pixels (0xRRGGBBAA) of a look facing `dir`, `rows` terminal rows tall, for scenes that lay the
		/// hero over a painted backdrop. Frame 0 stands still, 1-8 walk. `w` is the width in pixels.
		/// </summary>
		public static uint[] Pixels(HeroLook look, int dir, int frame, int rows, out int w)
		{
			var full = LpcComposer.Frame(look, "walk", Directions[(dir % 4 + 4) % 4], Math.Clamp(frame, 0, Frames));
			w = WidthFor(rows);
			return LpcComposer.ShrinkCrisp(full, CropX, CropY, CropW, CropH, w, rows * 2);
		}

		/// <summary>Writes the sprite with its top-left corner at (x, y).</summary>
		public static void Draw(HeroLook look, int x, int y, int dir, int frame)
		{
			var rows = Render(look, dir, frame);
			for (int r = 0; r < rows.Length; r++)
			{
				Console.SetCursorPosition(x, y + r);
				Console.Write(rows[r]);
			}
		}
	}
}
