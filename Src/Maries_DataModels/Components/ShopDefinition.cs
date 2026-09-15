using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maries_DataModels
{
	public class ShopDefinition
	{
		public string StoreName { get; set; } = "";
		public List<string> InventoryRefs { get; set; } = new List<string>();
	}
}
