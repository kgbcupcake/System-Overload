using System.IO;
using System.Text.Json;

namespace System_Overload.Utilities
{
	/// <summary>
	/// Provides a single, static, generic utility to serialize objects to JSON files.
	/// This is the one and only JSON writer to be used in the project to avoid ambiguity.
	/// </summary>
	public static class JsonContentWriter
	{
		public static bool EnableContentGeneration { get; set; } = false;

		public static void WriteJson<T>(string filePath, T data)
		{
			if (!EnableContentGeneration)
				return;

			if (File.Exists(filePath))
				return;

			var options = new JsonSerializerOptions { WriteIndented = true };
			string jsonString = JsonSerializer.Serialize(data, options);
			File.WriteAllText(filePath, jsonString);
		}
	}


}
