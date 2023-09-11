using System.Text.Json.Serialization;

namespace BG3.BagsOfSorting.Models
{
    public class SearchIndex
    {
        public class Localization
        {
            public string ContentUUID { get; set; }
            public string Value { get; set; }
        }

        public class Tag
        {
            public string UUID { get; set; }
            public string Name { get; set; }
        }

        public class GameObject
        {
            public string MapKey { get; set; }
            public string Name { get; set; }
            public string ParentTemplateId { get; set; }
            public string VisualTemplate { get; set; }
            public string Icon { get; set; }
            public string Stats { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string TechnicalDescription { get; set; }
            public List<string> Tags { get; set; }
            public GameObjectReferences References { get; set; }

            [JsonIgnore]
            public string ResolvedName
            {
                get
                {
                    var displayName = References.DisplayName?.FirstOrDefault()?.Value;

                    return string.IsNullOrEmpty(displayName) 
                        ? Name 
                        : $"{displayName} ({Name})";
                }
            }

            public GameObject()
            {
                References = new GameObjectReferences();
            }
        }

        public class GameObjectReferences
        {
            public List<GameObject> ParentTemplateId { get; set; }
            public List<GameObject> VisualTemplate { get; set; }
            public List<Localization> DisplayName { get; set; }
            public List<Localization> Description { get; set; }
            public List<Localization> TechnicalDescription { get; set; }
            public List<Tag> Tags { get; set; }
        }

        public Dictionary<string, List<Localization>> Localizations { get; set; }
        public Dictionary<string, List<Tag>> Tags { get; set; }
        public Dictionary<string, List<GameObject>> GameObjects { get; set; }

        public Dictionary<string, List<GameObject>> LocalizationToGameObjects { get; set; }
        public Dictionary<string, List<GameObject>> TagToGameObjects { get; set; }

        public SearchIndex()
        {
            Localizations = new Dictionary<string, List<Localization>>();
            Tags = new Dictionary<string, List<Tag>>();
            GameObjects = new Dictionary<string, List<GameObject>>();

            LocalizationToGameObjects = new Dictionary<string, List<GameObject>>();
            TagToGameObjects = new Dictionary<string, List<GameObject>>();
        }
    }
}
