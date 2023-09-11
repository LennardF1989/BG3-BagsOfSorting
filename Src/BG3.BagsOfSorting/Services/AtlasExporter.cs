#pragma warning disable S3260

using System.IO;
using LSLib.LS;
using BG3.BagsOfSorting.Extensions;
using BG3.BagsOfSorting.Models;
using Pfim;
using Package = LSLib.LS.Package;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace BG3.BagsOfSorting.Services
{
    public static class AtlasExporter
    {
        private class Atlas
        {
            public class Icon
            {
                public string MapKey { get; init; }
                public int X { get; init; }
                public int Y { get; init; }
            }

            public int IconWidth { get; init; }
            public int IconHeight { get; init; }
            public string TextureFile { get; init; }

            public List<Icon> Icons { get; init; }
        }

        private record TextureBank(string ID, string Template, string Texture);

        public static void Export(Context context)
        {
            if (context.Configuration.PAKPaths == null || !context.Configuration.PAKPaths.Any())
            {
                context.LogMessage("[Warning] No paths containing PAKs have been configured.");

                return;
            }

            var packages = context.Configuration.PAKPaths
                .SelectMany(GetPackages)
                .ToList();

            //NOTE: Respect load order and only get the latest version of each file
            var abstractFileInfos = packages
                .SelectMany(x => 
                    x.package.Files.Select(y => new { x, y })
                )
                .GroupBy(x => x.y.Name)
                .Select(x => x
                    .OrderByDescending(y => y.x.package.Metadata.Priority)
                    .First()
                    .y
                )
                .ToList();

            var textureBanks = GetTextureBankTemplates(abstractFileInfos)
                .SelectMany(x => x)
                .ToHashSet();

            var textureAtlases = GetTextureAtlases(abstractFileInfos, textureBanks)
                .ToList();

            ExtractTextures(abstractFileInfos, textureAtlases);

            packages.ForEach(x => x.packageReader.Dispose());
        }

        private static IEnumerable<(PackageReader packageReader, Package package)> GetPackages(string path)
        {
            var files = Directory.GetFiles(path, "*.pak", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                //NOTE: Don't expose the PackageReader, yet...
                var packageReader = new PackageReader(file);

                Package package;

                try
                {
                    package = packageReader.Read();
                }
                catch
                {
                    continue;
                }

                yield return (packageReader, package);
            }
        }

        private static IEnumerable<List<TextureBank>> GetTextureBankTemplates(IEnumerable<AbstractFileInfo> abstractFileInfos)
        {
            var potentialTextureBankFiles = abstractFileInfos
                .Where(x => x.Name.Contains("/[PAK]_UI/"));

            foreach (var abstractFileInfo in potentialTextureBankFiles)
            {
                using var stream = abstractFileInfo.MakeStream();

                var resource = abstractFileInfo.ReadResource();

                var templates = resource?.Regions
                    ?.Get("TextureBank")?.Children
                    ?.Get("Resource")
                    ?.Select(x => new TextureBank(
                        x.Attributes["ID"].Value.ToString()!,
                        x.Attributes["Template"].Value.ToString()!,
                        x.Attributes["SourceFile"].Value.ToString()!
                    ))
                    .ToList() ?? new List<TextureBank>();

                yield return templates;
            }
        }

        private static IEnumerable<Atlas> GetTextureAtlases(IEnumerable<AbstractFileInfo> abstractFileInfos, IReadOnlyCollection<TextureBank> textureBanks)
        {
            foreach (var abstractFileInfo in abstractFileInfos)
            {
                var potentialTextureBanks = textureBanks
                    .Where(x => abstractFileInfo.Name.Contains($"{x.Template}."))
                    .ToList();

                if (!potentialTextureBanks.Any())
                {
                    continue;
                }

                using var stream = abstractFileInfo.MakeStream();

                var resource = abstractFileInfo.ReadResource();

                if (resource == null)
                {
                    continue;
                }

                var atlasInfo = resource.Regions?.Get("TextureAtlasInfo");
                var iconList = resource.Regions?.Get("IconUVList");

                if (atlasInfo == null || iconList == null)
                {
                    continue;
                }

                var iconNodes = iconList.Children?.Get("IconUV");
                var textureSize = atlasInfo.Children?.Get("TextureAtlasTextureSize")?.Single();
                var iconSize = atlasInfo.Children?.Get("TextureAtlasIconSize")?.Single();
                var path = atlasInfo.Children?.Get("TextureAtlasPath")?.Single();

                if (iconNodes == null || textureSize == null || iconSize == null || path == null)
                {
                    continue;
                }

                var uuid = path.Attributes["UUID"].Value.ToString();

                var textureBank = potentialTextureBanks.SingleOrDefault(x => x.ID == uuid);

                if (textureBank == null)
                {
                    continue;
                }

                var iconWidth = (int)Convert.ChangeType(iconSize.Attributes["Width"].Value, TypeCode.Int32)!;
                var iconHeight = (int)Convert.ChangeType(iconSize.Attributes["Height"].Value, TypeCode.Int32)!;

                if (iconWidth != Constants.ICON_SIZE || iconHeight != Constants.ICON_SIZE)
                {
                    continue;
                }

                var textureWidth = (int)Convert.ChangeType(textureSize.Attributes["Width"].Value, TypeCode.Int32)!;
                var textureHeight = (int)Convert.ChangeType(textureSize.Attributes["Height"].Value, TypeCode.Int32)!;

                var icons = iconNodes
                    .Select(x =>
                    {
                        var u1 = (double)Convert.ChangeType(x.Attributes["U1"].Value, TypeCode.Double)!;
                        var v1 = (double)Convert.ChangeType(x.Attributes["V1"].Value, TypeCode.Double)!;

                        var x1 = (int)(u1 * textureWidth);
                        var y1 = (int)(v1 * textureHeight);

                        return new Atlas.Icon
                        {
                            MapKey = x.Attributes["MapKey"].Value.ToString(),
                            X = x1,
                            Y = y1
                        };
                    })
                    .ToList();

                var atlas = new Atlas
                {
                    Icons = icons,
                    IconWidth = iconWidth,
                    IconHeight = iconHeight,
                    TextureFile = textureBank.Texture
                };

                yield return atlas;
            }
        }

        private static void ExtractTextures(List<AbstractFileInfo> abstractFileInfos, IReadOnlyCollection<Atlas> textureAtlases)
        {
            Directory.CreateDirectory(Constants.ICONS_OUTPUT_PATH);

            foreach (var abstractFileInfo in abstractFileInfos)
            {
                var atlas = textureAtlases.SingleOrDefault(x => abstractFileInfo.Name.Contains(x.TextureFile));

                if (atlas == null)
                {
                    continue;
                }

                using var stream = abstractFileInfo.MakeStream();

                using var textureFile = Pfimage.FromStream(stream);

                foreach (var atlasIcon in atlas.Icons)
                {
                    using var result = Image.LoadPixelData<Bgra32>(textureFile.Data, textureFile.Width, textureFile.Height);
                    result.Mutate(x => x.Crop(new Rectangle(atlasIcon.X, atlasIcon.Y, atlas.IconWidth, atlas.IconHeight)));
                    result.SaveAsPng(Path.Combine(Constants.ICONS_OUTPUT_PATH, $"{atlasIcon.MapKey}.png"));
                }
            }
        }
    }
}
