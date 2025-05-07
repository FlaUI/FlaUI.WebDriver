@@ -0,0 +1,162 @@
using FlaUI.WebDriver.Models;
using Microsoft.AspNetCore.Mvc;
using FlaUI.Core.AutomationElements;
using FlaUI.WebDriver.Services;
using System.Xml;
using System.Text;

namespace FlaUI.WebDriver.Controllers
{
    [ApiController]
    [Route("session/{sessionId}/[controller]")]
    public class SourceController : ControllerBase
    {
        private readonly ILogger<SourceController> _logger;
        private readonly ISessionRepository _sessionRepository;

        public SourceController(ILogger<SourceController> logger, ISessionRepository sessionRepository)
        {
            _logger = logger;
            _sessionRepository = sessionRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetPageSource([FromRoute] string sessionId)
        {
            var session = GetActiveSession(sessionId);
            var rootElement = session.App == null ? session.Automation.GetDesktop() : session.CurrentWindow;
            var rootBounds = rootElement.BoundingRectangle;
            var xml = ConvertElementToXml(rootElement, rootBounds);
            return await Task.FromResult(WebDriverResult.Success(xml));
        }

        private string ConvertElementToXml(AutomationElement element, System.Drawing.Rectangle rootBounds)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.Unicode
            };

            var stringBuilder = new StringBuilder();
            using (var writer = XmlWriter.Create(stringBuilder, settings))
            {
                writer.WriteStartDocument();
                WriteElementToXml(writer, element, rootBounds);
                writer.WriteEndDocument();
            }

            return stringBuilder.ToString();
        }

        private void WriteElementToXml(XmlWriter writer, AutomationElement element, System.Drawing.Rectangle rootBounds)
        {
            var controlType = element.ControlType.ToString();
            writer.WriteStartElement(controlType);

            WritePropertyAttribute(writer, "AcceleratorKey", element.Properties.AcceleratorKey);
            WritePropertyAttribute(writer, "AccessKey", element.Properties.AccessKey);
            WritePropertyAttribute(writer, "AutomationId", element.Properties.AutomationId);
            WritePropertyAttribute(writer, "ClassName", element.Properties.ClassName);
            WritePropertyAttribute(writer, "FrameworkId", element.Properties.FrameworkId);
            WritePropertyAttribute(writer, "HasKeyboardFocus", element.Properties.HasKeyboardFocus);
            WritePropertyAttribute(writer, "HelpText", element.Properties.HelpText);
            WritePropertyAttribute(writer, "IsContentElement", element.Properties.IsContentElement);
            WritePropertyAttribute(writer, "IsControlElement", element.Properties.IsControlElement);
            WritePropertyAttribute(writer, "IsEnabled", element.Properties.IsEnabled);
            WritePropertyAttribute(writer, "IsKeyboardFocusable", element.Properties.IsKeyboardFocusable);
            WritePropertyAttribute(writer, "IsOffscreen", element.Properties.IsOffscreen);
            WritePropertyAttribute(writer, "IsPassword", element.Properties.IsPassword);
            WritePropertyAttribute(writer, "IsRequiredForForm", element.Properties.IsRequiredForForm);
            WritePropertyAttribute(writer, "ItemStatus", element.Properties.ItemStatus);
            WritePropertyAttribute(writer, "ItemType", element.Properties.ItemType);
            WritePropertyAttribute(writer, "LocalizedControlType", element.Properties.LocalizedControlType);
            WritePropertyAttribute(writer, "Name", element.Properties.Name);
            WritePropertyAttribute(writer, "Orientation", element.Properties.Orientation);
            WritePropertyAttribute(writer, "ProcessId", element.Properties.ProcessId);
            WritePropertyAttribute(writer, "RuntimeId", element.Properties.RuntimeId);

            var bounds = element.BoundingRectangle;
            writer.WriteAttributeString("x", (bounds.X - rootBounds.X).ToString());
            writer.WriteAttributeString("y", (bounds.Y - rootBounds.Y).ToString());
            writer.WriteAttributeString("width", bounds.Width.ToString());
            writer.WriteAttributeString("height", bounds.Height.ToString());

            if (element.Patterns.Window.IsSupported)
            {
                var windowPattern = element.Patterns.Window.Pattern;
                WritePropertyAttribute(writer, "CanMaximize", windowPattern.CanMaximize);
                WritePropertyAttribute(writer, "CanMinimize", windowPattern.CanMinimize);
                WritePropertyAttribute(writer, "IsModal", windowPattern.IsModal);
                WritePropertyAttribute(writer, "WindowVisualState", windowPattern.WindowVisualState);
                WritePropertyAttribute(writer, "WindowInteractionState", windowPattern.WindowInteractionState);
                WritePropertyAttribute(writer, "IsTopmost", windowPattern.IsTopmost);
            }

            if (element.Patterns.Scroll.IsSupported)
            {
                var scrollPattern = element.Patterns.Scroll.Pattern;
                WritePropertyAttribute(writer, "HorizontallyScrollable", scrollPattern.HorizontallyScrollable);
                WritePropertyAttribute(writer, "VerticallyScrollable", scrollPattern.VerticallyScrollable);
                WritePropertyAttribute(writer, "HorizontalScrollPercent", scrollPattern.HorizontalScrollPercent);
                WritePropertyAttribute(writer, "VerticalScrollPercent", scrollPattern.VerticalScrollPercent);
                WritePropertyAttribute(writer, "HorizontalViewSize", scrollPattern.HorizontalViewSize);
                WritePropertyAttribute(writer, "VerticalViewSize", scrollPattern.VerticalViewSize);
            }

            if (element.Patterns.SelectionItem.IsSupported)
            {
                var selectionItemPattern = element.Patterns.SelectionItem.Pattern;
                WritePropertyAttribute(writer, "IsSelected", selectionItemPattern.IsSelected);
                if (selectionItemPattern.SelectionContainer != null)
                {
                    WritePropertyAttribute(writer, "SelectionContainer", selectionItemPattern.SelectionContainer.ToString());
                }
            }

            if (element.Patterns.ExpandCollapse.IsSupported)
            {
                WritePropertyAttribute(writer, "ExpandCollapseState", element.Patterns.ExpandCollapse.Pattern.ExpandCollapseState);
            }

            WritePropertyAttribute(writer, "IsAvailable", element.IsAvailable);

            foreach (var child in element.FindAllChildren())
            {
                WriteElementToXml(writer, child, rootBounds);
            }

            writer.WriteEndElement();
        }

        private void WritePropertyAttribute<T>(XmlWriter writer, string name, T value)
        {
            if (value != null)
            {
                writer.WriteAttributeString(name, value.ToString());
            }
        }

        private Session GetActiveSession(string sessionId)
        {
            var session = GetSession(sessionId);
            if (session.App != null && session.App.HasExited)
            {
                throw WebDriverResponseException.NoWindowsOpenForSession();
            }
            return session;
        }

        private Session GetSession(string sessionId)
        {
            var session = _sessionRepository.FindById(sessionId);
            if (session == null)
            {
                throw WebDriverResponseException.SessionNotFound(sessionId);
            }
            session.SetLastCommandTimeToNow();
            return session;
        }
    }
}
