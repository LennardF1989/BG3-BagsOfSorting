using System.IO;

namespace BG3.BagsOfSorting
{
    public static class Constants
    {
        public const string DEFAULT_TREASURETABLE_FOLDERNAME = "PouchOfWonders";
        public const string DEFAULT_TREASURETABLE_FOLDERNAME_STANDALONE = "BagsOfSorting";
        public const string DEFAULT_TREASURETABLE_NAME = "POW_TT";

        public const string BASE_ICON = "Item_LOOT_Bag_Blackpowder";
        public const string SMALL_BASE_ICON = $"SMALL_{BASE_ICON}.png";
        public const string LARGE_BASE_ICON = $"LARGE_{BASE_ICON}.png";

        public const int ICON_SIZE = 64;
        public const int TOOLTIP_SIZE = 380;

        public static readonly string CONTENT_PATH = Path.Combine("Content");
        public static readonly string CONTENT_STOCK_PATH = Path.Combine(CONTENT_PATH, "Stock");
        public static readonly string CONTENT_MODFIXER_PATH = Path.Combine(CONTENT_STOCK_PATH, "ModFixer");
        public static readonly string CONTENT_POW_PATH = Path.Combine(CONTENT_STOCK_PATH, "PouchOfWonders");
        public static readonly string CONTENT_CUSTOM_PATH = Path.Combine(CONTENT_PATH, "Custom");
        public static readonly string OUTPUT_PATH = Path.Combine("Output");
        public static readonly string ICONS_OUTPUT_PATH = Path.Combine(OUTPUT_PATH, "Icons");
        public static readonly string ConfigurationFile = Path.Combine(CONTENT_PATH, "Bags.json");
        public static readonly string SEARCH_INDEX_FILE = Path.Combine(OUTPUT_PATH, "SearchIndex.json");
    }
}
