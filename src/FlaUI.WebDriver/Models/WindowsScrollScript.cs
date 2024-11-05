namespace FlaUI.WebDriver.Models
{
    public class WindowsScrollScript
    {
        public string? ElementId { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? DeltaX { get; set; }
        public int? DeltaY { get; set; }
        public string[]? ModifierKeys { get; set; }
    }
}