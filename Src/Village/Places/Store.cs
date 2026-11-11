using DungeonAdventures.Src.GameData.Components;
using DungeonAdventures.Src.GameEngine.Combat;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using Maries_DataModels;
using static Maries_DataModels.PlayerData;
using static System.Console;
using static DungeonAdventures.Src.Village.Places.ItemPanels;
using static DungeonAdventures.Src.Village.Places.PanelText;
using static DungeonAdventures.Src.Village.Places.ItemInfo;
using static DungeonAdventures.Src.Village.Places.ItemDetails;
using static DungeonAdventures.Src.Village.Places.PanelMenu;

namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>
	/// The General Store, in the inventory's two-panel layout: a BUY tab (healing potions and the
	/// Dark Pact) and a SELL tab (everything in the pack), with the selected item's details on the right.
	/// </summary>
	public class Store
	{
		private const string Place = "GENERAL STORE";
		private const string PlaceColor = "#2E0E4E";
		private const string PactColor = "#B06CFF";
		private static readonly string[] Tabs = { "Buy", "Sell" };

		// 1. The Entry Point
		public static void LoadShop(string shopFile = "GeneralStore.json")
		{
			var p = GameState.CurrentPlayer;

			// This runs UniversalLoader and fills AllItems, AllWeapons, etc.
			if (ItemRegistry.AllItems.Count == 0)
				ItemRegistry.Initialize();

			TimeSpan elapsed = DateTime.Now - p.LastRestockTime;
			if (elapsed.TotalMinutes >= 30)
			{
				p.StorePotionStock = 5;
				p.LastRestockTime = DateTime.Now;
			}

			UiEngine.DrawLoadingScreen(Story.Pick("store.enter"), 1000);
			UpdateAndRun(p);
		}

		// 2. The Main Loop
		private static void UpdateAndRun(loadPlayer p)
		{
			int tab = 0, buySelected = 0, sellSelected = 0;
			string message = Story.Pick("store.greeting").Pastel("#888888");

			while (true)
			{
				int potionPrice = 20 + (10 * p.StorePriceModifier);
				int pactPrice = 300 + (100 * p.DifficultyRating);
				float sellMultiplier = 1.0f + (p.DifficultyRating * 0.15f);

				// Gear in hand or being worn can't be sold. With several copies, a spare one is sold instead;
				// Item is that spare, or the equipped piece itself when there's no spare (Locked).
				var weapon = p.GetEquippedWeapon();
				bool InUse(ItemData i) => ReferenceEquals(i, weapon) || i is ArmorData { IsEquipped: true };
				var sellable = p.Inventory
					.GroupBy(i => i.Name)
					.Select(g =>
					{
						var spare = g.FirstOrDefault(i => !InUse(i));
						return (Name: g.Key, Count: g.Count(), Item: spare ?? g.First(), Locked: spare == null,
							Equipped: g.Any(InUse), Price: (int)(BaseSellPrice(g.First().Rarity) * sellMultiplier));
					})
					.ToList();

				string summary =
					$" {"Gold".Pastel(Dim)} {p.Coins.ToString().Pastel(Gold)}" +
					$"   {"Potions in stock".Pastel(Dim)} {p.StorePotionStock}" +
					$"   {"Dark Pacts".Pastel(Dim)} {p.DifficultyRating.ToString().Pastel(PactColor)}" +
					$"   {"Pack".Pastel(Dim)} {p.Inventory.Count}/{p.InventorySize}";

				PanelMenu.Choice choice;
				if (tab == 0)
				{
					var potion = ItemRegistry.GetItem("Healing Potion");
					var rows = new List<PanelRow>
					{
						new("Healing Potion", RarityColor(ItemRarity.Common),
							Right: p.StorePotionStock > 0 ? $"{potionPrice}g" : "sold out",
							RightColor: p.StorePotionStock == 0 ? Muted : potionPrice > p.Coins ? Bad : Gold),
						new("Dark Pact", PactColor, Right: $"{pactPrice}g", RightColor: pactPrice > p.Coins ? Bad : Gold),
					};

					choice = Choose(Place, PlaceColor, p, rows,
						i => i == 0 ? PotionDetail(p, potion, potionPrice) : PactDetail(p, pactPrice),
						message, summary, "Buy", ref buySelected, tabs: Tabs, activeTab: tab);
				}
				else
				{
					var rows = sellable.Select(s => new PanelRow(s.Name, RarityColor(s.Item.Rarity),
						Right: s.Locked ? "in use" : s.Count > 1 ? $"x{s.Count}  {s.Price}g" : $"{s.Price}g",
						RightColor: s.Locked ? Muted : Good,
						Tag: s.Equipped ? "[E] " : "")).ToList();

					choice = Choose(Place, PlaceColor, p, rows,
						i => ItemDetail(p, sellable[i].Item, equipped: sellable[i].Locked ? true : null,
							quantity: sellable[i].Count, extra: new[]
						{
							sellable[i].Locked
								? (sellable[i].Item is WeaponData ? "Equip another weapon to sell it." : "Take it off to sell it.").Pastel(Bad)
								: Stat("Sells for", $"{sellable[i].Price} gold", Good),
						}, showValue: false).Build(),
						message, summary, "Sell one", ref sellSelected, emptyText: "Nothing to sell.", tabs: Tabs, activeTab: tab);
				}

				if (choice.Back)
				{
					WriteAt(ListX, MessageRow, Pad(" " + Story.Pick("store.farewell").Pastel(Amber), ScreenWidth - 2));
					Thread.Sleep(500);
					Clear();
					return;
				}
				if (choice.Tab >= 0)
				{
					tab = choice.Tab;
					message = "";
					continue;
				}

				if (tab == 0 && choice.Index == 0)
				{
					if (p.StorePotionStock <= 0)
						message = "SOLD OUT: Come back later for more potions.".Pastel(Bad);
					else
					{
						message = TryBuyReturnStatus("Healing Potion", potionPrice, p);
						if (message.Contains("SUCCESS"))
						{
							p.StorePotionStock--;
							p.StorePriceModifier++;
							GameState.Sync();
						}
					}
				}
				else if (tab == 0)
				{
					message = TryBuyReturnStatus("Difficulty", pactPrice, p);
					if (message.Contains("SUCCESS")) GameState.Sync();
				}
				else
				{
					var target = sellable[choice.Index];
					if (target.Locked)
					{
						message = (target.Item is WeaponData
							? $"That's the {target.Name} in your hand. Equip another weapon in your inventory first."
							: $"You're wearing the {target.Name}. Take it off in your inventory first.").Pastel(Bad);
						continue;
					}
					p.Coins += target.Price;
					p.Inventory.Remove(target.Item);
					message = $"SOLD: {target.Name} (+{target.Price} gold)".Pastel("#00FF00");
					GameState.Sync();
				}
			}
		}

		private static List<string> PotionDetail(loadPlayer p, ItemData? potion, int price)
		{
			var extra = new[]
			{
				Stat("Price", $"{price} gold", price > p.Coins ? Bad : Gold),
				Stat("In stock", p.StorePotionStock.ToString(), p.StorePotionStock > 0 ? Text : Bad),
				"Price goes up 10 gold with each one sold.".Pastel(Muted),
			};
			if (potion != null)
				return ItemDetail(p, potion, extra: extra, showValue: false).Build();

			var d = new Detail("Healing Potion");
			d.Line("HEALING POTION", Text).Blank();
			foreach (var line in extra) d.Add(line);
			return d.Build();
		}

		private static List<string> PactDetail(loadPlayer p, int price)
		{
			var d = new Detail("Dark Pact");
			d.Line("DARK PACT", PactColor).Line("A bargain in the dark", Muted).Blank();
			d.Wrapped("Seal it and every foe grows stronger, but they pay better, and so does the shop.", Flavour).Blank();
			int r = p.DifficultyRating;
			d.Stat("Danger rank", $"{r} → {r + 1}", PactColor);
			d.Stat("Foes", $"{Danger.Percent(r)} → {Danger.Percent(r + 1)} health, attack", Bad);
			d.Stat("Foe rewards", $"+{r * 10}% → +{(r + 1) * 10}% XP, gold", Good);
			d.Stat("Sell prices", $"+{r * 15}% → +{(r + 1) * 15}%", Good);
			d.Stat("Price", $"{price} gold", price > p.Coins ? Bad : Gold);
			return d.Build();
		}

		private static int BaseSellPrice(ItemRarity rarity) => rarity switch
		{
			ItemRarity.Common => 15,
			ItemRarity.Uncommon => 45,
			ItemRarity.Rare => 120,
			ItemRarity.Epic => 300,
			ItemRarity.Legendary => 750,
			_ => 10
		};

		private static string TryBuyReturnStatus(string item, int cost, loadPlayer p)
		{
			if (p.Coins >= cost)
			{
				if (item == "Difficulty")
				{
					p.Coins -= cost;
					p.DifficultyRating++;
					return "SUCCESS: The pact is sealed. Your foes grow stronger.".Pastel("#00FF00");
				}

				if (item == "Healing Potion")
				{
					var template = ItemRegistry.GetItem("Healing Potion");

					if (template != null)
					{
						p.Coins -= cost;
						p.Inventory.Add(new ItemData(template));
						return "SUCCESS: Healing Potion bought.".Pastel("#00FF00");
					}

					// If it hits here, the NAME is right, but the Registry is empty or the file wasn't loaded
					return $"ERROR: [{item}] is missing from the item data.".Pastel("#FF0000");
				}
			}
			return "DENIED: Not enough gold.".Pastel("#FF0000");
		}
	}
}
