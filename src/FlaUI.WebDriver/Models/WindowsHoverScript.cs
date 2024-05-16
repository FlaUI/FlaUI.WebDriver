namespace FlaUI.WebDriver.Models
{
    public class WindowsHoverScript
    {
        public string? StartElementId { get; set; }
        public int? StartX { get; set; }
        public int? StartY { get; set; }
        public int? EndX { get; set; }
        public int? EndY { get; set; }
        public string? EndElementId { get; set; }
        public int? DurationMs { get; set; }
    }
}