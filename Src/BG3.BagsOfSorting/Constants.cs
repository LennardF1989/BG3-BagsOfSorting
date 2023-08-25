using System.IO;

namespace BG3.BagsOfSorting
{
    public static class Constants
    {
        public const string BASE_ICON = "Item_LOOT_Bag_Blackpowder";

        public static readonly string CONTENT_PATH = Path.Combine("Content");
        public static readonly string CONTENT_CUSTOM_PATH = Path.Combine(Constants.CONTENT_PATH, "Custom");
        public static readonly string ICONS_OUTPUT_PATH = Path.Combine("Output", "Icons");
        public static readonly string BAG_CONFIGURATION_FILE = Path.Combine(CONTENT_PATH, "Bags.json");
    }
}
