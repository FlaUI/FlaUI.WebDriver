namespace FlaUI.WebDriver;

/// <summary>
/// An input source is a virtual device providing input events.
/// </summary>
/// <see cref="https://www.w3.org/TR/webdriver2/#input-sources"/>
public class InputSource
{
    protected InputSource(string type) => Type = type;

    public string Type { get; }
}
