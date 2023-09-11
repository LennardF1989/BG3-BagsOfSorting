#pragma warning disable S1199

using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using BG3.BagsOfSorting.Extensions;
using BG3.BagsOfSorting.Models;
using LSLib.LS;
using LSLib.LS.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Configuration = BG3.BagsOfSorting.Models.Configuration;
using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.ImageSharp.Point;
using Size = SixLabors.ImageSharp.Size;

namespace BG3.BagsOfSorting.Services
{
    public static partial class PAKGenerator
    {
        [LibraryImport("texconv.dll", EntryPoint = "texconv")]
        private static partial int Texconv(
            int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] argv,
            [MarshalAs(UnmanagedType.Bool)] bool verbose = true,
            [MarshalAs(UnmanagedType.Bool)] bool initCOM = false,
            [MarshalAs(UnmanagedType.Bool)] bool allowSlowCodec = false,
            [MarshalAs(UnmanagedType.LPWStr)] string errorBuffer = null,
            int errorBufferSize = 0
        );

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
new treasuretable ""{0}""
CanMerge 1
{1}";

        private const string TREASURETABLE_FORMAT2 = @"new subtable ""{1},1""
object category ""{0}"",1,0,0,0,0,0,0,0
";

        private const string MODS_META = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<save>
    <version major=""4"" minor=""0"" revision=""9"" build=""900""/>
    <region id=""Config"">
        <node id=""root"">
            <children>
                <node id=""Dependencies""/>
                <node id=""ModuleInfo"">
                    <attribute id=""Author"" type=""LSString"" value=""LennardF1989""/>
                    <attribute id=""CharacterCreationLevelName"" type=""FixedString"" value=""""/>
                    <attribute id=""Description"" type=""LSString"" value=""""/>
                    <attribute id=""Folder"" type=""LSString"" value=""{0}""/>
                    <attribute id=""GMTemplate"" type=""FixedString"" value=""""/>
                    <attribute id=""LobbyLevelName"" type=""FixedString"" value=""""/>
                    <attribute id=""MD5"" type=""LSString"" value=""""/>
                    <attribute id=""MainMenuBackgroundVideo"" type=""FixedString"" value=""""/>
                    <attribute id=""MenuLevelName"" type=""FixedString"" value=""""/>
                    <attribute id=""Name"" type=""FixedString"" value=""{1}""/>
                    <attribute id=""NumPlayers"" type=""uint8"" value=""4""/>
                    <attribute id=""PhotoBooth"" type=""FixedString"" value=""""/>
                    <attribute id=""StartupLevelName"" type=""FixedString"" value=""""/>
                    <attribute id=""Tags"" type=""LSWString"" value=""""/>
                    <attribute id=""Type"" type=""FixedString"" value=""Add-on""/>
                    <attribute id=""UUID"" type=""FixedString"" value=""{2}""/>
                    <attribute id=""Version64"" type=""int64"" value=""1""/>
                    <children>
                        <node id=""PublishVersion"">
                            <attribute id=""Version64"" type=""int64"" value=""1""/>
                        </node>
                        <node id=""Scripts""/>
                        <node id=""TargetModes"">
                            <children>
                                <node id=""Target"">
                                    <attribute id=""Object"" type=""FixedString"" value=""Story""/>
                                </node>
                            </children>
                        </node>
                    </children>
                </node>
            </children>
        </node>
    </region>
</save>";

