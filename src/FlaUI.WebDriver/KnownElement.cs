using FlaUI.Core.AutomationElements;

namespace FlaUI.WebDriver
{
    public class KnownElement
    {
        public KnownElement(AutomationElement element, string? elementRuntimeId)
        {
            Element = element;
            ElementRuntimeId = elementRuntimeId;
            ElementReference = Guid.NewGuid().ToString();
        }

        public string ElementReference { get; }

        /// <summary>
        /// A temporarily unique ID, so cannot be used for identity over time, but can be used for improving performance of equality tests.
        /// "The identifier is only guaranteed to be unique to the UI of the desktop on which it was generated. Identifiers can be reused over time."
        /// </summary>
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/uiautomationclient/nf-uiautomationclient-iuiautomationelement-getruntimeid"/>
        public string? ElementRuntimeId { get; }

        public AutomationElement Element { get; }
    }
}
