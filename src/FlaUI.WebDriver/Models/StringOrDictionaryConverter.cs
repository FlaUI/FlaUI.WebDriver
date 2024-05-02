using System.Text.Json.Serialization;
using System.Text.Json;

namespace FlaUI.WebDriver.Models;

internal class StringOrDictionaryConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
            return dictionary;
        }
        else
        {
            throw new JsonException("Unexpected JSON token type.");
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}