        private static readonly string BAGS_OUTPUT_PATH = Path.Combine(Constants.OUTPUT_PATH, "Bags");
        private static readonly string BAGS_OUTPUT_PATH_REMOVED = Path.Combine(Constants.OUTPUT_PATH, "Bags_Removed");
        private static readonly string ICON_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "Assets", "Textures", "Icons");
        private static readonly string TOOLTIP_ICON_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "Game", "GUI", "Assets", "Tooltips", "ItemIcons");
        private static readonly string CONTROLLER_ICON_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "Game", "GUI", "Assets", "ControllerUIIcons", "items_png");
        private static readonly string ROOTTEMPLATES_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "RootTemplates");
        private static readonly string DATA_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "Stats", "Generated", "Data");
        private static readonly string ATLAS_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "GUI");
        private static readonly string TEXTUREBANK_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Public", "GustavDev", "Content", "UI", "[PAK]_UI");
        private static readonly string LOCALIZATION_PATH = Path.Combine(BAGS_OUTPUT_PATH, "Localization", "English");

        private static readonly ResizeOptions _itemIconResizeOptions = new()
        {
            Size = new Size(Constants.ICON_SIZE, Constants.ICON_SIZE),
            Position = AnchorPositionMode.Center,
            Mode = ResizeMode.Max
        };

        private static readonly ResizeOptions _tooltipIconResizeOptions = new()
        {
            Size = new Size(Constants.TOOLTIP_SIZE, Constants.TOOLTIP_SIZE),
            Position = AnchorPositionMode.Center,
            Mode = ResizeMode.Max
        };

        public static void Generate(Context context)
        {
            if (Directory.Exists(BAGS_OUTPUT_PATH))
            {
                Directory.Move(BAGS_OUTPUT_PATH, BAGS_OUTPUT_PATH_REMOVED);
                Directory.Delete(BAGS_OUTPUT_PATH_REMOVED, true);
            }

            BundlePouchOfWonders(context);
            GenerateBags(context);
            GenerateTreasureTable(context);
            CreatePackage(BAGS_OUTPUT_PATH, Constants.OUTPUT_PATH);
        }

        private static void GenerateTreasureTable(Context context)
        {
            if (context.Configuration.AdditionalTreasures.Count <= 0)
            {
                return;
            }

            var treasureTablePath = Path.Combine(
                BAGS_OUTPUT_PATH,
                "Public",
                context.Configuration.TreasureTable.FolderName,
                "Stats",
                "Generated"
            );

            Directory.CreateDirectory(treasureTablePath);

            GenerateTreasureTableFile(
                context,
                treasureTablePath
            );

            //If the TreasureTable-folder is different from the default value, we need to generate a meta.lsx to load it
            if (context.Configuration.TreasureTable != null &&
                context.Configuration.TreasureTable.FolderName != Constants.DEFAULT_TREASURETABLE_FOLDERNAME
               )
            {
                GenerateModsMeta(
                    context.Configuration.TreasureTable.FolderName, 
                    $"Bags of Sorting - {context.Configuration.TreasureTable.FolderName}"
                );
            }
        }

        private static void BundlePouchOfWonders(Context context)
        {
            if (!context.Configuration.BundlePouchOfWonders)
            {
                return;
            }

            var sourceDirectory = new DirectoryInfo(Constants.CONTENT_POW_PATH);
            var targetDirectory = new DirectoryInfo(BAGS_OUTPUT_PATH);

            sourceDirectory.Copy(targetDirectory, true);
        }

        private static void GenerateBags(Context context)
        {
            if (!context.Configuration.Bags.Any())
            {
                return;
            }

            Directory.CreateDirectory(ICON_PATH);
            Directory.CreateDirectory(TOOLTIP_ICON_PATH);
            Directory.CreateDirectory(CONTROLLER_ICON_PATH);
            Directory.CreateDirectory(ROOTTEMPLATES_PATH);
            Directory.CreateDirectory(DATA_PATH);
            Directory.CreateDirectory(ATLAS_PATH);
            Directory.CreateDirectory(TEXTUREBANK_PATH);
            Directory.CreateDirectory(LOCALIZATION_PATH);

            //Include a backup of the Bags.json in the PAK
            File.Copy(Constants.ConfigurationFile, Path.Combine(BAGS_OUTPUT_PATH, "Bags.json"));

            var numberOfUniqueMapKeys = context.Configuration
                .Bags
                .Select(x => x.MapKey)
                .ToHashSet()
                .Count;

            if (context.Configuration.Bags.Count != numberOfUniqueMapKeys)
            {
                context.LogMessage("[Fatal Error] All MapKeys have to be unique");

                return;
            }

            foreach (var bag in context.Configuration.Bags)
            {
                try
                {
                    GenerateBag(context, bag);
                }
                catch (Exception ex)
                {
                    context.LogMessage($"[{bag.Name} | Error] Unhandled exception: {ex.Message}");
                }
            }
        }

        private static void GenerateBag(Context context, Configuration.Bag bag)
        {
            bag.Name = CLIMethods.GetNameForBag(bag);

            GenerateLocalization(bag, LOCALIZATION_PATH);
            GenerateItemIcon(context, bag, ICON_PATH);
            GenerateTooltipIcon(context, bag, TOOLTIP_ICON_PATH, CONTROLLER_ICON_PATH);
            GenerateRootTemplate(bag, ROOTTEMPLATES_PATH);
            GenerateData(bag, DATA_PATH);
            GenerateAtlas(bag, ATLAS_PATH);
            GenerateTextureBank(bag, TEXTUREBANK_PATH);
        }

        private static void GenerateLocalization(Configuration.Bag bag, string outputPath)
        {
            var guid1 = GenerateLocalizationGuid();
            var guid2 = GenerateLocalizationGuid();
            var guid3 = GenerateLocalizationGuid();

            var contents = string.Format(
                LOCALIZATION_FORMAT,
                guid1,
                string.IsNullOrWhiteSpace(bag.DisplayName)
                    ? string.Empty
                    : SecurityElement.Escape(bag.DisplayName),
                guid2,
                string.IsNullOrWhiteSpace(bag.Description)
                    ? string.Empty
                    : SecurityElement.Escape(bag.Description),
                guid3,
                string.IsNullOrWhiteSpace(bag.TechnicalDescription)
                    ? string.Empty
                    : SecurityElement.Escape(bag.TechnicalDescription)
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

            ConvertXMLToLoca(contents, xmlPath, locaPath);
        }

        // ReSharper disable AccessToDisposedClosure
        private static void GenerateItemIcon(
            Context context,
            Configuration.Bag bag,
            string outputPath
        )
        {
            var baseIconPath = Path.Combine(Constants.CONTENT_STOCK_PATH, Constants.SMALL_BASE_ICON);

            if (!File.Exists(baseIconPath))
            {
                context.LogMessage($"[{bag.Name} | Item Icon | Error] Could not find {baseIconPath}");

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
                    ? Path.Combine(Constants.CONTENT_CUSTOM_PATH, $"{bag.ItemIcon.Name}.png")
                    : Path.Combine(Constants.ICONS_OUTPUT_PATH, $"{bag.ItemIcon.Name}.png");

                if (!File.Exists(iconPath))
                {
                    context.LogMessage($"[{bag.Name} | Item Icon | Error] Could not find {iconPath}");

                    return;
                }

                if (bag.ItemIcon.Generate)
                {
                    using var baseIconImage = Image.Load(baseIconPath);
                    using var iconImage = Image.Load(iconPath);
                    using var outputImage = new Image<Rgba32>(Constants.ICON_SIZE, Constants.ICON_SIZE);

                    if (baseIconImage.Width != Constants.ICON_SIZE || baseIconImage.Height != Constants.ICON_SIZE)
                    {
                        context.LogMessage($"[{bag.Name} | Item Icon | Warning] Expected {baseIconPath} to be {Constants.ICON_SIZE}x{Constants.ICON_SIZE}, but it's {baseIconImage.Width}x{baseIconImage.Height}.");
                        baseIconImage.Mutate(x => x.Resize(_itemIconResizeOptions));
                    }

                    if (iconImage.Width != Constants.ICON_SIZE || iconImage.Height != Constants.ICON_SIZE)
                    {
                        context.LogMessage($"[{bag.Name} | Item Icon | Warning] Expected {iconPath} to be {Constants.ICON_SIZE}x{Constants.ICON_SIZE}, but it's {iconImage.Width}x{iconImage.Height}.");

                        iconImage.Mutate(x => x.Resize(_itemIconResizeOptions));
                    }

                    var twoThirds = (int)Math.Round(Constants.ICON_SIZE * 0.66f);

                    iconImage.Mutate(x => x.Resize(twoThirds, twoThirds));

                    if (context.Configuration.AlignGeneratedItemIconsRight)
                    {
                        outputImage.Mutate(x => x
                            .DrawImage(baseIconImage, new Point(-8, 0), 1f)
                            .DrawImage(iconImage, new Point(Constants.ICON_SIZE - twoThirds, 0), 1f)
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

                    if (iconImage.Width != Constants.ICON_SIZE || iconImage.Height != Constants.ICON_SIZE)
                    {
                        context.LogMessage($"[{bag.Name} | Item Icon | Warning] Expected {iconPath} to be {Constants.ICON_SIZE}x{Constants.ICON_SIZE}, but it's {iconImage.Width}x{iconImage.Height}.");

                        iconImage.Mutate(x => x.Resize(_itemIconResizeOptions));
                    }

                    iconImage.SaveAsPng(outputIconPath);
                }
            }

            var fullPath = Path.GetFullPath(outputIconPath);
            var fullPathDirectory = Path.GetFullPath(Path.GetDirectoryName(fullPath)!);

            ConvertPNGtoDDS(context, fullPath, fullPathDirectory, $"{bag.Name} | Item Icon");
        }

        private static void GenerateTooltipIcon(
            Context context,
            Configuration.Bag bag,
            string outputPath,
            string additionalOutputPath
        )
        {
            var baseIconPath = Path.Combine(Constants.CONTENT_STOCK_PATH, Constants.LARGE_BASE_ICON);

            if (!File.Exists(baseIconPath))
            {
                context.LogMessage($"[{bag.Name} | Tooltip Icon | Error] Could not find {baseIconPath}");

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
                    ? Path.Combine(Constants.CONTENT_CUSTOM_PATH, $"{bag.TooltipIcon.Name}.png")
                    : Path.Combine(Constants.ICONS_OUTPUT_PATH, $"{bag.TooltipIcon.Name}.png");

                if (!File.Exists(iconPath))
                {
                    context.LogMessage($"[{bag.Name} | Tooltip Icon | Error] Could not find {iconPath}");

                    return;
                }

                if (bag.TooltipIcon.Generate)
                {
                    using var baseIconImage = Image.Load(baseIconPath);
                    using var iconImage = Image.Load(iconPath);
                    using var outputImage = new Image<Rgba32>(Constants.TOOLTIP_SIZE, Constants.TOOLTIP_SIZE);

                    if (baseIconImage.Width != Constants.TOOLTIP_SIZE || baseIconImage.Height != Constants.TOOLTIP_SIZE)
                    {
                        context.LogMessage($"[{bag.Name} => Tooltip Icon | Warning] Expected {baseIconPath} to be {Constants.TOOLTIP_SIZE}x{Constants.TOOLTIP_SIZE}, but it's {baseIconImage.Width}x{baseIconImage.Height}.");
                        baseIconImage.Mutate(x => x.Resize(_tooltipIconResizeOptions));
                    }

                    if (iconImage.Width > Constants.TOOLTIP_SIZE || iconImage.Height > Constants.TOOLTIP_SIZE)
                    {
                        context.LogMessage($"[{bag.Name} | Tooltip Icon | Warning] Expected {iconPath} to be {Constants.TOOLTIP_SIZE}x{Constants.TOOLTIP_SIZE} or less, but it's {iconImage.Width}x{iconImage.Height}.");

                        iconImage.Mutate(x => x.Resize(_tooltipIconResizeOptions));
                    }

                    const int topRight = Constants.TOOLTIP_SIZE - Constants.ICON_SIZE * 2 - 8;

                    outputImage.Mutate(x => x
                        .DrawImage(baseIconImage, new Point(0, 0), 1f)
                        .DrawImage(iconImage, new Point(topRight, Constants.ICON_SIZE + 16), 1f)
                    );

                    outputImage.SaveAsPng(outputIconPath);
                }
                else
                {
                    using var iconImage = Image.Load(iconPath);

                    if (iconImage.Width != Constants.TOOLTIP_SIZE || iconImage.Height != Constants.TOOLTIP_SIZE)
                    {
                        context.LogMessage($"[{bag.Name} | Tooltip Icon | Warning] Expected {iconPath} to be {Constants.TOOLTIP_SIZE}x{Constants.TOOLTIP_SIZE}, but it's {iconImage.Width}x{iconImage.Height}.");

                        iconImage.Mutate(x => x.Resize(_tooltipIconResizeOptions));
                    }

                    iconImage.SaveAsPng(outputIconPath);
                }
            }

            //NOTE: Files in PAKs are case-sensitive!
            var fullPath = Path.GetFullPath(outputIconPath);
            var fullPathDirectory = Path.GetFullPath(Path.GetDirectoryName(fullPath)!);
            var fullPathNoExtension = Path.Combine(fullPathDirectory, Path.GetFileNameWithoutExtension(outputIconPath));

            ConvertPNGtoDDS(context, fullPath, fullPathDirectory, $"{bag.Name} | Tooltip Icon");

            var destinationFileName = $"{fullPathNoExtension}.DDS";
            File.Move($"{fullPathNoExtension}.dds", destinationFileName);

            //Copy to the additional output path
            File.Copy(
                destinationFileName,
                Path.Combine(Path.GetFullPath(additionalOutputPath), Path.GetFileName(destinationFileName))
            );
        }
        // ReSharper enable AccessToDisposedClosure

        private static void GenerateRootTemplate(Configuration.Bag bag, string outputPath)
        {
            var contents = string.Format(
                ROOTTEMPLATE_FORMAT,
                bag.MapKey,
                bag.Name,
                bag.DisplayName,
                bag.Description,
                bag.TechnicalDescription,
                bag.Color == Configuration.EColor.Orange,
                !string.IsNullOrWhiteSpace(bag.AutoPickupCondition),
                string.IsNullOrWhiteSpace(bag.AutoPickupCondition) ? string.Empty : bag.AutoPickupCondition
            );

            var lsxPath = Path.Combine(outputPath, $"{bag.Name}.lsx");
            var lsfPath = Path.Combine(outputPath, $"{bag.Name}.lsf");

            ConvertLSXToLSF(contents, lsxPath, lsfPath);
        }

        private static void GenerateData(Configuration.Bag bag, string outputPath)
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

        private static void GenerateAtlas(Configuration.Bag bag, string outputPath)
        {
            var contents = string.Format(
                ATLAS_FORMAT,
                bag.Name,
                bag.MapKey
            );

            var lsxPath = Path.Combine(outputPath, $"{bag.Name}.lsx");
            var lsfPath = Path.Combine(outputPath, $"{bag.Name}.lsf");

            ConvertLSXToLSF(contents, lsxPath, lsfPath);
        }

        private static void GenerateTextureBank(Configuration.Bag bag, string outputPath)
        {
            var contents = string.Format(
                TEXTUREBANK_FORMAT,
                bag.Name,
                bag.MapKey
            );

            var lsxPath = Path.Combine(outputPath, $"{bag.Name}.lsx");
            var lsfPath = Path.Combine(outputPath, $"{bag.Name}.lsf");

            ConvertLSXToLSF(contents, lsxPath, lsfPath);
        }

        private static void GenerateTreasureTableFile(
            Context context,
            string outputPath
        )
        {
            var stringBuilder = new StringBuilder();

            var bags = context.Configuration.Bags.Select(x => (x.Name, x.Amount)).ToList();

            foreach (var (name, amount) in bags)
            {
                stringBuilder.AppendFormat(TREASURETABLE_FORMAT2, $"I_{name}", amount ?? 1);
            }

            if (context.Configuration.AdditionalTreasures != null)
            {
                foreach (var kvp in context.Configuration.AdditionalTreasures)
                {
                    stringBuilder.AppendFormat(TREASURETABLE_FORMAT2, kvp.Key, kvp.Value.GetValue<int>());
                }
            }

            //NOTE: If we didn't add anything, make sure it's still a valid Treasure Table.
            if (stringBuilder.Length == 0)
            {
                stringBuilder.AppendFormat(TREASURETABLE_FORMAT2, "T_Empty", 1);
            }

            var output = string.Format(TREASURETABLE_FORMAT1, context.Configuration.TreasureTable.Name, stringBuilder);

            File.WriteAllText(Path.Combine(outputPath, "TreasureTable.txt"), output);
        }

        private static void GenerateModsMeta(string folderName, string modName)
        {
            var outputPath = Path.Combine(BAGS_OUTPUT_PATH, "Mods", folderName);

            Directory.CreateDirectory(outputPath);

            //Copy the ModFixer
            var sourceDirectory = new DirectoryInfo(Constants.CONTENT_MODFIXER_PATH);
            var targetDirectory = new DirectoryInfo(outputPath);

            sourceDirectory.Copy(targetDirectory, true);

            //NOTE: Convert the name into a deterministic GUID using MD5
            var uuid = new Guid(
                MD5.HashData(Encoding.UTF8.GetBytes(folderName))
            );

            var contents = string.Format(MODS_META, folderName, modName, uuid);

            var lsxPath = Path.Combine(outputPath, "meta.lsx");
            var lsfPath = Path.Combine(outputPath, "meta.lsf");

            ConvertLSXToLSF(contents, lsxPath, lsfPath);
        }

        private static void CreatePackage(string inputPath, string outputPath)
        {
            var packager = new Packager();

            packager.CreatePackage(
                Path.GetFullPath(Path.Combine(outputPath, "BagsOfSorting.pak")),
                Path.GetFullPath(inputPath),
                new PackageCreationOptions
                {
                    Priority = byte.MaxValue,
                    Version = PackageVersion.V18,
                    Compression = CompressionMethod.LZ4
                }
            );
        }

        private static string ConvertColorToRarity(Configuration.EColor color)
        {
            return color switch
            {
                Configuration.EColor.Green => "Uncommon",
                Configuration.EColor.Blue => "Rare",
                Configuration.EColor.Pink => "VeryRare",
                Configuration.EColor.Gold => "Legendary",
                _ => "Common"
            };
        }

        private static void ConvertPNGtoDDS(Context context, string inputPath, string outputDirectory, string errorReference)
        {
            try
            {
                var arguments = new[]
                {
                    "-f",
                    "DXT5",
                    inputPath,
                    "-o",
                    outputDirectory
                };

                var errorBuffer = new string('\0', 512);
                var result = Texconv(
                    arguments.Length,
                    arguments,
                    errorBuffer: errorBuffer,
                    errorBufferSize: errorBuffer.Length
                );

                if (result != 0)
                {
                    context.LogMessage($"[{errorReference} | Error] Failed to convert PNG to DDS: {errorBuffer[..(errorBuffer.IndexOf('\0') - 1)]}");
                }
            }
            catch (Exception ex)
            {
                context.LogMessage($"[{errorReference} | Error] Failed to convert PNG to DDS: {ex}");
            }
        }

        private static void ConvertXMLToLoca(string contents, string xmlPath, string locaPath)
        {
            File.WriteAllText(xmlPath, contents);

            using var xmlFileStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
            using var xmlFile = new LocaXmlReader(xmlFileStream);

            using var locaFileStream = File.OpenWrite(locaPath);
            var locaFile = new LocaWriter(locaFileStream);
            locaFile.Write(xmlFile.Read());
        }

        private static void ConvertLSXToLSF(string contents, string lsxPath, string lsfPath)
        {
            File.WriteAllText(lsxPath, contents);

            using var lsxFileStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
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
