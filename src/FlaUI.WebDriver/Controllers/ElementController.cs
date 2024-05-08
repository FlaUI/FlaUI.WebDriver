using System.Text;
using FlaUI.Core.AutomationElements;
using FlaUI.WebDriver.Models;
using FlaUI.WebDriver.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlaUI.WebDriver.Controllers
{
    [Route("session/{sessionId}/[controller]")]
    [ApiController]
    public class ElementController : ControllerBase
    {
        private readonly ILogger<ElementController> _logger;
        private readonly ISessionRepository _sessionRepository;
        private readonly IActionsDispatcher _actionsDispatcher;

        public ElementController(ILogger<ElementController> logger, ISessionRepository sessionRepository, IActionsDispatcher actionsDispatcher)
        {
            _logger = logger;
            _sessionRepository = sessionRepository;
            _actionsDispatcher = actionsDispatcher;
        }

        [HttpGet("active")]
        public async Task<ActionResult> GetActiveElement([FromRoute] string sessionId)
        {
            var session = GetActiveSession(sessionId);
            var element = session.GetOrAddKnownElement(session.Automation.FocusedElement());
            return await Task.FromResult(WebDriverResult.Success(new FindElementResponse()
            {
                ElementReference = element.ElementReference
            }));
        }

        [HttpGet("{elementId}/displayed")]
        public async Task<ActionResult> IsElementDisplayed([FromRoute] string sessionId, [FromRoute] string elementId)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);
            return await Task.FromResult(WebDriverResult.Success(!element.IsOffscreen));
        }

        [HttpGet("{elementId}/enabled")]
        public async Task<ActionResult> IsElementEnabled([FromRoute] string sessionId, [FromRoute] string elementId)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);
            return await Task.FromResult(WebDriverResult.Success(element.IsEnabled));
        }

        [HttpGet("{elementId}/name")]
        public async Task<ActionResult> GetElementTagName([FromRoute] string sessionId, [FromRoute] string elementId)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);
            return await Task.FromResult(WebDriverResult.Success(element.ControlType));
        }

        [HttpPost("{elementId}/click")]
        public async Task<ActionResult> ElementClick([FromRoute] string sessionId, [FromRoute] string elementId)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);

            ScrollElementContainerIntoView(element);
            if (!await Wait.Until(() => !element.IsOffscreen, session.ImplicitWaitTimeout))
            {
                return ElementNotInteractable(elementId);
            }
            element.Click();

            return WebDriverResult.Success();
        }

        [HttpPost("{elementId}/clear")]
        public async Task<ActionResult> ElementClear([FromRoute] string sessionId, [FromRoute] string elementId)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);

            element.AsTextBox().Text = "";

            return await Task.FromResult(WebDriverResult.Success());
        }

        [HttpGet("{elementId}/text")]
        public async Task<ActionResult> GetElementText([FromRoute] string sessionId, [FromRoute] string elementId)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);
            var text = GetElementText(element);

            return await Task.FromResult(WebDriverResult.Success(text));
        }

        private static string GetElementText(AutomationElement element)
        {
            // https://www.w3.org/TR/webdriver2/#get-element-text says about this:
            // 
            // > Let rendered text be the result of performing implementation-specific steps whose result is exactly
            // > the same as the result of a Function.[[Call]](null, element) with bot.dom.getVisibleText as the this value.
            //
            // Because it's implementation-defined, this method tries to follow WinAppDriver's implementation as closely as
            // possible.
            if (element.Patterns.Text.IsSupported)
            {
                return element.Patterns.Text.Pattern.DocumentRange.GetText(int.MaxValue);
            }
            else if (element.Patterns.Value.IsSupported)
            {
                return element.Patterns.Value.Pattern.Value.ToString();
            }
            else if (element.Patterns.RangeValue.IsSupported)
            {
                return element.Patterns.RangeValue.Pattern.Value.ToString();
            }
            else if (element.Patterns.Selection.IsSupported)
            {
                var selected = element.Patterns.Selection.Pattern.Selection.Value;
                return string.Join(", ", selected.Select(GetElementText));
            }
            else
            {
                return GetRenderedText(element);
            }
        }

        private static string GetRenderedText(AutomationElement element)
        {
            var result = new StringBuilder();
            AddRenderedText(element, result);
            return result.ToString();
        }

        private static void AddRenderedText(AutomationElement element, StringBuilder stringBuilder)
        {
            if (!string.IsNullOrWhiteSpace(element.Name))
            {
                if(stringBuilder.Length > 0)
                {
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(element.Name);
            }
            foreach (var child in element.FindAllChildren())
            {
                if (child.Properties.ClassName.ValueOrDefault == "TextBlock")
                {
                    // Text blocks set the `Name` of their parent element already
                    continue;
                }
                AddRenderedText(child, stringBuilder);
            }
        }

        [HttpGet("{elementId}/selected")]
        public async Task<ActionResult> IsElementSelected([FromRoute] string sessionId, [FromRoute] string elementId)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);
            var isSelected = false;
            if (element.Patterns.SelectionItem.IsSupported)
            {
                isSelected = element.Patterns.SelectionItem.PatternOrDefault.IsSelected.ValueOrDefault;
            }
            else if (element.Patterns.Toggle.IsSupported)
            {
                isSelected = element.Patterns.Toggle.PatternOrDefault.ToggleState.ValueOrDefault == Core.Definitions.ToggleState.On;
            }
            return await Task.FromResult(WebDriverResult.Success(isSelected));
        }

        [HttpPost("{elementId}/value")]
        public async Task<ActionResult> ElementSendKeys([FromRoute] string sessionId, [FromRoute] string elementId, [FromBody] ElementSendKeysRequest elementSendKeysRequest)
        {
            _logger.LogDebug("Element send keys for session {SessionId} and element {ElementId}", sessionId, elementId);

            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);

            ScrollElementContainerIntoView(element);
            if (!await Wait.Until(() => !element.IsOffscreen, session.ImplicitWaitTimeout))
            {
                return ElementNotInteractable(elementId);
            }

            element.Focus();

            // Warning: Deviation from the spec. https://www.w3.org/TR/webdriver2/#element-send-keys says:
            //
            // > Set the text insertion caret using set selection range using current text length for both the start and end parameters.
            //
            // In English: "the caret should be placed at the end of the text before sending keys". That doesn't seem to be possible
            // with UIA, meaning that the text gets inserted at the beginning, which is also WinAppDriver's behavior.

            var inputState = session.InputState;
            var inputId = Guid.NewGuid().ToString();
            var source = (KeyInputSource)inputState.CreateInputSource("key");

            inputState.AddInputSource(inputId, source);

            try
            {
                await _actionsDispatcher.DispatchActionsForString(session, inputId, source, elementSendKeysRequest.Text);
            }
            finally
            {
                inputState.RemoveInputSource(inputId);
            }

            return WebDriverResult.Success();
        }

        [HttpGet("{elementId}/attribute/{attributeId}")]
        public async Task<ActionResult> GetAttribute([FromRoute] string sessionId, [FromRoute] string elementId, [FromRoute] string attributeId)
        {
            var session = GetSession(sessionId);
            var element = GetElement(session, elementId);
            var library = element.FrameworkAutomationElement.PropertyIdLibrary;
            var periodIndex = attributeId.IndexOf('.');
            object? value = null;

            if (periodIndex >= 0)
            {
                var patternName = attributeId.Substring(0, periodIndex);
                var propertyName = attributeId.Substring(periodIndex + 1);

                if (element.TryGetPattern(patternName, out var pattern))
                {
                    pattern.TryGetProperty(propertyName, out value);
                }
            }
            else
            {
                element.TryGetProperty(attributeId, out value);
            }

            return await Task.FromResult(WebDriverResult.Success(value?.ToString()));
        }

        [HttpGet("{elementId}/rect")]
        public async Task<ActionResult> GetElementRect([FromRoute] string sessionId, [FromRoute] string elementId)
        {
            var session = GetSession(sessionId);
            var element = GetElement(session, elementId);
            var elementBoundingRect = element.BoundingRectangle;
            var elementRect = new ElementRect
            {
                X = elementBoundingRect.X,
                Y = elementBoundingRect.Y,
                Width = elementBoundingRect.Width,
                Height = elementBoundingRect.Height
            };
            return await Task.FromResult(WebDriverResult.Success(elementRect));
        }

        private static void ScrollElementContainerIntoView(AutomationElement element)
        {
            element.Patterns.ScrollItem.PatternOrDefault?.ScrollIntoView();
        }

        private static ActionResult ElementNotInteractable(string elementId)
        {
            return WebDriverResult.BadRequest(new ErrorResponse()
            {
                ErrorCode = "element not interactable",
                Message = $"Element with ID {elementId} is off screen"
            });
        }

        private AutomationElement GetElement(Session session, string elementId)
        {
            var element = session.FindKnownElementById(elementId);
            if (element == null)
            {
                throw WebDriverResponseException.ElementNotFound(elementId);
            }
            return element;
        }

        private Session GetActiveSession(string sessionId)
        {
            var session = GetSession(sessionId);
            if (session.App == null || session.App.HasExited)
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
