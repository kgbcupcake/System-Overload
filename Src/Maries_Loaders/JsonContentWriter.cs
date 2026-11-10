using System.IO;
using System.Text.Json;

namespace Maries_Loaders
{
	/// <summary>
	/// Provides a single, static, generic utility to serialize objects to JSON files.
	/// This is the one and only JSON writer to be used in the project to avoid ambiguity.
	/// </summary>
	public static class JsonContentWriter
	{
		public static bool EnableContentGeneration { get; set; } = false;

		public static void WriteJson<T>(string filePath, T data, bool overwrite = false, JsonSerializerOptions? options = null)
		{
			if (!EnableContentGeneration && !overwrite)
				return;

			if (File.Exists(filePath) && !overwrite)
				return;

			options ??= new JsonSerializerOptions { WriteIndented = true };
			string jsonString = JsonSerializer.Serialize(data, options);
			File.WriteAllText(filePath, jsonString);
		}
	}


}
