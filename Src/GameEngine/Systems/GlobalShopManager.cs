using System_Overload.Src.GameEngine;
using Src.GameData.Components;

namespace Src.GameEngine.Systems
{
    public static class GlobalShopManager
    {
        public static List<ItemData> Items { get; set; } = new List<ItemData>();

        public static void LoadAllFromMaster()
        {
            Items = LoadGame.LoadAllFromFolder<ItemData>("shop_stock");
        }
    }
}