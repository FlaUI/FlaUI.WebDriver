@@ -0,0 +1,139 @@
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
        private const int InitialStringBuilderCapacity = 64 * 1024; // 64KB initial capacity

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
                Encoding = Encoding.Unicode,
                OmitXmlDeclaration = true
            };

            var stringBuilder = new StringBuilder(InitialStringBuilderCapacity);
            using var writer = XmlWriter.Create(stringBuilder, settings);
            WriteElementToXml(writer, element, rootBounds);
            return stringBuilder.ToString();
        }

        private void WriteElementToXml(XmlWriter writer, AutomationElement element, System.Drawing.Rectangle rootBounds)
        {
            var controlType = element.ControlType.ToString();
            writer.WriteStartElement(controlType);

            var properties = element.Properties;
            WriteAttributeIfNotNull(writer, "AutomationId", properties.AutomationId);
            WriteAttributeIfNotNull(writer, "ClassName", properties.ClassName);
            WriteAttributeIfNotNull(writer, "FrameworkId", properties.FrameworkId);
            WriteAttributeIfNotNull(writer, "IsEnabled", properties.IsEnabled);
            WriteAttributeIfNotNull(writer, "Name", properties.Name);

            var bounds = element.BoundingRectangle;
            writer.WriteAttributeString("x", (bounds.X - rootBounds.X).ToString());
            writer.WriteAttributeString("y", (bounds.Y - rootBounds.Y).ToString());
            writer.WriteAttributeString("width", bounds.Width.ToString());
            writer.WriteAttributeString("height", bounds.Height.ToString());

            var patterns = element.Patterns;
            if (patterns.Window.IsSupported)
            {
                var windowPattern = patterns.Window.Pattern;
                WriteAttributeIfNotNull(writer, "CanMaximize", windowPattern.CanMaximize);
                WriteAttributeIfNotNull(writer, "CanMinimize", windowPattern.CanMinimize);
                WriteAttributeIfNotNull(writer, "IsModal", windowPattern.IsModal);
                WriteAttributeIfNotNull(writer, "WindowVisualState", windowPattern.WindowVisualState);
            }

            if (patterns.Scroll.IsSupported)
            {
                var scrollPattern = patterns.Scroll.Pattern;
                WriteAttributeIfNotNull(writer, "HorizontallyScrollable", scrollPattern.HorizontallyScrollable);
                WriteAttributeIfNotNull(writer, "VerticallyScrollable", scrollPattern.VerticallyScrollable);
                WriteAttributeIfNotNull(writer, "HorizontalScrollPercent", scrollPattern.HorizontalScrollPercent);
                WriteAttributeIfNotNull(writer, "VerticalScrollPercent", scrollPattern.VerticalScrollPercent);
            }

            if (patterns.SelectionItem.IsSupported)
            {
                var selectionItemPattern = patterns.SelectionItem.Pattern;
                WriteAttributeIfNotNull(writer, "IsSelected", selectionItemPattern.IsSelected);
            }

            if (patterns.ExpandCollapse.IsSupported)
            {
                WriteAttributeIfNotNull(writer, "ExpandCollapseState", patterns.ExpandCollapse.Pattern.ExpandCollapseState);
            }

            var children = element.FindAllChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    WriteElementToXml(writer, child, rootBounds);
                }
            }

            writer.WriteEndElement();
        }

        private void WriteAttributeIfNotNull<T>(XmlWriter writer, string name, T? value)
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
