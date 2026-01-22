// DEPRECATED: This UI is being replaced by the Hostile Terminal.
// using ClickableTransparentOverlay;
// using ImGuiNET;
using DungeonAdventures.Src.GameEngine;
using DungeonAdventures.Src.GameEngine.GameData;
using Pastel;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.ComponentModel;
using DungeonAdventures.Src.GameData;
using DungeonAdventures.Src.Adventures.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


// DEPRECATED: DevGuiRenderer class and its contents are deprecated.
// All functionality is being replaced by the Hostile Terminal.
/*
namespace DungeonAdventures.Src.Interfaces
{

	public partial class DevGuiRenderer : Overlay
	{
		// Color mapping for ParseAndRenderColoredText
		private static readonly Dictionary<string, Vector4> _colorMap = new Dictionary<string, Vector4>(StringComparer.OrdinalIgnoreCase)
		{
			{"Red", new Vector4(1.0f, 0.0f, 0.0f, 1.0f)},
			{"Green", new Vector4(0.0f, 1.0f, 0.0f, 1.0f)},
			{"Blue", new Vector4(0.0f, 0.0f, 1.0f, 1.0f)},
			{"White", new Vector4(1.0f, 1.0f, 1.0f, 1.0f)},
			{"Black", new Vector4(0.0f, 0.0f, 0.0f, 1.0f)},
			{"Yellow", new Vector4(1.0f, 1.0f, 0.0f, 1.0f)},
			{"Cyan", new Vector4(0.0f, 1.0f, 1.0f, 1.0f)},
			{"Magenta", new Vector4(1.0f, 0.0f, 1.0f, 1.0f)},
			{"Gray", new Vector4(0.5f, 0.5f, 0.5f, 1.0f)},
			{"DarkRed", new Vector4(0.5f, 0.0f, 0.0f, 1.0f)},
			{"DarkGreen", new Vector4(0.0f, 0.5f, 0.0f, 1.0f)},
			{"DarkBlue", new Vector4(0.0f, 0.0f, 0.5f, 1.0f)},
            // Add more colors as needed
        };

		private static void ParseAndRenderColoredText(string text)
		{
			// Corrected regex to parse [ColorName]Text correctly
			var regex = new Regex(@"\\[([a-zA-Z]+)\](.[^\\[]*)");
			var matches = regex.Matches(text);

			if (matches.Count == 0)
			{
				ImGui.TextUnformatted(text);
				return;
			}

			int lastIndex = 0;
			foreach (Match match in matches)
			{
				// Render text before the colored segment
				if (match.Index > lastIndex)
				{
					ImGui.TextUnformatted(text.Substring(lastIndex, match.Index - lastIndex));
					ImGui.SameLine(0, 0); // Keep on the same line without spacing
				}

				string colorName = match.Groups[1].Value;
				string segmentText = match.Groups[2].Value;

				if (_colorMap.TryGetValue(colorName, out Vector4 color))
				{
					ImGui.TextColored(color, segmentText);
				}
				else
				{
					// If color not found, render as uncolored text
					ImGui.TextUnformatted(segmentText);
				}
				ImGui.SameLine(0, 0); // Keep on the same line without spacing
				lastIndex = match.Index + match.Length;
			}

			// Render any remaining text after the last colored segment
			if (lastIndex < text.Length)
			{
				ImGui.TextUnformatted(text.Substring(lastIndex));
			}
		}

		#region Variables and State Management
		// Window & Engine State
		private bool _isFirstFrame = true;
		private DateTime _lastToggleTime = DateTime.MinValue;
		private bool _devModeInternal = false;
		private bool _logPaused = false;
		private string _logFilter = "";
		private string _profileSearchFilter = "";
		public string GameVersion { get; set; }

		public bool IsVisible { get; set; } = false;

		private DateTime _lastF8ToggleTime = DateTime.Now;

		// File & Profile Management System
		private bool _showDeleteConfirm = false;
		private bool _showEditPopup = false;
		private bool _showCreatePopup = false;
		private string _profileToDelete = "";
		private string _profileToEdit = "";
		private string _jsonEditorBuffer;
		private string _currentLoadedFileName = "None";
		private string _newProfileName = "NewHero";
		private string _lastSavedTime = "Never";
		private loadPlayer _tempNewPlayer = new loadPlayer();
		public static bool NeedsMenuRedraw = false; //

		// Notification & Injection System
		public static string _injectionMsg = "";
		public static bool _injectionSuccess = false;
		public static bool _showInjectionPopup = false;


		// Text RPG Engine Testing Variables
		private string _testDialogue = "The old man looks at {PlayerName} and says, '[Red]Beware the shadows![White]'";
		private string _testRoomDesc = "You stand in a cold, damp cell. The stone walls are covered in moss, and a single rusted chain hangs from the ceiling.";
		private Vector3 _warpCoords = new Vector3(0, 0, 0);
		public string _targetRoomID = "Cellar_01";
		public bool _showWarpMenu = false;

		// Theme related variables
		private Vector4 color1 = new Vector4(1, 1, 1, 1);       // Font
		private Vector4 BorderColor = new Vector4(0, 1, 1, 1);  // Border
		private float sd1 = 1.0f; // Window Border Size
		private float sd2 = 1.0f; // Frame Border Size Child
		private float sd4 = 1.0f; // Child Border Size
		private float sd5 = 5.0f; // Window Rounding
		private float sd6 = 4.0f; // Frame Rounding
		private float sd7 = 4.0f; // Child Rounding
		private Vector2 sd3 = new Vector2(900, 700); // Window Size
		private Vector4 TgbColor = new Vector4(0.1f, 0.1f, 0.1f, 1.0f); // 1.0f is fully opaque
		private Vector4 ChildBgColor = new Vector4(0.08f, 0.08f, 0.12f, 1.0f);


		private string _configPath = Path.Combine(GameState.MasterPath, "settings", "gui_config.json");
		private string _warpFilter = "";
		private List<string> _availableMaps = new List<string>(); // Used by PopulateAvailableMaps

		// Fields for Shop Stock and Adventure Manager
		private List<ItemData> _shopStockItems = new();
		private List<AdventureData> _availableAdventures = new();
		private string _adventureSearchFilter = "";

		public bool _showSocketingInterface = false;
		public WeaponData _weaponToSocket = null;
		public bool _showPlayerPropertyEditor = false;
		public object _playerToEdit = null;

		[DllImport("user32.dll")]
		static extern short GetAsyncKeyState(int key);
		#endregion

		public DevGuiRenderer()
		{
			GameVersion = "Alpha 0.0.1";
			_jsonEditorBuffer = string.Empty;

			LoadTheme();
			PopulateAvailableMaps();
		}

		private void PopulateAvailableMaps()
		{
			_availableMaps.Clear();
			string dungeonsPath = Path.Combine(GameState.MasterPath, "dungeons");
			string adventuresPath = Path.Combine(GameState.MasterPath, "adventures");

			if (Directory.Exists(dungeonsPath))
			{
				foreach (var file in Directory.GetFiles(dungeonsPath, "*.json"))
				{
					_availableMaps.Add(Path.GetFileNameWithoutExtension(file));
				}
			}

			if (Directory.Exists(adventuresPath))
			{
				foreach (var file in Directory.GetFiles(adventuresPath, "*.json"))
				{
					_availableMaps.Add(Path.GetFileNameWithoutExtension(file));
				}
			}
			_availableMaps.Sort();
		}

		public static class DevLog
		{
			public static List<string> Buffer = new List<string>();
			private static int _maxLines = 200;
			public static readonly object _lock = new object();

			public static void Write(string message, string type = "INFO")
			{
				if (string.IsNullOrWhiteSpace(message)) return;
				lock (_lock)
				{
					string timestamp = DateTime.Now.ToString("HH:mm:ss");
					string formattedLine = message.StartsWith("[")
						? formattedLine = $"[{timestamp}] [{type}] {message}"
						: formattedLine = $"[{timestamp}] [INFO] {message}";

					Buffer.Add(formattedLine);

					if (Buffer.Count > _maxLines)
					{
						Buffer.RemoveRange(0, Buffer.Count - _maxLines);
					}
				}
			}
		}
	}
*/