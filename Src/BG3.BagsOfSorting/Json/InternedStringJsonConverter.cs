using System.Text.Json;
using System.Text.Json.Serialization;

namespace BG3.BagsOfSorting.Json
{
    public class InternedStringJsonConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();

            return s == null ? null : string.IsInterned(s) ?? string.Intern(s);
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
