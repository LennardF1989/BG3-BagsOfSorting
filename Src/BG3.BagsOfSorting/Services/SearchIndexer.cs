#pragma warning disable S1199

using System.IO;
using BG3.BagsOfSorting.Extensions;
using BG3.BagsOfSorting.Models;
using LSLib.LS;

namespace BG3.BagsOfSorting.Services
{
    public static class SearchIndexer
    {
        private sealed class GameObjectEqualityComparer : IEqualityComparer<SearchIndex.GameObject>
        {
            public bool Equals(SearchIndex.GameObject x, SearchIndex.GameObject y)
            {
                if (x == y)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return x.MapKey == y.MapKey &&
                       x.Name == y.Name &&
                       x.ParentTemplateId == y.ParentTemplateId &&
                       x.VisualTemplate == y.VisualTemplate &&
                       x.Icon == y.Icon &&
                       x.Stats == y.Stats &&
                       x.DisplayName == y.DisplayName &&
                       x.Description == y.Description &&
                       x.TechnicalDescription == y.TechnicalDescription &&
                       (
                           (x.Tags is null && y.Tags is null) ||
                           x.Tags is not null && y.Tags is not null && x.Tags.SequenceEqual(y.Tags)
                       );
            }

            public int GetHashCode(SearchIndex.GameObject obj)
            {
                return obj.GetHashCode();
            }
        }

        [Flags]
        public enum SearchIndexFilter
        {
            LocalizationHandle = 1,
            LocalizationValue = 2,
            TagUUID = 4,
            TagName = 8,
            GameObjectMapKey = 16,
            GameObjectAttributes = 32,
            All = LocalizationHandle +
                  LocalizationValue +
                  TagUUID +
                  TagName +
                  GameObjectMapKey +
                  GameObjectAttributes
        }

        private static readonly GameObjectEqualityComparer _gameObjectEqualityComparer = new();

        public static HashSet<SearchIndex.GameObject> Search(SearchIndex searchIndex, string query, SearchIndexFilter filter = SearchIndexFilter.All)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new HashSet<SearchIndex.GameObject>();
            }

            var gameObjects = new HashSet<SearchIndex.GameObject>();

