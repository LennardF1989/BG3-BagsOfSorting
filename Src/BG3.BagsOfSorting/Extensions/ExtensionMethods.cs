#pragma warning disable S1168

using LSLib.LS;
using System.IO;

namespace BG3.BagsOfSorting.Extensions
{
    public static class ExtensionMethods
    {
        public static T Get<T>(this IReadOnlyDictionary<string, T> dictionary, string key)
        {
            return dictionary.TryGetValue(key, out var result) ? result : default;
        }

        public static List<T> Get<T>(this IReadOnlyDictionary<string, List<T>> dictionary, string key)
        {
            return dictionary.TryGetValue(key, out var result) ? result : default;
        }

        public static void GetAndAdd<T>(this IDictionary<string, List<T>> dictionary, string key, T value)
        {
            if (!dictionary.TryGetValue(key, out var result))
            {
                dictionary[key] = result = new List<T>();
            }

            result.Add(value);
        }

        public static string StringOrNull(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        public static Resource ReadResource(this AbstractFileInfo abstractFileInfo)
        {
            using var stream = abstractFileInfo.MakeStream();

            Resource resource = null;

            if (Path.GetExtension(abstractFileInfo.Name) == ".lsf")
            {
                using var lsfReader = new LSFReader(stream);
                resource = lsfReader.Read();
            }
            else if (Path.GetExtension(abstractFileInfo.Name) == ".lsx")
            {
                using var lsxReader = new LSXReader(stream);
                resource = lsxReader.Read();
            }

            abstractFileInfo.ReleaseStream();

            return resource;
        }

        public static void Copy(this DirectoryInfo self, DirectoryInfo destination, bool recursively)
        {
            foreach (var file in self.GetFiles())
            {
                file.CopyTo(Path.Combine(destination.FullName, file.Name));
            }

            if (!recursively)
            {
                return;
            }

            foreach (var directory in self.GetDirectories())
            {
                directory.Copy(destination.CreateSubdirectory(directory.Name), true);
            }
        }
    }
}
