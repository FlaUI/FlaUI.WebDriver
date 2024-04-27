using System.Text.Json;

namespace FlaUI.WebDriver.Models
{
    public class Capabilities
    {
        public Dictionary<string, JsonElement>? AlwaysMatch { get; set; }
        public List<Dictionary<string, JsonElement>>? FirstMatch { get; set; }
    }
}
