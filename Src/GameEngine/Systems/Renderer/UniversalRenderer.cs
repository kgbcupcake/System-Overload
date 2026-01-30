using Spectre.Console;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SpectreColor = Spectre.Console.Color;

namespace System_Overload.GameEngine.Systems.Renderer
{
	public static class UniversalRenderer
	{
		private const string AssetRoot = "assets";

		/// <summary>
		/// Renders pixel art assets to the terminal while preserving crisp edges and detail.
		/// Uses nearest-neighbor resampling to maintain the pixel art aesthetic.
		/// </summary>
		public static void Render(
			string subFolder,
			string fileName,
			int maxWidth = 20,  // Increased from 32
			int maxHeight = 10,  // Increased from 10
			bool maintainAspectRatio = true,
			float terminalCharAspectRatio = 0.5f)  // Adjustable - typical is 0.5 (char is 2x taller than wide)
		{
			string path = Path.Combine(AssetRoot, subFolder, fileName);
			if (!File.Exists(path))
			{
				AnsiConsole.MarkupLine($"[red]Error: Asset not found at {path}[/]");
				return;
			}

			using var image = Image.Load<Rgba32>(path);

			// --- 1. Calculate dimensions preserving aspect ratio ---
			int targetWidth = maxWidth;
			int targetHeight = maxHeight;

			if (maintainAspectRatio)
			{
				// Image aspect ratio (width / height)
				float imageAspect = (float)image.Width / image.Height;

				// When rendering to terminal, we need to account for character shape
				// Terminal chars are typically ~2x taller than wide (aspect ~0.5)
				// So we need to compensate: effective display aspect = targetWidth * charAspect / targetHeight

				// Calculate scaled dimensions that fit within bounds
				float widthIfHeightMaxed = maxHeight * imageAspect / terminalCharAspectRatio;
				float heightIfWidthMaxed = maxWidth * terminalCharAspectRatio / imageAspect;

				if (widthIfHeightMaxed <= maxWidth)
				{
					// Height is the limiting factor
					targetHeight = maxHeight;
					targetWidth = (int)Math.Round(widthIfHeightMaxed);
				}
				else
				{
					// Width is the limiting factor
					targetWidth = maxWidth;
					targetHeight = (int)Math.Round(heightIfWidthMaxed);
				}

				// Ensure minimum size
				targetWidth = Math.Max(1, targetWidth);
				targetHeight = Math.Max(1, targetHeight);
			}

			// --- 2. Resize with nearest-neighbor for crisp pixel art ---
			var resizedImage = image.Clone(ctx =>
				ctx.Resize(new ResizeOptions
				{
					Size = new SixLabors.ImageSharp.Size(targetWidth, targetHeight),
					Sampler = KnownResamplers.NearestNeighbor,
					Mode = ResizeMode.Stretch
				})
			);

			// --- 3. Render to Spectre canvas ---
			var canvas = new Canvas(targetWidth, targetHeight);

			for (int y = 0; y < targetHeight; y++)
			{
				for (int x = 0; x < targetWidth; x++)
				{
					Rgba32 pixel = resizedImage[x, y];

					// Skip fully transparent pixels
					if (pixel.A < 128)
						continue;

					// Use color directly if fully opaque
					if (pixel.A == 255)
					{
						canvas.SetPixel(x, y, new SpectreColor(pixel.R, pixel.G, pixel.B));
					}
					else
					{
						// Blend with alpha for semi-transparent pixels
						// Assuming dark background for blending
						float alpha = pixel.A / 255f;
						byte r = (byte)(pixel.R * alpha);
						byte g = (byte)(pixel.G * alpha);
						byte b = (byte)(pixel.B * alpha);
						canvas.SetPixel(x, y, new SpectreColor(r, g, b));
					}
				}
			}

			resizedImage.Dispose();
			AnsiConsole.Write(canvas);
		}

		/// <summary>
		/// Render with explicit dimensions (no aspect ratio preservation)
		/// </summary>
		public static void RenderExact(
			string subFolder,
			string fileName,
			int width,
			int height)
		{
			Render(subFolder, fileName, width, height, maintainAspectRatio: false);
		}

		/// <summary>
		/// Render at original size (1:1 pixel mapping)
		/// Useful for testing/debugging
		/// </summary>
		public static void RenderOriginalSize(
			string subFolder,
			string fileName,
			int maxWidth = 120,
			int maxHeight = 40)
		{
			string path = Path.Combine(AssetRoot, subFolder, fileName);
			if (!File.Exists(path))
			{
				AnsiConsole.MarkupLine($"[red]Error: Asset not found at {path}[/]");
				return;
			}

			using var image = Image.Load<Rgba32>(path);

			int renderWidth = Math.Min(image.Width, maxWidth);
			int renderHeight = Math.Min(image.Height, maxHeight);

			var canvas = new Canvas(renderWidth, renderHeight);

			for (int y = 0; y < renderHeight; y++)
			{
				for (int x = 0; x < renderWidth; x++)
				{
					Rgba32 pixel = image[x, y];

					if (pixel.A < 128)
						continue;

					canvas.SetPixel(x, y, new SpectreColor(pixel.R, pixel.G, pixel.B));
				}
			}

			AnsiConsole.Write(canvas);
		}
	}
}