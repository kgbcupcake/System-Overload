using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine.Combat;
using DungeonAdventures.Src.Village.Places;
using Maries_DataModels;
using Maries_Loaders;
using Pastel;
using static DungeonAdventures.Src.Village.Places.ItemPanels;

namespace DungeonAdventures.Src.Game.Interfaces
{
	/// <summary>
	/// The three callings, their paths, and what each starts with. Character creation and the class
	/// screens both read this, so what the screen promises is what the hero gets. A path is the
	/// calling's flavour: it shares the calling's health, danger rank, gold and starting kit.
	/// </summary>
	internal static class ClassBook
	{
		public sealed record Calling(string Name, string Color, string[] Paths, int Health, int Danger, int Gold);

		public static readonly Calling[] Callings =
		{
			new("Warrior", "#FF4500", new[] { "Knight", "Paladin", "Berserker" }, Health: 150, Danger: 0, Gold: 50),
			new("Rogue", "#9370DB", new[] { "Assassin", "Shadowblade", "Ranger" }, Health: 100, Danger: 1, Gold: 150),
			new("Mage", "#00BFFF", new[] { "Wizard", "Warlock", "Battlemage" }, Health: 120, Danger: 2, Gold: 25),
		};

		/// <summary>The calling a class or path belongs to (Knight -> Warrior), or null.</summary>
		public static Calling? Of(string classOrPath) =>
			Callings.FirstOrDefault(c => c.Name == classOrPath || c.Paths.Contains(classOrPath));

		/// <summary>The items a calling starts with (fresh copies each call).</summary>
		public static List<ItemData> Kit(string classOrPath) => Of(classOrPath)?.Name switch
		{
			"Warrior" => new()
			{
				new WeaponData("Iron Warhammer", 18, 12.0f, 250, ItemRarity.Common, "Heavy", EffectType.Defense, 100, 0, "None", 5, 2),
				new ItemData("Healing Potion", 50, 0.1f, 2, "Consumable", ItemRarity.Common, EffectType.Restorative, 0, 0, false, 0),
			},
			"Rogue" => new()
			{
				new WeaponData("Whisper Dagger", 25, 1.5f, 400, ItemRarity.Uncommon, "Melee", EffectType.Speed, 50, 0, "None", 20, 1),
				new ItemData("Lockpick Set", 75, 0.2f, 1, "Tool", ItemRarity.Rare, EffectType.None, 1, 0, false, 0),
			},
			"Mage" => new()
			{
				new WeaponData("Oak Staff", 15, 4.0f, 300, ItemRarity.Common, "Magic", EffectType.Electric, 100, 0, "None", 10, 4),
				new GemData { Name = "Mana Crystal", Power = 5.0f, Attributes = new List<EffectType> { EffectType.Electric } },
			},
			_ => new(),
		};
	}

	/// <summary>
	/// Character creation's "choose your calling" and "choose your path" screens, in the inventory's
	/// two-panel look: the choices on the left, the selected one's icon, story and starting numbers
	/// on the right. Escape on the path screen goes back to the callings.
	/// </summary>
	internal static class ClassPickScreen
	{
		private const string PlaceColor = "#5A3A1A";

		/// <summary>Gold every hero has before the calling's own (PlayerData's default).</summary>
		private static readonly int StartingCoins = new PlayerData.loadPlayer().Coins;

		/// <summary>Picks a calling, then one of its paths, and sets the hero's class to the path.</summary>
		public static void Run(PlayerData.loadPlayer player)
		{
			int calling = 0;
			string message = Story.Line("creation.greeting", ("name", player.PlayerName)).Pastel(Amber);
			while (true)
			{
				var rows = ClassBook.Callings.Select(c => new PanelRow(c.Name, c.Color, Right: string.Join(" · ", c.Paths), RightColor: Muted)).ToList();
				var choice = Choose("CHOOSE YOUR CALLING", PlaceColor, player, rows, i => CallingDetail(ClassBook.Callings[i]),
					message, Summary(), "Choose", ref calling);
				if (choice.Back)
				{
					message = "Every hero needs a calling. Pick one to go on.".Pastel(Amber);
					continue;
				}

				string? path = PickPath(player, ClassBook.Callings[choice.Index]);
				if (path != null)
				{
					player.PlayerClass = path;
					return;
				}
				message = "";
			}
		}

		/// <summary>The path screen for one calling; null when the player goes back.</summary>
		private static string? PickPath(PlayerData.loadPlayer player, ClassBook.Calling calling)
		{
			int selected = 0;
			var rows = calling.Paths.Select(p => new PanelRow(p, calling.Color)).ToList();
			string prompt = Story.Has("creation.subclass_prompt")
				? Story.Line("creation.subclass_prompt", ("class", calling.Name))
				: $"Every {calling.Name} walks one path. Choose yours.";
			var choice = Choose($"CHOOSE YOUR PATH: {calling.Name.ToUpper()}", PlaceColor, player, rows,
				i => PathDetail(calling, calling.Paths[i]), prompt.Pastel(calling.Color), Summary(),
				"Choose", ref selected);
			return choice.Back ? null : calling.Paths[choice.Index];
		}

		private static List<string> CallingDetail(ClassBook.Calling c)
		{
			var d = new Detail("class " + c.Name);
			d.Line(c.Name.ToUpper(), c.Color);
			d.Line("Calling", Muted);
			string key = "class." + c.Name.ToLower();
			if (Story.Has(key)) d.Wrapped(Story.Line(key), Flavour);
			d.Blank();
			Numbers(d, c);
			d.Blank();
			d.Line("Paths: " + string.Join(", ", c.Paths), Dim);
			return d.Build();
		}

		private static List<string> PathDetail(ClassBook.Calling c, string path)
		{
			var d = new Detail("class " + path);
			d.Line(path.ToUpper(), c.Color);
			d.Line($"{c.Name} path", Muted);
			string key = "class." + path.ToLower();
			if (Story.Has(key)) d.Wrapped(Story.Line(key), Flavour);
			d.Blank();
			d.Line($"Starts like every {c.Name}:", Dim);
			Numbers(d, c);
			return d.Build();
		}

		/// <summary>Health, danger, gold and the starting kit of a calling.</summary>
		private static void Numbers(Detail d, ClassBook.Calling c)
		{
			d.Stat("Health", c.Health.ToString(), Good);
			// Danger rank makes every foe tougher (and better paid): see Danger.
			d.Stat("Danger rank", c.Danger == 0 ? "0" : $"{c.Danger} (foes {Danger.Percent(c.Danger)})", c.Danger == 0 ? Text : Bad);
			d.Stat("Gold", (StartingCoins + c.Gold).ToString(), Gold);
			foreach (var item in ClassBook.Kit(c.Name))
			{
				string what = item switch
				{
					WeaponData w => $"{w.Name} ({w.Damage} dmg)",
					_ => item.Amount > 1 ? $"{item.Name} x{item.Amount}" : item.Name,
				};
				d.Add(Stat(item is WeaponData ? "Weapon" : "Also", Fit(what, DetailInner - 12), RarityColor(item.Rarity)));
			}
		}

		private static string Summary() =>
			$" {"Danger rank".Pastel(Dim)} {$"makes foes {Danger.Percent(1)} tougher per rank, and pay {(int)(Danger.RewardPerRank * 100)}% more".Pastel(Muted)}";
	}
}
