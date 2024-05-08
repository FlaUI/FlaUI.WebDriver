namespace FlaUI.WebDriver;

/// <summary>
/// A key input source is an input source that is associated with a keyboard-type device.
/// </summary>
/// <see cref="https://www.w3.org/TR/webdriver2/#key-input-source"/>
public class KeyInputSource() : InputSource("key")
{
    public HashSet<string> Pressed = [];

    public bool Alt { get; set; }
    public bool Ctrl{ get; set; }
    public bool Meta { get; set; }
    public bool Shift { get; set; }

    public void Reset()
    {
        Pressed.Clear();
        Alt = false;
        Ctrl = false;
        Meta = false;
        Shift = false;
    }
}