using System_Overload.Src.GameEngine.Systems.Loaders;
using Src.GameData.Components;

namespace Src.GameEngine.Systems
{
    public static class GlobalShopManager
    {
        public static List<ItemData> Items { get; set; } = new List<ItemData>();

        public static void LoadAllFromMaster()
        {
            Items = UniversalLoader.LoadCollection<ItemData>("shop_stock");
        }
    }
}