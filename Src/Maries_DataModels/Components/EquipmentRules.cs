using System.Text.Json.Serialization;

namespace Maries_DataModels
{
	/// <summary>Stats a socketed gem can raise.</summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SocketStat { Damage, Health, Defense, CritChance, Accuracy }

	/// <summary>A slot gear is worn in, e.g. Head or Chest.</summary>
	public class EquipSlotRule
	{
		public string Id { get; set; } = "";
		public string Label { get; set; } = "";
	}

	/// <summary>A kind of item that can hold gems, and which item Types count as it.</summary>
	public class SocketHostRule
	{
		public string Id { get; set; } = "";
		public string Label { get; set; } = "";
		public int MaxSockets { get; set; } = 0;
		public List<string> ItemTypes { get; set; } = new();
	}

	/// <summary>A gem attribute's bonus in one kind of host: Amount = gem Power x PerPower.</summary>
	public class GemEffectRule
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public EffectType Attribute { get; set; }
		/// <summary>A SocketHostRule Id, or "*" for any host.</summary>
		[MariesGui.Contracts.MariesGuiRef("SocketHosts.Id", "equipment", AlsoAllowed = "*")]
		public string Host { get; set; } = "*";
		public SocketStat Stat { get; set; }
		public float PerPower { get; set; } = 1f;
	}

	/// <summary>A flat bonus written on one gem, overriding the attribute rules for that host.</summary>
	public class GemModifier
	{
		/// <summary>A SocketHostRule Id, or "*" for any host.</summary>
		[MariesGui.Contracts.MariesGuiRef("SocketHosts.Id", "equipment", AlsoAllowed = "*")]
		public string Host { get; set; } = "*";
		public SocketStat Stat { get; set; }
		public float Amount { get; set; }
	}

	/// <summary>
	/// Everything about gear slots and gem sockets, loaded from the "equipment" content folder.
	/// </summary>
	[MariesGui.Contracts.MariesGuiContent("equipment")]
	public class EquipmentRules
	{
		public List<EquipSlotRule> EquipSlots { get; set; } = new();
		public List<SocketHostRule> SocketHosts { get; set; } = new();
		public List<GemEffectRule> GemEffects { get; set; } = new();

		public int SetGemCost { get; set; } = 0;
		public int RemoveGemCost { get; set; } = 0;
		public bool RemoveGemDestroysGem { get; set; } = false;
		public int PunchBaseCost { get; set; } = 0;
		public int PunchCostPerSocket { get; set; } = 0;

		/// <summary>The rules in use. Set by the loader at startup; empty (no sockets) until then.</summary>
		[JsonIgnore]
		public static EquipmentRules Current { get; set; } = new();

		/// <summary>The socket host an item counts as (by its Type), or null if it can't hold gems.</summary>
		public SocketHostRule? HostFor(ItemData item) =>
			item is GemData ? null :
			SocketHosts.FirstOrDefault(h => h.ItemTypes.Any(t => t.Equals(item.Type, StringComparison.OrdinalIgnoreCase)));

		public bool CanHoldGems(ItemData item) => HostFor(item) is { MaxSockets: > 0 };

		/// <summary>Sockets an item really has: its own count, capped by its host's maximum.</summary>
		public int SocketsOn(ItemData item) => HostFor(item) is { } host ? Math.Min(item.NumSockets, host.MaxSockets) : 0;

		public bool CanPunchSocket(ItemData item) => HostFor(item) is { } host && item.NumSockets < host.MaxSockets;

		public int PunchCost(ItemData item) => PunchBaseCost + PunchCostPerSocket * item.NumSockets;

		public string SlotLabel(string slotId) =>
			EquipSlots.FirstOrDefault(s => s.Id.Equals(slotId, StringComparison.OrdinalIgnoreCase))?.Label ?? slotId;

		/// <summary>What one gem gives while set in an item of the given host.</summary>
		public IEnumerable<(SocketStat Stat, float Amount)> GemBonuses(GemData gem, string hostId)
		{
			static bool Matches(string ruleHost, string host) =>
				ruleHost == "*" || ruleHost.Equals(host, StringComparison.OrdinalIgnoreCase);

			var own = gem.Modifiers.Where(m => Matches(m.Host, hostId)).ToList();
			if (own.Count > 0)
				return own.Select(m => (m.Stat, m.Amount));

			return gem.Attributes
				.SelectMany(a => GemEffects.Where(e => e.Attribute == a && Matches(e.Host, hostId)))
				.Select(e => (e.Stat, gem.Power * e.PerPower));
		}

		/// <summary>Bonuses from every gem set in an item, summed by stat.</summary>
		public Dictionary<SocketStat, float> ItemBonuses(ItemData item)
		{
			var totals = new Dictionary<SocketStat, float>();
			if (HostFor(item) is not { } host) return totals;

			foreach (var gem in item.SocketedGems.Take(SocketsOn(item)))
			{
				foreach (var (stat, amount) in GemBonuses(gem, host.Id))
					totals[stat] = totals.GetValueOrDefault(stat) + amount;
			}
			return totals;
		}

		/// <summary>"+4 Damage, +2 Defense", for showing a bonus list in the UI.</summary>
		public static string Describe(IEnumerable<(SocketStat Stat, float Amount)> bonuses)
		{
			var parts = bonuses
				.GroupBy(b => b.Stat)
				.Select(g => (Stat: g.Key, Amount: g.Sum(b => b.Amount)))
				.Where(b => b.Amount != 0)
				.Select(b => $"{(b.Amount > 0 ? "+" : "")}{b.Amount:0.#}{(b.Stat is SocketStat.CritChance or SocketStat.Accuracy ? "%" : "")} {StatLabel(b.Stat)}");
			string text = string.Join(", ", parts);
			return text.Length > 0 ? text : "No effect";
		}

		public static string StatLabel(SocketStat stat) => stat switch
		{
			SocketStat.Health => "Max HP",
			SocketStat.CritChance => "Crit",
			_ => stat.ToString(),
		};
	}
}
