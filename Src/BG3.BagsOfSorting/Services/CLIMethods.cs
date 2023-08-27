#pragma warning disable S101

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using BG3.BagsOfSorting.Models;
using LSLib.LS;
using LSLib.LS.Enums;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace BG3.BagsOfSorting.Services
{
    public static class CLIMethods
    {
        private const string SMALL_ICON_PREFIX = "SMALL_";
        private const string LARGE_ICON_PREFIX = "LARGE_";
        private const string SMALL_BASE_ICON = SMALL_ICON_PREFIX + Constants.BASE_ICON + ".png";
        private const string LARGE_BASE_ICON = LARGE_ICON_PREFIX + Constants.BASE_ICON + ".png";
        private const string BAG_NAME = "BOS_";
        private const int ICON_SIZE = 64;
        private const int TOOLTIP_SIZE = 380;

        private const string LOCALIZATION_DEFAULT_DISPLAYNAME_GUID = "hbf0be390g47d8g44c8g9012g76d639df51fe";
        private const string LOCALIZATION_DEFAULT_DESCRIPTION_GUID = "h0513bb29g9957g49adgaa2ag74896db0304c";

        private const string LOCALIZATION_FORMAT = @"<contentList date=""26/10/2020 01:10"">
  <content contentuid=""{0}"" version=""1"">{1}</content> 
  <content contentuid=""{2}"" version=""1"">{3}</content>
  <content contentuid=""{4}"" version=""1"">{5}</content>
</contentList>";

        private const string ROOTTEMPLATE_FORMAT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<save>
	<version major=""4"" minor=""0"" revision=""9"" build=""330"" />
	<region id=""Templates"">
		<node id=""Templates"">
			<children>
				<node id=""GameObjects"">
					<attribute id=""MapKey"" type=""FixedString"" value=""{0}"" />
					<attribute id=""Name"" type=""LSString"" value=""{1}"" />
					<attribute id=""LevelName"" type=""FixedString"" value="""" />
					<attribute id=""Type"" type=""FixedString"" value=""item"" />
					<attribute id=""ParentTemplateId"" type=""FixedString"" value=""3e6aac21-333b-4812-a554-376c2d157ba9"" />
					<attribute id=""DisplayName"" type=""TranslatedString"" handle=""{2}"" version=""1"" />
					<attribute id=""Description"" type=""TranslatedString"" handle=""{3}"" version=""1"" />
					<attribute id=""TechnicalDescription"" type=""TranslatedString"" handle=""{4}"" version=""1"" />
					<attribute id=""Icon"" type=""FixedString"" value=""{1}"" />
					<attribute id=""Stats"" type=""FixedString"" value=""{1}"" />
                    <attribute id=""StoryItem"" type=""bool"" value=""{5}"" />
					<attribute id=""ContainerAutoAddOnPickup"" type=""bool"" value=""{6}"" />
					<attribute id=""ContainerContentFilterCondition"" type=""LSString"" value=""{7}"" />
				</node>
			</children>
		</node>
	</region>
</save>";

        private const string DATA_FORMAT = @"new entry ""{0}""
type ""Object""
using ""OBJ_Bag""
data ""RootTemplate"" ""{1}""
data ""Rarity"" ""{2}""
";

        private const string ATLAS_FORMAT = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<save>
    <version major=""4"" minor=""0"" revision=""6"" build=""5"" />
    <region id=""TextureAtlasInfo"">
        <node id=""root"">
            <children>
                <node id=""TextureAtlasIconSize"">
                    <attribute id=""Height"" type=""int64"" value=""64""/>
                    <attribute id=""Width"" type=""int64"" value=""64""/>
                </node>
                <node id=""TextureAtlasPath"">
                    <attribute id=""Path"" type=""LSString"" value=""Assets/Textures/Icons/{0}.dds""/>
                    <attribute id=""UUID"" type=""FixedString"" value=""{1}""/>
                </node>
                <node id=""TextureAtlasTextureSize"">
                    <attribute id=""Height"" type=""int64"" value=""64""/>
                    <attribute id=""Width"" type=""int64"" value=""64""/>
                </node>
            </children>
        </node>
    </region>
    <region id=""IconUVList"">
        <node id=""root"">
            <children>
                <node id=""IconUV"">
                    <attribute id=""MapKey"" type=""FixedString"" value=""{0}""/>
                    <attribute id=""U1"" type=""float"" value=""0.0""/>
                    <attribute id=""U2"" type=""float"" value=""1.0""/>
                    <attribute id=""V1"" type=""float"" value=""0.0""/>
                    <attribute id=""V2"" type=""float"" value=""1.0""/>
                </node>
            </children>
        </node>
    </region>
</save>";

        private const string TEXTUREBANK_FORMAT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<save>
	<version major=""4"" minor=""0"" revision=""6"" build=""5"" />
	<region id=""TextureBank"">
		<node id=""TextureBank"">
			<children>
				<node id=""Resource"">
					<attribute id=""ID"" type=""FixedString"" value=""{1}"" />
					<attribute id=""Localized"" type=""bool"" value=""False"" />
					<attribute id=""Name"" type=""LSString"" value=""{0}"" />
					<attribute id=""SRGB"" type=""bool"" value=""True"" />
					<attribute id=""SourceFile"" type=""LSString"" value=""Public/GustavDev/Assets/Textures/Icons/{0}.dds"" />
					<attribute id=""Streaming"" type=""bool"" value=""True"" />
					<attribute id=""Template"" type=""FixedString"" value=""{0}"" />
					<attribute id=""Type"" type=""int64"" value=""0"" />
					<attribute id=""_OriginalFileVersion_"" type=""int64"" value=""1"" />
				</node>
			</children>
		</node>
	</region>
</save>";

        private const string TREASURETABLE_FORMAT1 = @"treasure itemtypes ""Common"",""Uncommon"",""Rare"",""Epic"",""Legendary"",""Divine"",""Unique""
new treasuretable ""POW_TT""
CanMerge 1
{0}";

        private const string TREASURETABLE_FORMAT2 = @"new subtable ""{1},1""
object category ""{0}"",1,0,0,0,0,0,0,0
";

        private static readonly string CONTENT_STOCK_PATH = Path.Combine(Constants.CONTENT_PATH, "Stock");
        private static readonly string BAGS_OUTPUT_PATH = Path.Combine("Output", "Bags");
        private static readonly string BAGS_OUTPUT_PATH_REMOVED = Path.Combine("Output", "Bags_Removed");
        private static readonly string ICON_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "Assets", "Textures", "Icons");
        private static readonly string TOOLTIP_ICON_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "Game", "GUI", "Assets", "Tooltips", "ItemIcons");
        private static readonly string ROOTTEMPLATES_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "RootTemplates");
        private static readonly string DATA_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "Stats", "Generated", "Data");
        private static readonly string ATLAS_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "GUI");
        private static readonly string TEXTUREBANK_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "Content", "UI", "[PAK]_UI");
        private static readonly string LOCALIZATION_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Localization", "English");
        private static readonly string TREASURETABLE_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "DiceSet_01", "Stats", "Generated");

        private static readonly ResizeOptions _itemIconResizeOptions = new()
        {
            Size = new Size(ICON_SIZE, ICON_SIZE),
            Position = AnchorPositionMode.Center,
            Mode = ResizeMode.Max
        };

        private static readonly ResizeOptions _tooltipIconResizeOptions = new()
        {
            Size = new Size(TOOLTIP_SIZE, TOOLTIP_SIZE),
            Position = AnchorPositionMode.Center,
            Mode = ResizeMode.Max
        };

        private static List<string> _log;

        public static List<string> GetLog()
        {
            return _log;
        }

        public static void ExportAtlasIcons()
        {
            _log = new List<string>();

            var files = new DirectoryInfo(CONTENT_STOCK_PATH)
                .GetFiles("*.lsx", SearchOption.AllDirectories);

            Directory.CreateDirectory(Constants.ICONS_OUTPUT_PATH);

            foreach (var fileInfo in files)
            {
                try
                {
                    ExportAtlasIcons(fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    _log.Add($"[{fileInfo.Name} | Error] Unhandled exception: {ex.Message}");
                }
            }
        }

        // ReSharper disable AssignNullToNotNullAttribute
        // ReSharper disable PossibleNullReferenceException
        private static void ExportAtlasIcons(string fileName)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(File.OpenRead(fileName));

            var infoNodes = xmlDocument.SelectSingleNode("//region[@id='TextureAtlasInfo']/node/children");

            var atlas = new Atlas
            {
                IconWidth = int.Parse(infoNodes.SelectSingleNode("//node[@id='TextureAtlasIconSize']/attribute[@id='Width']/@value").Value),
                IconHeight = int.Parse(infoNodes.SelectSingleNode("//node[@id='TextureAtlasIconSize']/attribute[@id='Height']/@value").Value),
                TextureWidth = int.Parse(infoNodes.SelectSingleNode("//node[@id='TextureAtlasTextureSize']/attribute[@id='Width']/@value").Value),
                TextureHeight = int.Parse(infoNodes.SelectSingleNode("//node[@id='TextureAtlasTextureSize']/attribute[@id='Height']/@value").Value),
                TextureFile = infoNodes.SelectSingleNode("//node[@id='TextureAtlasPath']/attribute[@id='Path']/@value").Value,
                Icons = new List<Atlas.Icon>()
            };

            var iconsNodes = xmlDocument.SelectNodes("//region[@id='IconUVList']/node/children/node");

            for (var i = 0; i < iconsNodes.Count; i++)
            {
                var icon = iconsNodes[i];

                var u1 = double.Parse(
                    icon.SelectSingleNode("attribute[@id='U1']/@value").Value,
                    CultureInfo.InvariantCulture
                );

                var v1 = double.Parse(
                    icon.SelectSingleNode("attribute[@id='V1']/@value").Value,
                    CultureInfo.InvariantCulture
                );

                var x = (int)(u1 * atlas.TextureWidth);
                var y = (int)(v1 * atlas.TextureHeight);

                atlas.Icons.Add(new Atlas.Icon
                {
                    MapKey = icon.SelectSingleNode("attribute[@id='MapKey']/@value").Value,
                    X = x,
                    Y = y
                });
            }

            using var textureFile = Pfimage.FromFile(Path.Combine(CONTENT_STOCK_PATH, "Public", "Shared", atlas.TextureFile));

            foreach (var atlasIcon in atlas.Icons)
            {
                var result = Image.LoadPixelData<Bgra32>(textureFile.Data, textureFile.Width, textureFile.Height);
                result.Mutate(x => x.Crop(new Rectangle(atlasIcon.X, atlasIcon.Y, atlas.IconWidth, atlas.IconHeight)));
                result.SaveAsPng(Path.Combine(Constants.ICONS_OUTPUT_PATH, $"{atlasIcon.MapKey}.png"));
            }
        }
        // ReSharper enable AssignNullToNotNullAttribute
        // ReSharper enable PossibleNullReferenceException

        public static void GenerateBags()
        {
            _log = new List<string>();

            using var bagsConfigurationFile = File.OpenRead(Constants.BAG_CONFIGURATION_FILE);

            var bagConfiguration = JsonSerializer.Deserialize<BagConfiguration>(bagsConfigurationFile);

            if (Directory.Exists(BAGS_OUTPUT_PATH))
            {
                Directory.Move(BAGS_OUTPUT_PATH, BAGS_OUTPUT_PATH_REMOVED);
                Directory.Delete(BAGS_OUTPUT_PATH_REMOVED, true);
            }

            Directory.CreateDirectory(ICON_PATH);
            Directory.CreateDirectory(TOOLTIP_ICON_PATH);
            Directory.CreateDirectory(ROOTTEMPLATES_PATH);
            Directory.CreateDirectory(DATA_PATH);
            Directory.CreateDirectory(ATLAS_PATH);
            Directory.CreateDirectory(TEXTUREBANK_PATH);
            Directory.CreateDirectory(LOCALIZATION_PATH);

            //Include a backup of the Bags.json in the PAK
            File.Copy(Constants.BAG_CONFIGURATION_FILE, Path.Combine(BAGS_OUTPUT_PATH, "Bags.json"));

            var numberOfUniqueMapKeys = bagConfiguration
                .Bags
                .Select(x => x.MapKey)
                .ToHashSet()
                .Count;

            if (bagConfiguration.Bags.Count != numberOfUniqueMapKeys)
            {
                _log.Add("[Fatal Error] All MapKeys have to be unique");

                return;
            }

            foreach (var bag in bagConfiguration.Bags)
            {
                try
                {
                    GenerateBag(bagConfiguration, bag);
                }
                catch (Exception ex)
                {
                    _log.Add($"[{bag.Name} | Error] Unhandled exception: {ex.Message}");
                }
            }

            Directory.CreateDirectory(TREASURETABLE_PATH);

            GenerateTreasureTable(
                bagConfiguration,
                TREASURETABLE_PATH
            );

            CreatePackage(BAGS_OUTPUT_PATH);
        }

        private static void GenerateBag(BagConfiguration bagConfiguration, BagConfiguration.Bag bag)
        {
            bag.Name = GetNameForBag(bag);

            GenerateLocalization(bag, LOCALIZATION_PATH);
            GenerateItemIcon(bag, bagConfiguration.AlignGeneratedItemIconsRight, ICON_PATH);
            GenerateTooltipIcon(bag, TOOLTIP_ICON_PATH);
            GenerateRootTemplate(bag, ROOTTEMPLATES_PATH);
            GenerateData(bag, DATA_PATH);
            GenerateAtlas(bag, ATLAS_PATH);
            GenerateTextureBank(bag, TEXTUREBANK_PATH);
        }

        private static void GenerateLocalization(BagConfiguration.Bag bag, string outputPath)
        {
            var guid1 = GenerateLocalizationGuid();
            var guid2 = GenerateLocalizationGuid();
            var guid3 = GenerateLocalizationGuid();

            var result = string.Format(
                LOCALIZATION_FORMAT,
                guid1, bag.DisplayName,
                guid2, bag.Description,
                guid3, bag.TechnicalDescription
            );

            bag.DisplayName = string.IsNullOrWhiteSpace(bag.DisplayName)
                ? LOCALIZATION_DEFAULT_DISPLAYNAME_GUID
                : guid1;
            bag.Description = string.IsNullOrWhiteSpace(bag.Description)
                ? LOCALIZATION_DEFAULT_DESCRIPTION_GUID
                : guid2;
            bag.TechnicalDescription = guid3;

            var xmlPath = Path.Combine(outputPath, $"{bag.Name}.xml");
            var locaPath = Path.Combine(outputPath, $"{bag.Name}.loca");

            File.WriteAllText(xmlPath, result);

            ConvertXMLToLoca(xmlPath, locaPath);
        }

        // ReSharper disable AccessToDisposedClosure
        private static void GenerateItemIcon(BagConfiguration.Bag bag, bool alignGeneratedItemIconsRight, string outputPath)
        {
            var baseIconPath = Path.Combine(CONTENT_STOCK_PATH, SMALL_BASE_ICON);

            if (!File.Exists(baseIconPath))
            {
                _log.Add($"[{bag.Name} | Item Icon | Error] Could not find {baseIconPath}");

                return;
            }

            var outputIconPath = Path.Combine(outputPath, $"{bag.Name}.png");

            if (bag.ItemIcon == null)
            {
                File.Copy(baseIconPath, outputIconPath);
            }
            else
            {
                var iconPath = bag.ItemIcon.Custom
                    ? Path.Combine(Constants.CONTENT_CUSTOM_PATH, $"{SMALL_ICON_PREFIX}{bag.ItemIcon.Name}.png")
                    : Path.Combine(Constants.ICONS_OUTPUT_PATH, $"{bag.ItemIcon.Name}.png");

                if (!File.Exists(iconPath))
                {
                    _log.Add($"[{bag.Name} | Item Icon | Error] Could not find {iconPath}");

                    return;
                }

                if (bag.ItemIcon.Generate)
                {
                    using var baseIconImage = Image.Load(baseIconPath);
                    using var iconImage = Image.Load(iconPath);
                    using var outputImage = new Image<Rgba32>(ICON_SIZE, ICON_SIZE);

                    if (baseIconImage.Width != ICON_SIZE || baseIconImage.Height != ICON_SIZE)
                    {
                        _log.Add($"[{bag.Name} | Item Icon | Warning] Expected {baseIconPath} to be {ICON_SIZE}x{ICON_SIZE}, but it's {baseIconImage.Width}x{baseIconImage.Height}.");

                        baseIconImage.Mutate(x => x.Resize(_itemIconResizeOptions));
                    }

                    if (iconImage.Width != ICON_SIZE || iconImage.Height != ICON_SIZE)
                    {
                        _log.Add($"[{bag.Name} | Item Icon | Warning] Expected {iconPath} to be {ICON_SIZE}x{ICON_SIZE}, but it's {iconImage.Width}x{iconImage.Height}.");

                        iconImage.Mutate(x => x.Resize(_itemIconResizeOptions));
                    }

                    var twoThirds = (int)Math.Round(ICON_SIZE * 0.66f);

                    iconImage.Mutate(x => x.Resize(twoThirds, twoThirds));

                    if (alignGeneratedItemIconsRight)
                    {
                        outputImage.Mutate(x => x
                            .DrawImage(baseIconImage, new Point(-8, 0), 1f)
                            .DrawImage(iconImage, new Point(ICON_SIZE - twoThirds, 0), 1f)
                        );
                    }
                    else
                    {
                        outputImage.Mutate(x => x
                            .DrawImage(baseIconImage, new Point(-8, 0), 1f)
                            .Flip(FlipMode.Horizontal)
                            .DrawImage(iconImage, Point.Empty, 1f)
                        );
                    }

                    outputImage.SaveAsPng(outputIconPath);
                }
                else
                {
                    using var iconImage = Image.Load(iconPath);

                    if (iconImage.Width != ICON_SIZE || iconImage.Height != ICON_SIZE)
                    {
                        _log.Add($"[{bag.Name} | Item Icon | Warning] Expected {iconPath} to be {ICON_SIZE}x{ICON_SIZE}, but it's {iconImage.Width}x{iconImage.Height}.");

                        iconImage.Mutate(x => x.Resize(_itemIconResizeOptions));
                    }

                    iconImage.SaveAsPng(outputIconPath);
                }
            }

            var fullPath = Path.GetFullPath(outputIconPath);
            var fullPathDirectory = Path.GetFullPath(Path.GetDirectoryName(fullPath));

            ConvertPNGtoDDS(fullPath, fullPathDirectory);
        }

        private static void GenerateTooltipIcon(BagConfiguration.Bag bag, string outputPath)
        {
            var baseIconPath = Path.Combine(CONTENT_STOCK_PATH, LARGE_BASE_ICON);

            if (!File.Exists(baseIconPath))
            {
                _log.Add($"[{bag.Name} | Tooltip Icon | Error] Could not find {baseIconPath}");

                return;
            }

            var outputIconPath = Path.Combine(outputPath, $"{bag.Name}.png");

            if (bag.TooltipIcon == null)
            {
                File.Copy(baseIconPath, outputIconPath);
            }
            else
            {
                var iconPath = bag.TooltipIcon.Custom
                    ? Path.Combine(Constants.CONTENT_CUSTOM_PATH, $"{LARGE_ICON_PREFIX}{bag.TooltipIcon.Name}.png")
                    : Path.Combine(Constants.ICONS_OUTPUT_PATH, $"{bag.TooltipIcon.Name}.png");

                if (!File.Exists(iconPath))
                {
                    _log.Add($"[{bag.Name} | Tooltip Icon | Error] Could not find {iconPath}");

                    return;
                }

                if (bag.TooltipIcon.Generate)
                {
                    using var baseIconImage = Image.Load(baseIconPath);
                    using var iconImage = Image.Load(iconPath);
                    using var outputImage = new Image<Rgba32>(TOOLTIP_SIZE, TOOLTIP_SIZE);

                    if (baseIconImage.Width != TOOLTIP_SIZE || baseIconImage.Height != TOOLTIP_SIZE)
                    {
                        _log.Add($"[{bag.Name} => Tooltip Icon | Warning] Expected {baseIconPath} to be {TOOLTIP_SIZE}x{TOOLTIP_SIZE}, but it's {baseIconImage.Width}x{baseIconImage.Height}.");

                        baseIconImage.Mutate(x => x.Resize(_tooltipIconResizeOptions));
                    }

                    if (iconImage.Width > TOOLTIP_SIZE || iconImage.Height > TOOLTIP_SIZE)
                    {
                        _log.Add($"[{bag.Name} | Tooltip Icon | Warning] Expected {iconPath} to be {TOOLTIP_SIZE}x{TOOLTIP_SIZE} or less, but it's {iconImage.Width}x{iconImage.Height}.");

                        iconImage.Mutate(x => x.Resize(_tooltipIconResizeOptions));
                    }

                    var topRight = TOOLTIP_SIZE - ICON_SIZE * 2 - 8;

                    outputImage.Mutate(x => x
                        .DrawImage(baseIconImage, new Point(0, 0), 1f)
                        .DrawImage(iconImage, new Point(topRight, ICON_SIZE + 16), 1f)
                    );

                    outputImage.SaveAsPng(outputIconPath);
                }
                else
                {
                    using var iconImage = Image.Load(iconPath);

                    if (iconImage.Width != TOOLTIP_SIZE || iconImage.Height != TOOLTIP_SIZE)
                    {
                        _log.Add($"[{bag.Name} | Tooltip Icon | Warning] Expected {iconPath} to be {TOOLTIP_SIZE}x{TOOLTIP_SIZE}, but it's {iconImage.Width}x{iconImage.Height}.");

                        iconImage.Mutate(x => x.Resize(_tooltipIconResizeOptions));
                    }

                    iconImage.SaveAsPng(outputIconPath);
                }
            }

            //NOTE: Files in PAKs are case-sensitive!
            var fullPath = Path.GetFullPath(outputIconPath);
            var fullPathDirectory = Path.GetFullPath(Path.GetDirectoryName(fullPath));
            var fullPathNoExtension = Path.Combine(fullPathDirectory, Path.GetFileNameWithoutExtension(outputIconPath));

            ConvertPNGtoDDS(fullPath, fullPathDirectory);

            File.Move($"{fullPathNoExtension}.dds", $"{fullPathNoExtension}.DDS");
        }
        // ReSharper enable AccessToDisposedClosure

        private static void GenerateRootTemplate(BagConfiguration.Bag bag, string outputPath)
        {
            var lsxPath = Path.Combine(outputPath, $"{bag.Name}.lsx");
            var lsfPath = Path.Combine(outputPath, $"{bag.Name}.lsf");

            File.WriteAllText(
                lsxPath,
                string.Format(
                    ROOTTEMPLATE_FORMAT,
                    bag.MapKey,
                    bag.Name,
                    bag.DisplayName,
                    bag.Description,
                    bag.TechnicalDescription,
                    bag.Color == BagConfiguration.EColor.Orange,
                    !string.IsNullOrWhiteSpace(bag.AutoPickupCondition),
                    string.IsNullOrWhiteSpace(bag.AutoPickupCondition) ? string.Empty : bag.AutoPickupCondition
                )
            );

            ConvertLSXToLSF(lsxPath, lsfPath);
        }

        private static void GenerateData(BagConfiguration.Bag bag, string outputPath)
        {
            File.WriteAllText(
                Path.Combine(outputPath, $"{bag.Name}.txt"),
                string.Format(
                    DATA_FORMAT,
                    bag.Name,
                    bag.MapKey,
                    ConvertColorToRarity(bag.Color)
                )
            );
        }

        private static void GenerateAtlas(BagConfiguration.Bag bag, string outputPath)
        {
            var lsxPath = Path.Combine(outputPath, $"{bag.Name}.lsx");
            var lsfPath = Path.Combine(outputPath, $"{bag.Name}.lsf");

            File.WriteAllText(
                lsxPath,
                string.Format(
                    ATLAS_FORMAT,
                    bag.Name,
                    bag.MapKey
                )
            );

            ConvertLSXToLSF(lsxPath, lsfPath);
        }

        private static void GenerateTextureBank(BagConfiguration.Bag bag, string outputPath)
        {
            var lsxPath = Path.Combine(outputPath, $"{bag.Name}.lsx");
            var lsfPath = Path.Combine(outputPath, $"{bag.Name}.lsf");

            File.WriteAllText(
                lsxPath,
                string.Format(
                    TEXTUREBANK_FORMAT,
                    bag.Name,
                    bag.MapKey
                )
            );

            ConvertLSXToLSF(lsxPath, lsfPath);
        }

        private static void GenerateTreasureTable(
            BagConfiguration bagConfiguration,
            string outputPath
        )
        {
            var stringBuilder = new StringBuilder();

            var bags = bagConfiguration.Bags.Select(x => (x.Name, x.Amount)).ToList();

            foreach (var (name, amount) in bags)
            {
                stringBuilder.AppendFormat(TREASURETABLE_FORMAT2, $"I_{name}", amount ?? 1);
            }

            if (bagConfiguration.AdditionalTreasures != null)
            {
                foreach (var kvp in bagConfiguration.AdditionalTreasures)
                {
                    stringBuilder.AppendFormat(TREASURETABLE_FORMAT2, kvp.Key, kvp.Value.GetValue<int>());
                }
            }

            var output = string.Format(TREASURETABLE_FORMAT1, stringBuilder);

            File.WriteAllText(Path.Combine(outputPath, "TreasureTable.txt"), output);
        }

        private static void CreatePackage(string inputPath)
        {
            var packager = new Packager();

            packager.CreatePackage(
                Path.GetFullPath(Path.Combine(inputPath, "BagsOfSorting.pak")),
                Path.GetFullPath(inputPath),
                new PackageCreationOptions
                {
                    Priority = byte.MaxValue,
                    Version = PackageVersion.V18
                }
            );
        }

        public static void AddBag()
        {
            var bagsConfiguration = LoadConfiguration();

            bagsConfiguration.Bags.Add(GetNewBag());

            SaveConfiguration(bagsConfiguration);
        }

        public static BagConfiguration LoadConfiguration()
        {
            BagConfiguration bagsConfiguration;

            if (File.Exists(Constants.BAG_CONFIGURATION_FILE))
            {
                var bagsConfigurationFile = File.OpenRead(Constants.BAG_CONFIGURATION_FILE);

                bagsConfiguration = JsonSerializer.Deserialize<BagConfiguration>(bagsConfigurationFile);

                bagsConfigurationFile.Dispose();
            }
            else
            {
                bagsConfiguration = new BagConfiguration
                {
                    Bags = new List<BagConfiguration.Bag>(),
                    AdditionalTreasures = new JsonObject()
                };
            }

            return bagsConfiguration;
        }

        public static void SaveConfiguration(BagConfiguration bagConfiguration)
        {
            File.WriteAllText(Constants.BAG_CONFIGURATION_FILE, JsonSerializer.Serialize(bagConfiguration, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }

        public static BagConfiguration.Bag GetNewBag()
        {
            return new BagConfiguration.Bag
            {
                MapKey = Guid.NewGuid(),
                Color = BagConfiguration.EColor.None,
                ItemIcon = new BagConfiguration.Icon
                {
                    Name = Constants.BASE_ICON
                },
                TooltipIcon = new BagConfiguration.Icon
                {
                    Name = Constants.BASE_ICON
                }
            };
        }

        public static string GetNameForBag(BagConfiguration.Bag bag)
        {
            return $"{BAG_NAME}{bag.MapKey.ToString().Replace("-", string.Empty).ToLower()}";
        }

        private static string ConvertColorToRarity(BagConfiguration.EColor color)
        {
            return color switch
            {
                BagConfiguration.EColor.Green => "Uncommon",
                BagConfiguration.EColor.Blue => "Rare",
                BagConfiguration.EColor.Pink => "VeryRare",
                BagConfiguration.EColor.Gold => "Legendary",
                _ => "Common"
            };
        }

        private static void ConvertPNGtoDDS(string inputPath, string outputDirectory)
        {
            var process = Process.Start(
                Path.Combine(CONTENT_STOCK_PATH, "texconv.exe"),
                $"-f DXT5 \"{inputPath}\" -o \"{outputDirectory}\""
            );

            process.WaitForExit();
        }

        private static void ConvertXMLToLoca(string xmlPath, string locaPath)
        {
            using var xmlFileStream = File.OpenRead(xmlPath);
            using var xmlFile = new LocaXmlReader(xmlFileStream);

            using var locaFileStream = File.OpenWrite(locaPath);
            var locaFile = new LocaWriter(locaFileStream);
            locaFile.Write(xmlFile.Read());
        }

        private static void ConvertLSXToLSF(string lsxPath, string lsfPath)
        {
            using var lsxFileStream = File.OpenRead(lsxPath);
            using var lsxFile = new LSXReader(lsxFileStream);

            using var lsfFileStream = File.OpenWrite(lsfPath);
            var lsfFile = new LSFWriter(lsfFileStream);
            lsfFile.Write(lsxFile.Read());
        }

        private static string GenerateLocalizationGuid()
        {
            return $"h{Guid.NewGuid()}".Replace('-', 'g');
        }
    }
}