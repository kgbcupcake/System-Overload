namespace System_Overload.Src.GameData.Components
{
    public class LootTableData
    {
        /// <summary>
        /// The name of the item to drop, matching the 'Name' in the item's JSON file.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// The percentage chance (0-100) for this item to drop.
        /// </summary>
        public int DropChance { get; set; }

        /// <summary>
        /// The minimum quantity to drop if the roll is successful.
        /// </summary>
        public int MinQuantity { get; set; }

        /// <summary>
        /// The maximum quantity to drop if the roll is successful.
        /// </summary>
        public int MaxQuantity { get; set; }

        public LootTableData()
        {
            ItemName = string.Empty;
            DropChance = 100;
            MinQuantity = 1;
            MaxQuantity = 1;
        }
    }
}