            //Localization
            {
                var localizationHandle = filter.HasFlag(SearchIndexFilter.LocalizationHandle);
                var localizationValue = filter.HasFlag(SearchIndexFilter.LocalizationValue);

                if (localizationHandle || localizationValue)
                {
                    var localizations = searchIndex.Localizations.Values
                        .Where(x => x.Exists(
                            y =>
                                (localizationHandle && y.ContentUUID.Contains(query, StringComparison.InvariantCultureIgnoreCase)) ||
                                (localizationValue && y.Value.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                        ))
                        .SelectMany(x => x)
                        .Distinct();

                    foreach (var localization in localizations)
                    {
                        searchIndex.LocalizationToGameObjects
                            .Get(localization.ContentUUID)
                            ?.ForEach(x => gameObjects.Add(x));
                    }
                }
            }

            //Tags
            {
                var tagUUID = filter.HasFlag(SearchIndexFilter.TagUUID);
                var tagName = filter.HasFlag(SearchIndexFilter.TagName);

                if (tagUUID || tagName)
                {
                    var tags = searchIndex.Tags.Values
                        .Where(x => x.Exists(
                            y =>
                                (tagUUID && y.UUID.Contains(query, StringComparison.InvariantCultureIgnoreCase)) ||
                                (tagName && y.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                        ))
                        .SelectMany(x => x)
                        .Distinct();

                    foreach (var tag in tags)
                    {
                        searchIndex.TagToGameObjects
                            .Get(tag.UUID)
                            ?.ForEach(x => gameObjects.Add(x));
                    }
                }
            }

            //GameObject
            {
                var gameObjectMapKey = filter.HasFlag(SearchIndexFilter.GameObjectMapKey);
                var gameObjectAttributes = filter.HasFlag(SearchIndexFilter.GameObjectAttributes);

                if (gameObjectMapKey || gameObjectAttributes)
                {
                    var foundGameObjects = searchIndex.GameObjects.Values
                        .Where(x => x.Exists(
                            y =>
                                (gameObjectMapKey && y.MapKey.Contains(query, StringComparison.InvariantCultureIgnoreCase)) ||
                                (gameObjectAttributes && (
                                    y.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
                                    (y.ParentTemplateId != null && y.ParentTemplateId.Contains(query, StringComparison.InvariantCultureIgnoreCase)) ||
                                    (y.VisualTemplate != null && y.VisualTemplate.Contains(query, StringComparison.InvariantCultureIgnoreCase)) ||
                                    (y.Icon != null && y.Icon.Contains(query, StringComparison.InvariantCultureIgnoreCase)) ||
                                    (y.Stats != null && y.Stats.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                                ))
                        ))
                        .SelectMany(x => x)
                        .Distinct();

                    foreach (var gameObject in foundGameObjects)
                    {
                        searchIndex.GameObjects
                            .Get(gameObject.MapKey)
                            ?.ForEach(x => gameObjects.Add(x));
                    }
                }
            }

            return gameObjects;
        }

        public static SearchIndex Index(Context context)
        {
            var searchIndex = new SearchIndex();

            if (context.Configuration.PAKPaths == null || !context.Configuration.PAKPaths.Any())
            {
                context.LogMessage("[Warning] No paths containing PAKs have been configured.");

                return searchIndex;
            }

            foreach (var path in context.Configuration.PAKPaths)
            {
                IndexPath(searchIndex, path);
            }

            PopulateReverseLookup(searchIndex);

            return searchIndex;
        }

        private static void IndexPath(SearchIndex searchIndex, string path)
        {
            var files = Directory.GetFiles(path, "*.pak", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                using var packageReader = new PackageReader(file);
                Package package;

                try
                {
                    package = packageReader.Read();
                }
                catch
                {
                    continue;
                }

                IndexLocalization(searchIndex, package);
                IndexTags(searchIndex, package);
                IndexGameObjects(searchIndex, package);
            }
        }

        private static void IndexLocalization(SearchIndex searchIndex, Package package)
        {
            var localizationFiles = package.Files
                .Where(x => x.Name.EndsWith(".loca"));

            foreach (var abstractFileInfo in localizationFiles)
            {
                using var stream = abstractFileInfo.MakeStream();

                using var locaReader = new LocaReader(stream);
                LocaResource resource;

                try
                {
                    resource = locaReader.Read();
                }
                catch
                {
                    continue;
                }

                foreach (var entry in resource.Entries)
                {
                    searchIndex.Localizations.GetAndAdd(entry.Key, new SearchIndex.Localization
                    {
                        ContentUUID = entry.Key,
                        Value = entry.Text
                    });
                }
            }
        }

        private static void IndexTags(SearchIndex searchIndex, Package package)
        {
            var tagFiles = package.Files
                .Where(x => x.Name.Contains("/Tags/"));

            foreach (var abstractFileInfo in tagFiles)
            {
                var resource = abstractFileInfo.ReadResource();

                var result = resource?.Regions
                    ?.Get("Tags");

                if (result == null)
                {
                    continue;
                }

                var uuid = result.Attributes.Get("UUID")?.Value.ToString().StringOrNull();
                var name = result.Attributes.Get("Name")?.Value.ToString().StringOrNull();

                if (uuid == null || name == null)
                {
                    continue;
                }

                searchIndex.Tags.GetAndAdd(uuid, new SearchIndex.Tag
                {
                    UUID = uuid,
                    Name = name
                });
            }
        }

        private static void IndexGameObjects(SearchIndex searchIndex, Package package)
        {
            var rootTemplateFiles = package.Files
                .Where(x => x.Name.Contains("/RootTemplates/") || x.Name.Contains("/Items/"));

            foreach (var abstractFileInfo in rootTemplateFiles)
            {
                using var stream = abstractFileInfo.MakeStream();

                var resource = abstractFileInfo.ReadResource();

                var gameObjects = resource?.Regions
                    ?.Get("Templates")?.Children
                    ?.Get("GameObjects")
                    ?.ToList();

                if (gameObjects == null)
                {
                    continue;
                }

                foreach (var result in gameObjects)
                {
                    var type = result.Attributes.Get("Type")?.Value.ToString();

                    if (type != "item")
                    {
                        continue;
                    }

                    var mapKey = result.Attributes.Get("MapKey")?.Value.ToString().StringOrNull();
                    var name = result.Attributes.Get("Name")?.Value.ToString().StringOrNull();

                    if (mapKey == null || name == null)
                    {
                        continue;
                    }

                    var parentTemplateId = result.Attributes.Get("ParentTemplateId")?.Value.ToString().StringOrNull();
                    var visualTemplate = result.Attributes.Get("VisualTemplate")?.Value.ToString().StringOrNull();
                    var icon = result.Attributes.Get("Icon")?.Value.ToString().StringOrNull();
                    var stats = result.Attributes.Get("Stats")?.Value.ToString().StringOrNull();
                    var displayName = (result.Attributes.Get("DisplayName")?.Value as TranslatedString)?.Handle.StringOrNull();
                    var description = (result.Attributes.Get("Description")?.Value as TranslatedString)?.Handle.StringOrNull();
                    var technicalDescription = (result.Attributes.Get("TechnicalDescription")?.Value as TranslatedString)?.Handle.StringOrNull();

                    var gameObjectTags = result.Children
                        .Get("Tags")
                        ?.SingleOrDefault()
                        ?.Children.Get("Tag")
                        ?.Select(y => y.Attributes.Get("Object")?.Value.ToString().StringOrNull())
                        .Where(x => x is not null)
                        .ToList();

                    var existingGameObjects = searchIndex.GameObjects.Get(mapKey);

                    var newGameObject = new SearchIndex.GameObject
                    {
                        MapKey = mapKey,
                        Name = name,
                        ParentTemplateId = parentTemplateId,
                        VisualTemplate = visualTemplate,
                        Icon = icon,
                        Stats = stats,
                        DisplayName = displayName,
                        Description = description,
                        TechnicalDescription = technicalDescription,
                        Tags = gameObjectTags != null && gameObjectTags.Any()
                            ? gameObjectTags
                            : null
                    };

                    if (existingGameObjects == null)
                    {
                        searchIndex.GameObjects.GetAndAdd(mapKey, newGameObject);
                    }
                    else if (!existingGameObjects.Contains(newGameObject, _gameObjectEqualityComparer))
                    {
                        existingGameObjects.Add(newGameObject);
                    }
                }
            }
        }

        private static void PopulateReverseLookup(SearchIndex searchIndex)
        {
            foreach (var gameObject in searchIndex.GameObjects.Values.SelectMany(x => x))
            {
                if (gameObject.ParentTemplateId != null)
                {
                    var parentTemplateId = searchIndex.GameObjects.Get(gameObject.ParentTemplateId);

                    if (parentTemplateId != null)
                    {
                        gameObject.References.ParentTemplateId = parentTemplateId;
                    }
                }

                if (gameObject.VisualTemplate != null)
                {
                    var visualTemplate = searchIndex.GameObjects.Get(gameObject.VisualTemplate);

                    if (visualTemplate != null)
                    {
                        gameObject.References.VisualTemplate = visualTemplate;
                    }
                }

                if (gameObject.DisplayName != null)
                {
                    searchIndex.LocalizationToGameObjects.GetAndAdd(gameObject.DisplayName, gameObject);

                    gameObject.References.DisplayName = searchIndex.Localizations.Get(gameObject.DisplayName);
                }

                if (gameObject.Description != null)
                {
                    searchIndex.LocalizationToGameObjects.GetAndAdd(gameObject.Description, gameObject);

                    gameObject.References.Description = searchIndex.Localizations.Get(gameObject.Description);
                }

                if (gameObject.TechnicalDescription != null)
                {
                    searchIndex.LocalizationToGameObjects.GetAndAdd(gameObject.TechnicalDescription, gameObject);

                    gameObject.References.TechnicalDescription = searchIndex.Localizations.Get(gameObject.TechnicalDescription);
                }

                if (gameObject.Tags != null)
                {
                    foreach (var tag in gameObject.Tags)
                    {
                        searchIndex.TagToGameObjects.GetAndAdd(tag, gameObject);

                        var foundTags = searchIndex.Tags.Get(tag);

                        if (foundTags != null)
                        {
                            gameObject.References.Tags = foundTags;
                        }
                    }
                }
            }
        }
    }
}