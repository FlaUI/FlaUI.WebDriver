using FlaUI.Core.AutomationElements;

namespace FlaUI.WebDriver
{
    public class KnownWindow
    {
        public KnownWindow(Window window)
        {
            Window = window;
            WindowHandle = Guid.NewGuid().ToString();
        }
        public string WindowHandle { get; set; }
        public Window Window { get; set; }
    }
}
