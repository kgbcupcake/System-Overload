using System.Text;

namespace DungeonAdventures.Src.Scenes
{
	/// <summary>A grid of 0xRRGGBBAA pixels. 0 means nothing there (fully clear).</summary>
	public sealed class Canvas
	{
		public int W { get; }
		public int H { get; }
		public uint[] Px { get; }

		public Canvas(int w, int h)
		{
			W = w;
			H = h;
			Px = new uint[w * h];
		}

		public bool Inside(int x, int y) => x >= 0 && x < W && y >= 0 && y < H;

		public uint this[int x, int y]
		{
			get => Inside(x, y) ? Px[y * W + x] : 0;
			set { if (Inside(x, y)) Px[y * W + x] = value; }
		}

		public void CopyTo(Canvas other) => Array.Copy(Px, other.Px, Math.Min(Px.Length, other.Px.Length));
	}

	/// <summary>
	/// Shows a canvas in the terminal with half-blocks (two pixels a cell, 24-bit colour), from a given
	/// terminal row down. After the first frame only the cells that changed are written, so a scene
	/// can animate at ~15 fps without flicker.
	/// </summary>
	public sealed class CanvasScreen
	{
		private readonly int _top;
		private uint[]? _shown;

		/// <param name="topRow">The terminal row (0-based) the canvas's first two pixel rows go in.</param>
		public CanvasScreen(int topRow) => _top = topRow;

		/// <summary>Forget what's on screen, so the next frame is written in full (after a Clear).</summary>
		public void Invalidate() => _shown = null;

		public void Present(Canvas c)
		{
			var px = c.Px;
			int w = c.W, rows = c.H / 2;
			bool Changed(int r, int x)
			{
				if (_shown == null) return true;
				int a = 2 * r * w + x, b = a + w;
				return px[a] != _shown[a] || px[b] != _shown[b];
			}

			var sb = new StringBuilder();
			for (int r = 0; r < rows; r++)
			{
				int x = 0;
				while (x < w)
				{
					if (!Changed(r, x)) { x++; continue; }
					sb.Append("\x1b[").Append(_top + r + 1).Append(';').Append(x + 1).Append('H');
					while (x < w && Changed(r, x))
					{
						uint top = px[2 * r * w + x], bottom = px[(2 * r + 1) * w + x];
						sb.Append("\x1b[38;2;").Append(top >> 24).Append(';').Append((top >> 16) & 0xFF).Append(';').Append((top >> 8) & 0xFF)
						  .Append("m\x1b[48;2;").Append(bottom >> 24).Append(';').Append((bottom >> 16) & 0xFF).Append(';').Append((bottom >> 8) & 0xFF)
						  .Append("m▀");
						x++;
					}
					sb.Append("\x1b[0m");
				}
			}
			_shown = (uint[])px.Clone();
			if (sb.Length > 0) Console.Write(sb.ToString());
		}
	}
}
