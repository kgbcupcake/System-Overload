using Pastel;
using System.Text;

namespace Maries_Ui
{
	public static class GlitchText
	{
		// Pass charPool == null to draw from the full printable ASCII range instead of a fixed pool.
		public static string NextGlitchChar(Random rnd, string[]? charPool, string[] colorPool)
		{
			string ch = charPool != null
				? charPool[rnd.Next(charPool.Length)]
				: ((char)rnd.Next(33, 126)).ToString();

			return ch.Pastel(colorPool[rnd.Next(colorPool.Length)]);
		}

		public static string GenerateGlitchString(int length, string[]? charPool, string[] colorPool, Random? rnd = null)
		{
			rnd ??= new Random();
			var sb = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				sb.Append(NextGlitchChar(rnd, charPool, colorPool));
			}
			return sb.ToString();
		}
	}
}
