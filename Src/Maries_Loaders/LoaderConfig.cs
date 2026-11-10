namespace Maries_Loaders
{
	public static class LoaderConfig
	{
		public static string ContentRoot { get; set; } = string.Empty;

		public static bool DevMode { get; set; } = false;

		public static string GetGlobalPath(string folder) => Path.Combine(ContentRoot, folder.ToLower());
	}
}
