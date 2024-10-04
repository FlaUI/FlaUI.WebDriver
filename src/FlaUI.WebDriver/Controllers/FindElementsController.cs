using FlaUI.WebDriver.Models;
using Microsoft.AspNetCore.Mvc;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.AutomationElements;
using System.Text.RegularExpressions;
using FlaUI.WebDriver.Services;

namespace FlaUI.WebDriver.Controllers
{
    [Route("session/{sessionId}")]
    [ApiController]
    public class FindElementsController : ControllerBase
    {
        private readonly ILogger<FindElementsController> _logger;
        private readonly ISessionRepository _sessionRepository;
        private readonly IConditionParser _conditionParser;

        public FindElementsController(ILogger<FindElementsController> logger, ISessionRepository sessionRepository, IConditionParser conditionParser)
        {
            _logger = logger;
            _sessionRepository = sessionRepository;
            _conditionParser = conditionParser;
        }

        [HttpPost("element")]
        public async Task<ActionResult> FindElement([FromRoute] string sessionId, [FromBody] FindElementRequest findElementRequest)
        {
            var session = GetActiveSession(sessionId);
            return await FindElementFrom(() => session.App == null ? session.Automation.GetDesktop() : session.CurrentWindow, findElementRequest, session);
        }

        [HttpPost("element/{elementId}/element")]
        public async Task<ActionResult> FindElementFromElement([FromRoute] string sessionId, [FromRoute] string elementId, [FromBody] FindElementRequest findElementRequest)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);
            return await FindElementFrom(() => element, findElementRequest, session);
        }

        [HttpPost("elements")]
        public async Task<ActionResult> FindElements([FromRoute] string sessionId, [FromBody] FindElementRequest findElementRequest)
        {
            var session = GetActiveSession(sessionId);
            return await FindElementsFrom(() => session.App == null ? session.Automation.GetDesktop() : session.CurrentWindow, findElementRequest, session);
        }

        [HttpPost("element/{elementId}/elements")]
        public async Task<ActionResult> FindElementsFromElement([FromRoute] string sessionId, [FromRoute] string elementId, [FromBody] FindElementRequest findElementRequest)
        {
            var session = GetActiveSession(sessionId);
            var element = GetElement(session, elementId);
            return await FindElementsFrom(() => element, findElementRequest, session);
        }

        private async Task<ActionResult> FindElementFrom(Func<AutomationElement> startNode, FindElementRequest findElementRequest, Session session)
        {
            AutomationElement? element;
            if (findElementRequest.Using == "xpath") 
            { 
                element = await Wait.Until(() => startNode().FindFirstByXPath(findElementRequest.Value), element => element != null, session.ImplicitWaitTimeout);
            }
            else 
            { 
                var condition = _conditionParser.ParseCondition(session.Automation.ConditionFactory, findElementRequest.Using, findElementRequest.Value);
                element = await Wait.Until(() => startNode().FindFirstDescendant(condition), element => element != null, session.ImplicitWaitTimeout);
            }

            if (element == null)
            {
                return NoSuchElement(findElementRequest);
            }

            var knownElement = session.GetOrAddKnownElement(element);
            return await Task.FromResult(WebDriverResult.Success(new FindElementResponse
            {
                ElementReference = knownElement.ElementReference,
            }));
        }

        private async Task<ActionResult> FindElementsFrom(Func<AutomationElement> startNode, FindElementRequest findElementRequest, Session session)
        {
            AutomationElement[] elements;
            if (findElementRequest.Using == "xpath")
            {
                elements = await Wait.Until(() => startNode().FindAllByXPath(findElementRequest.Value), elements => elements.Length > 0, session.ImplicitWaitTimeout);
            }
            else
            {
                var condition = _conditionParser.ParseCondition(session.Automation.ConditionFactory, findElementRequest.Using, findElementRequest.Value);
                elements = await Wait.Until(() => startNode().FindAllDescendants(condition), elements => elements.Length > 0, session.ImplicitWaitTimeout);
            }

            var knownElements = elements.Select(session.GetOrAddKnownElement);
            return await Task.FromResult(WebDriverResult.Success(

                knownElements.Select(knownElement => new FindElementResponse()
                {
                    ElementReference = knownElement.ElementReference
                }).ToArray()
            ));
        }

        private static ActionResult NoSuchElement(FindElementRequest findElementRequest)
        {
            return WebDriverResult.NotFound(new ErrorResponse()
            {
                ErrorCode = "no such element",
                Message = $"No element found with selector '{findElementRequest.Using}' and value '{findElementRequest.Value}'"
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
