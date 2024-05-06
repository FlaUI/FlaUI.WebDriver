using FlaUI.WebDriver.Models;
using FlaUI.WebDriver.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlaUI.WebDriver.Controllers
{
    [Route("session/{sessionId}/[controller]")]
    [ApiController]
    public class ActionsController : ControllerBase
    {
        private readonly ILogger<ActionsController> _logger;
        private readonly ISessionRepository _sessionRepository;

        public ActionsController(ILogger<ActionsController> logger, ISessionRepository sessionRepository)
        {
            _logger = logger;
            _sessionRepository = sessionRepository;
        }

        [HttpPost]
        public async Task<ActionResult> PerformActions([FromRoute] string sessionId, [FromBody] ActionsRequest actionsRequest)
        {
            var session = GetSession(sessionId);
            var actionsByTick = ExtractActionSequence(session, actionsRequest);
            foreach (var tickActions in actionsByTick)
            {
                var tickDuration = tickActions.Max(tickAction => tickAction.Duration) ?? 0;
                var dispatchTickActionTasks = tickActions.Select(tickAction => ActionsDispatcher.DispatchAction(session, tickAction));
                if (tickDuration > 0)
                {
                    dispatchTickActionTasks = dispatchTickActionTasks.Concat(new[] { Task.Delay(tickDuration) });
                }
                await Task.WhenAll(dispatchTickActionTasks);
            }

            // The spec says that input sources must be created for actions, but then doesn't specify how or when they should be
            // removed. Guessing that it should be here and like this?
            // https://github.com/w3c/webdriver/issues/1810#issuecomment-2095912170
            foreach (var tickAction in actionsByTick)
            {
                foreach (var action in tickAction)
                {
                    await ActionsDispatcher.DispatchReleaseActions(session, action.Id);
                    session.InputState.RemoveInputSource(action.Id);
                }
            }

            return WebDriverResult.Success();
        }

        [HttpDelete]
        public async Task<ActionResult> ReleaseActions([FromRoute] string sessionId)
        {
            var session = GetSession(sessionId);

            foreach (var cancelAction in session.InputState.InputCancelList)
            {
                await ActionsDispatcher.DispatchAction(session, cancelAction);
            }
            session.InputState.Reset();

            return WebDriverResult.Success();
        }

        /// <summary>
        /// See https://www.w3.org/TR/webdriver2/#dfn-extract-an-action-sequence.
        /// Returns all sequence actions synchronized by index.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="actionsRequest">The request</param>
        /// <returns></returns>
        private static List<List<Action>> ExtractActionSequence(Session session, ActionsRequest actionsRequest)
        {
            var actionsByTick = new List<List<Action>>();
            foreach (var actionSequence in actionsRequest.Actions)
            {
                // TODO: Implement other input source types.
                if (actionSequence.Type == "key")
                {
                    var source = session.InputState.GetOrCreateInputSource(actionSequence.Type, actionSequence.Id);

                    // The spec says that input sources must be created for actions and they are later expected to be
                    // found in the input source map, but doesn't specify what should add them. Guessing that it should
                    // be done here. https://github.com/w3c/webdriver/issues/1810
                    session.InputState.AddInputSource(actionSequence.Id, source);
                }

                for (var tickIndex = 0; tickIndex < actionSequence.Actions.Count; tickIndex++)
                {
                    var actionItem = actionSequence.Actions[tickIndex];
                    var action = new Action(actionSequence, actionItem);
                    if (actionsByTick.Count < tickIndex + 1)
                    {
                        actionsByTick.Add(new List<Action>());
                    }
                    actionsByTick[tickIndex].Add(action);
                }
            }
            return actionsByTick;
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
