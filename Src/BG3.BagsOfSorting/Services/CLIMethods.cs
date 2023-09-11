#pragma warning disable S101

using BG3.BagsOfSorting.Json;
using BG3.BagsOfSorting.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BG3.BagsOfSorting.Services
{
    public static class CLIMethods
    {
        private const string BAG_NAME = "BOS_";

        private static readonly JsonSerializerOptions _configurationJsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        private static readonly JsonSerializerOptions _searchIndexJsonSerializerOptions = new()
        {
            Converters =
            {
                new InternedStringJsonConverter()
            },
            ReferenceHandler = ReferenceHandler.Preserve
        };

        private static CancellationTokenSource _cancellationTokenSource;

        public static void ExportAtlasIcons(Context context)
        {
            AtlasExporter.Export(context);

            ReleaseMemory();
        }

        public static void AddBag()
        {
            ServiceLocator.Configuration.Bags.Add(GetNewBag());

            SaveConfiguration(ServiceLocator.Configuration);
        }

        public static void GeneratePAK(Context context)
        {
            PAKGenerator.Generate(context);

            ReleaseMemory();
        }

        public static SearchIndex IndexPAK(Context context)
        {
            var searchIndex = SearchIndexer.Index(context);

            SaveSearchIndex(searchIndex);

            //NOTE: Make sure we load the interned String version
            searchIndex = LoadSearchIndex();

            ReleaseMemory();

            return searchIndex;
        }

        public static HashSet<SearchIndex.GameObject> SearchPAK(string query)
        {
            return SearchIndexer.Search(ServiceLocator.SearchIndex, query);
        }

        public static Configuration LoadConfiguration()
        {
            Configuration configuration;

            if (File.Exists(Constants.ConfigurationFile))
            {
                using var bagsConfigurationFile = File.OpenRead(Constants.ConfigurationFile);

                configuration = JsonSerializer.Deserialize<Configuration>(bagsConfigurationFile);
            }
            else
            {
                configuration = new Configuration
                {
                    Bags = new List<Configuration.Bag>(),
                    AdditionalTreasures = new JsonObject()
                };
            }

            return configuration;
        }

        public static void SaveConfiguration(Configuration configuration)
        {
            File.WriteAllText(
                Constants.ConfigurationFile, 
                JsonSerializer.Serialize(configuration, _configurationJsonSerializerOptions)
            );
        }

        public static SearchIndex LoadSearchIndex()
        {
            if (!File.Exists(Constants.SEARCH_INDEX_FILE))
            {
                return new SearchIndex();
            }

            using var searchIndexFile = File.OpenRead(Constants.SEARCH_INDEX_FILE);

            var searchIndex = JsonSerializer.Deserialize<SearchIndex>(
                searchIndexFile,
                _searchIndexJsonSerializerOptions
            );

            return searchIndex;
        }

        public static void SaveSearchIndex(SearchIndex index)
        {
            Directory.CreateDirectory(Constants.OUTPUT_PATH);

            File.WriteAllText(
                Constants.SEARCH_INDEX_FILE, 
                JsonSerializer.Serialize(index, _searchIndexJsonSerializerOptions)
            );
        }

        public static Configuration.Bag GetNewBag()
        {
            return new Configuration.Bag
            {
                MapKey = Guid.NewGuid(),
                Color = Configuration.EColor.None,
                ItemIcon = new Configuration.Icon
                {
                    Name = Constants.BASE_ICON
                },
                TooltipIcon = new Configuration.Icon
                {
                    Name = Constants.BASE_ICON
                }
            };
        }

        public static string GetNameForBag(Configuration.Bag bag)
        {
            return $"{BAG_NAME}{bag.MapKey.ToString().Replace("-", string.Empty).ToLower()}";
        }

        public static void ReleaseMemory()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = _cancellationTokenSource.Token;

            //Run this again after 90 seconds to Trim the internal ArrayPool<byte>.Shared used by JsonSerializer
            Task.Run(async () =>
            {
                await Task.Delay(90_000, cancellationToken);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.WaitForFullGCComplete();
                GC.Collect();
            }, cancellationToken);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();
        }
    }
}