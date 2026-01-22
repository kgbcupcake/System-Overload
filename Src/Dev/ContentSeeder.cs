using System_Overload.Src.GameData.Components;

namespace System_Overload.Dev
{
	public static class ContentSeeder
	{
		private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
		{
			// Creates the target directory (e.g., .../Data/sectors)
			Directory.CreateDirectory(target.FullName);

			// Only copy .json files that don't already exist to prevent overwriting user data
			foreach (FileInfo fi in source.GetFiles("*.json"))
			{
				string targetFilePath = Path.Combine(target.FullName, fi.Name);
				if (!File.Exists(targetFilePath))
				{
					fi.CopyTo(targetFilePath);
				}
			}
		}

		public static void SeedInitialContent()
		{
			string destRoot = GameState.MasterPath;
			string sourceRoot;

	
			sourceRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "GameData", "Templates");

	
			// Log the path so you can see if the ".." logic is reaching your WSL folder
			Console.WriteLine($"[SEEDER] Searching: {Path.GetFullPath(sourceRoot)}");

			if (!Directory.Exists(sourceRoot)) return;

			foreach (DirectoryInfo sourceSubDir in new DirectoryInfo(sourceRoot).GetDirectories())
			{
				// Force lowercase to match your GameState.GetGlobalPath logic
				string folderName = sourceSubDir.Name.ToLower();
				DirectoryInfo targetSubDir = new DirectoryInfo(Path.Combine(destRoot, folderName));

				CopyFilesRecursively(sourceSubDir, targetSubDir);
			}
		}
	}
}