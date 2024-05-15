using System.Text.Json.Serialization;

namespace FlaUI.WebDriver
{
    public class TimeoutsConfiguration
    {
        [JsonPropertyName("script")]
        public double? ScriptTimeoutMs { get; set; } = 30000;
        [JsonPropertyName("pageLoad")]
        public double PageLoadTimeoutMs { get; set; } = 300000;
        [JsonPropertyName("implicit")]
        public double ImplicitWaitTimeoutMs { get; set; } = 0;
    }
}
