using System.Text.Json;

namespace FlaUI.WebDriver.Models
{
    public class ExecuteScriptRequest
    {
        public string Script { get; set; } = null!;
        public List<JsonElement> Args { get; set; } = new List<JsonElement>();
    }
}
