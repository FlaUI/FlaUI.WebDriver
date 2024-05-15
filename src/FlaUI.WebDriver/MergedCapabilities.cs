using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FlaUI.WebDriver
{
    public class MergedCapabilities
    {
        public Dictionary<string, JsonElement> Capabilities { get; }

        public MergedCapabilities(Dictionary<string, JsonElement> firstMatchCapabilities, Dictionary<string, JsonElement> requiredCapabilities) 
            : this(MergeCapabilities(firstMatchCapabilities, requiredCapabilities))
        {
            
        }

        public MergedCapabilities(Dictionary<string, JsonElement> capabilities)
        {
            Capabilities = capabilities;
        }

        private static Dictionary<string, JsonElement> MergeCapabilities(Dictionary<string, JsonElement> firstMatchCapabilities, Dictionary<string, JsonElement> requiredCapabilities)
        {
            var duplicateKeys = firstMatchCapabilities.Keys.Intersect(requiredCapabilities.Keys);
            if (duplicateKeys.Any())
            {
                throw WebDriverResponseException.InvalidArgument($"Capabilities cannot be merged because there are duplicate capabilities: {string.Join(", ", duplicateKeys)}");
            }

            return firstMatchCapabilities.Concat(requiredCapabilities)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public bool TryGetStringCapability(string key, [MaybeNullWhen(false)] out string value)
        {
            if (Capabilities.TryGetValue(key, out var valueJson))
            {
                if (valueJson.ValueKind != JsonValueKind.String)
                {
                    throw WebDriverResponseException.InvalidArgument($"Capability {key} must be a string");
                }

                value = valueJson.GetString();
                return value != null;
            }

            value = null;
            return false;
        }

        public bool TryGetNumberCapability(string key, out double value)
        {
            if (Capabilities.TryGetValue(key, out var valueJson))
            {
                if (valueJson.ValueKind != JsonValueKind.Number)
                {
                    throw WebDriverResponseException.InvalidArgument($"Capability {key} must be a number");
                }

                value = valueJson.GetDouble();
                return true;
            }

            value = default;
            return false;
        }

        public void Copy(string key, MergedCapabilities fromCapabilities)
        {
            Capabilities.Add(key, fromCapabilities.Capabilities[key]);
        }

        public bool Contains(string key)
        {
            return Capabilities.ContainsKey(key);
        }

        public bool TryGetCapability<T>(string key, [MaybeNullWhen(false)] out T? value)
        {
            if (!Capabilities.TryGetValue(key, out var valueJson))
            {
                value = default;
                return false;
            }

            var deserializedValue = JsonSerializer.Deserialize<T>(valueJson);
            if (deserializedValue == null)
            {
                throw WebDriverResponseException.InvalidArgument($"Could not deserialize {key} capability");
            }
            value = deserializedValue;
            return true;
        }
    }
}
