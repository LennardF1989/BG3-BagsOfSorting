using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BG3.BagsOfSorting
{
    public class BagConfiguration
    {
        public class Icon
        {
            public string Name { get; set; }
            public bool Custom { get; set; }
            public bool Generate { get; set; }
        }

        public class Bag
        {
            public Guid MapKey { get; set; }

            [JsonIgnore]
            public string Name { get; set; }

            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string TechnicalDescription { get; set; }
            public string AutoPickupCondition { get; set; }
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public EColor Color { get; set; }
            public Icon ItemIcon { get; set; }
            public Icon TooltipIcon { get; set; }
            public int? Amount { get; set; }
        }

        public enum EColor
        {
            None = 0, //Common
            Green, //Uncommon
            Blue, //Rare
            Pink, //VeryRare
            Gold, //Legendary
            Orange, //StoryItem = True
        }

        public List<Bag> Bags { get; set; }
        public JsonObject AdditionalTreasures { get; set; }
        public bool AlignGeneratedItemIconsRight { get; set; }
    }
}
