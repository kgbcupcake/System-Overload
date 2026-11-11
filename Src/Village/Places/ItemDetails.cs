using DungeonAdventures.Src.GameData.Components;
using Maries_DataModels;
using Maries_Loaders;
using Maries_Ui;
using Pastel;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;
using static DungeonAdventures.Src.Village.Places.ItemPanels;
using static DungeonAdventures.Src.Village.Places.PanelText;
using static DungeonAdventures.Src.Village.Places.ItemInfo;

namespace DungeonAdventures.Src.Village.Places
{
	/// <summary>The right-hand detail panel: a line builder with the item's icon, and the full write-up of an item.</summary>
	internal static class ItemDetails
	{
		/// <summary>
		/// Builds the lines of a detail panel. With an icon, the first rows are kept narrow so text
		/// wraps beside it; <see cref="Build"/> places the icon in the top-right corner.
		/// </summary>
		public sealed class Detail
		{
			private const int IconGap = 2;
			private readonly List<string> _lines = new();
			private readonly string[]? _icon;
			private readonly int _narrow;

			public Detail(string? iconFor = null)
			{
				_icon = iconFor != null ? PixelIcon.ForItem(iconFor) : null;
				_narrow = _icon != null ? DetailInner - PixelIcon.Width(_icon) - IconGap : DetailInner;
			}

			public bool HasIcon => _icon != null;
			public int Count => _lines.Count;

			/// <summary>Width available to the next line.</summary>
			public int Width => _lines.Count < (_icon?.Length ?? 0) ? _narrow : DetailInner;

			public Detail Add(string line) { _lines.Add(line); return this; }
			public Detail Blank() => Add("");
			public Detail Stat(string label, string value, string color) => Add(PanelText.Stat(label, value, color));

			/// <summary>Plain text cut to the current width, then coloured.</summary>
			public Detail Line(string text, string color) => Add(Fit(text, Width).Pastel(color));

			/// <summary>Word-wrapped text, narrower while beside the icon.</summary>
			public Detail Wrapped(string text, string color)
			{
				var words = new Queue<string>(text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
				while (words.Count > 0)
					_lines.Add(TakeLine(words, Width).Pastel(color));
				return this;
			}

			public List<string> Build()
			{
				var lines = new List<string>(_lines);
				int iconRows = _icon?.Length ?? 0;
				for (int r = 0; r < iconRows; r++)
				{
					// Where the icon has no pixels (its transparent margin) the line can use the full width.
					if (r < lines.Count && UiEngine.StripAnsi(_icon![r]).Trim().Length == 0) continue;
					// A few stat lines can still be a little too long for the narrow column: cut those.
					string text = r < lines.Count ? ClipVisible(lines[r], _narrow) : "";
					string row = Pad(text, _narrow) + new string(' ', IconGap) + _icon![r];
					if (r < lines.Count) lines[r] = row; else lines.Add(row);
				}
				return lines.Take(VisibleRows).ToList();
			}
		}

		/// <summary>
		/// Full details of an item with its icon. <paramref name="equipped"/> null leaves out the
		/// equipped/worn line (shop stock); <paramref name="extra"/> lines go after the stats, before
		/// weight and value.
		/// </summary>
		public static Detail ItemDetail(PlayerData.loadPlayer player, ItemData item, bool? equipped = null,
			int quantity = 1, IEnumerable<string>? extra = null, bool showValue = true)
		{
			var d = new Detail(item.Name);
			d.Line(item.Name.ToUpper(), RarityColor(item.Rarity));
			d.Line($"{item.Rarity} {TypeLabel(item)}", Muted);

			string descKey = "item." + Slug(item.Name);
			if (Story.Has(descKey))
			{
				// The narrow column beside an icon wraps to more lines, so skip this spacer to keep
				// the stats at the bottom inside the panel.
				if (!d.HasIcon) d.Blank();
				d.Wrapped(Story.Line(descKey), Flavour);
			}
			d.Blank();

			var rules = EquipmentRules.Current;
			switch (item)
			{
				case WeaponData w:
					int min = w.MinDamage > 0 ? w.MinDamage : (int)Math.Round(w.Damage * 0.8);
					int max = w.MaxDamage > 0 ? w.MaxDamage : (int)Math.Round(w.Damage * 1.2);
					d.Stat("Damage", $"{min}-{max}", Bad);
					d.Stat("Crit chance", $"{w.CritChance}%", Amber);
					if (w.Effect != EffectType.None)
						d.Stat(IsElement(w.Effect) ? "Element" : "Effect", w.Effect.ToString(), "#87CEEB");
					if (w.LevelReq > 0) d.Stat("Level req", w.LevelReq.ToString(), w.LevelReq > player.Level ? Bad : Text);
					AddSocketLines(d, w);
					if (equipped is { } we) d.Stat("Status", we ? "EQUIPPED" : "In pack", we ? Good : Muted);
					break;

				case ArmorData a:
					d.Stat("Slot", rules.SlotLabel(a.Slot), Text);
					d.Stat("Defense", $"+{a.Defense}", "#87CEEB");
					if (a.BonusHealth != 0) d.Stat("Max HP", $"+{a.BonusHealth}", Good);
					if (a.LevelReq > 0) d.Stat("Level req", a.LevelReq.ToString(), a.LevelReq > player.Level ? Bad : Text);
					AddSocketLines(d, a);
					if (equipped is { } ae) d.Stat("Status", ae ? "WORN" : "In pack", ae ? Good : Muted);
					break;

				case GemData g:
					d.Stat("Power", $"+{g.Power:0.#}", GemColor);
					if (g.Attributes.Count > 0) d.Stat("Element", string.Join(", ", g.Attributes), "#87CEEB");
					d.Add("When set in:".Pastel(Dim));
					foreach (var host in rules.SocketHosts)
						d.Line($"  {host.Label}: {EquipmentRules.Describe(rules.GemBonuses(g, host.Id))}", Good);
					d.Add("Only counts in equipped gear.".Pastel(Muted));
					break;

				default:
					if (IsPotion(item)) d.Stat("Effect", PotionEffect(item), Good);
					if (item.IsQuestItem) d.Stat("Quest item", "Can't drop", Amber);
					break;
			}

			if (extra != null)
				foreach (var line in extra) d.Add(line);

			if (quantity > 1) d.Stat("Carrying", quantity.ToString(), Text);
			d.Stat("Weight", $"{item.Weight:0.#}", Text);
			if (showValue) d.Stat("Value", $"{item.Value} gold", Gold);
			return d;
		}

		/// <summary>Socket count and each set gem with what it gives in this item.</summary>
		private static void AddSocketLines(Detail d, ItemData item)
		{
			var rules = EquipmentRules.Current;
			int sockets = rules.SocketsOn(item);
			if (sockets <= 0) return;

			string hostId = rules.HostFor(item)!.Id;
			d.Stat("Sockets", $"{item.SocketedGems.Count}/{sockets}", GemColor);
			foreach (var gem in item.SocketedGems.Take(sockets))
				d.Line($"  ◆ {gem.Name}: {EquipmentRules.Describe(rules.GemBonuses(gem, hostId))}", GemColor);
		}
	}
}
