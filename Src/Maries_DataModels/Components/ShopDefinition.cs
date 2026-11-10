using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maries_DataModels
{
	[MariesGui.Contracts.MariesGuiContent("shops")]
	public class ShopDefinition
	{
		public string StoreName { get; set; } = "";
		[MariesGui.Contracts.MariesGuiRef("Name", "items", "weapons", "armor", "gems")]
		public List<string> InventoryRefs { get; set; } = new List<string>();
	}
}
