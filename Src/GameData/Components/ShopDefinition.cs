using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System_Overload.Src.GameData.Components
{
	[MariesGui.Contracts.MariesGuiContent("shops")]
	public class ShopDefinition
	{
		public string StoreName { get; set; } = "";
		public List<string> InventoryRefs { get; set; } = new List<string>();
	}
}
