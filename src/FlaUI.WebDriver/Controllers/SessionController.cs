using FlaUI.WebDriver.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlaUI.WebDriver.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly ISessionRepository _sessionRepository;

        public SessionController(ILogger<SessionController> logger, ISessionRepository sessionRepository)
        {
            _logger = logger;
            _sessionRepository = sessionRepository;
        }

        [HttpPost]
        public async Task<ActionResult> CreateNewSession([FromBody] CreateSessionRequest request)
        {
            var possibleCapabilities = GetPossibleCapabilities(request);
            IDictionary<string, JsonElement>? matchedCapabilities = null;
            IEnumerable<IDictionary<string, JsonElement>> matchingCapabilities = possibleCapabilities
                .Where(capabilities => IsMatchingCapabilitySet(capabilities, out matchedCapabilities))
                .Select(capabillities => matchedCapabilities!);

            Core.Application? app;
            var isAppOwnedBySession = false;
            var capabilities = matchingCapabilities.FirstOrDefault();
            if (capabilities == null)
            {
                return WebDriverResult.Error(new ErrorResponse
                {
                    ErrorCode = "session not created",
                    Message = "Required capabilities did not match. Capability `platformName` with value `windows` is required, capability 'appium:automationName' with value `FlaUI` is required, and one of appium:app, appium:appTopLevelWindow or appium:appTopLevelWindowTitleMatch must be passed as a capability"
                });
            }
            if (TryGetStringCapability(capabilities, "appium:app", out var appPath))
            {
                if (appPath == "Root")
                {
                    app = null;
                }
                else 
                {
                    TryGetStringCapability(capabilities, "appium:appArguments", out var appArguments);
                    try
                    {
                        if (appPath.EndsWith("!App"))
                        {
                            app = Core.Application.LaunchStoreApp(appPath, appArguments);
                        }
                        else
                        {
                            var processStartInfo = new ProcessStartInfo(appPath, appArguments ?? "");
                            if(TryGetStringCapability(capabilities, "appium:appWorkingDir", out var appWorkingDir))
                            {
                                processStartInfo.WorkingDirectory = appWorkingDir;
                            }
                            app = Core.Application.Launch(processStartInfo);
                        }
                    }
                    catch(Exception e)
                    {
                        throw WebDriverResponseException.InvalidArgument($"Starting app '{appPath}' with arguments '{appArguments}' threw an exception: {e.Message}");
                    }
                }

                isAppOwnedBySession = true;
            }
            else if (TryGetStringCapability(capabilities, "appium:appTopLevelWindow", out var appTopLevelWindowString))
            {
                Process process = GetProcessByMainWindowHandle(appTopLevelWindowString);
                app = Core.Application.Attach(process);
            }
            else if (TryGetStringCapability(capabilities, "appium:appTopLevelWindowTitleMatch", out var appTopLevelWindowTitleMatch))
            {
                Process? process = GetProcessByMainWindowTitle(appTopLevelWindowTitleMatch);
                app = Core.Application.Attach(process);
            }
            else
            {
                throw WebDriverResponseException.InvalidArgument("One of appium:app, appium:appTopLevelWindow or appium:appTopLevelWindowTitleMatch must be passed as a capability");
            }
            var session = new Session(app, isAppOwnedBySession);
            if(TryGetNumberCapability(capabilities, "appium:newCommandTimeout", out var newCommandTimeout))
            {
                session.NewCommandTimeout = TimeSpan.FromSeconds(newCommandTimeout);
            }
            if (capabilities.TryGetValue("timeouts", out var valueJson))
            {
                var timeoutsConfiguration = JsonSerializer.Deserialize<TimeoutsConfiguration>(valueJson);
                if (timeoutsConfiguration == null)
                {
                    throw WebDriverResponseException.InvalidArgument("Could not deserialize timeouts capability");
                }
                session.TimeoutsConfiguration = timeoutsConfiguration;
            }
            _sessionRepository.Add(session);
            _logger.LogInformation("Created session with ID {SessionId} and capabilities {Capabilities}", session.SessionId, capabilities);
            return await Task.FromResult(WebDriverResult.Success(new CreateSessionResponse()
            {
                SessionId = session.SessionId,
                Capabilities = capabilities
            }));
        }

        private bool IsMatchingCapabilitySet(IDictionary<string, JsonElement> capabilities, out IDictionary<string, JsonElement> matchedCapabilities)
        {
            matchedCapabilities = new Dictionary<string, JsonElement>();
            if (TryGetStringCapability(capabilities, "platformName", out var platformName)
                && platformName.ToLowerInvariant() == "windows")
            {
                matchedCapabilities.Add("platformName", capabilities["platformName"]);
            }
            else
            {
                return false;
            }

            if (TryGetStringCapability(capabilities, "appium:automationName", out var automationName)
                && automationName.ToLowerInvariant() == "flaui")
            {
                matchedCapabilities.Add("appium:automationName", capabilities["appium:automationName"]);
            }
            else
            {
                return false;
            }

            if (TryGetStringCapability(capabilities, "appium:app", out var appPath))
            {
                matchedCapabilities.Add("appium:app", capabilities["appium:app"]);

                if (appPath != "Root")
                {
                    if(capabilities.ContainsKey("appium:appArguments"))
                    {
                        matchedCapabilities.Add("appium:appArguments", capabilities["appium:appArguments"]);
                    }
                    if (!appPath.EndsWith("!App"))
                    {
                        if (capabilities.ContainsKey("appium:appWorkingDir"))
                        {
                            matchedCapabilities.Add("appium:appWorkingDir", capabilities["appium:appWorkingDir"]);
                        }
                    }
                }
            }
            else if (capabilities.ContainsKey("appium:appTopLevelWindow"))
            {
                matchedCapabilities.Add("appium:appTopLevelWindow", capabilities["appium:appTopLevelWindow"]);
            }
            else if (capabilities.ContainsKey("appium:appTopLevelWindowTitleMatch"))
            {
                matchedCapabilities.Add("appium:appTopLevelWindowTitleMatch", capabilities["appium:appTopLevelWindowTitleMatch"]);
            }
            else
            {
                return false;
            }

            if (capabilities.ContainsKey("appium:newCommandTimeout"))
            {
                matchedCapabilities.Add("appium:newCommandTimeout", capabilities["appium:newCommandTimeout"]); ;
            }

            if (capabilities.ContainsKey("timeouts"))
            {
                matchedCapabilities.Add("timeouts", capabilities["timeouts"]);
            }

            return matchedCapabilities.Count == capabilities.Count;
        }

        private static bool TryGetStringCapability(IDictionary<string, JsonElement> capabilities, string key, [MaybeNullWhen(false)] out string value)
        {
            if(capabilities.TryGetValue(key, out var valueJson))
            {
                if(valueJson.ValueKind != JsonValueKind.String)
                {
                    throw WebDriverResponseException.InvalidArgument($"Capability {key} must be a string");
                }

                value = valueJson.GetString();
                return value != null;
            }

            value = null;
            return false;
        }

        private static bool TryGetNumberCapability(IDictionary<string, JsonElement> capabilities, string key, out double value)
        {
            if (capabilities.TryGetValue(key, out var valueJson))
            {
                if (valueJson.ValueKind != JsonValueKind.Number)
                {
                    throw WebDriverResponseException.InvalidArgument($"Capability {key} must be a number");
                }

                value = valueJson.GetDouble();
                return true;
            }

            value = default;
            return false;
        }

        private static Process GetProcessByMainWindowTitle(string appTopLevelWindowTitleMatch)
        {
            Regex appMainWindowTitleRegex;
            try
            {
                appMainWindowTitleRegex = new Regex(appTopLevelWindowTitleMatch);
            } 
            catch(ArgumentException e)
            {
                throw WebDriverResponseException.InvalidArgument($"Capability appium:appTopLevelWindowTitleMatch '{appTopLevelWindowTitleMatch}' is not a valid regular expression: {e.Message}");
            }
            var processes = Process.GetProcesses().Where(process => appMainWindowTitleRegex.IsMatch(process.MainWindowTitle)).ToArray();
            if (processes.Length == 0)
            {
                throw WebDriverResponseException.InvalidArgument($"Process with main window title matching '{appTopLevelWindowTitleMatch}' could not be found");
            }
            else if (processes.Length > 1)
            {
                throw WebDriverResponseException.InvalidArgument($"Found multiple ({processes.Length}) processes with main window title matching '{appTopLevelWindowTitleMatch}'");
            }
            return processes[0];
        }

        private static Process GetProcessByMainWindowHandle(string appTopLevelWindowString)
        {
            int appTopLevelWindow;
            try
            {
                appTopLevelWindow = Convert.ToInt32(appTopLevelWindowString, 16);
            }
            catch (Exception)
            {
                throw WebDriverResponseException.InvalidArgument($"Capability appium:appTopLevelWindow '{appTopLevelWindowString}' is not a valid hexadecimal string");
            }
            if (appTopLevelWindow == 0)
            {
                throw WebDriverResponseException.InvalidArgument($"Capability appium:appTopLevelWindow '{appTopLevelWindowString}' should not be zero");
            }
            var process = Process.GetProcesses().SingleOrDefault(process => process.MainWindowHandle.ToInt32() == appTopLevelWindow);
            if (process == null)
            {
                throw WebDriverResponseException.InvalidArgument($"Process with main window handle {appTopLevelWindowString} could not be found");
            }
            return process;
        }

        private static IEnumerable<IDictionary<string, JsonElement>> GetPossibleCapabilities(CreateSessionRequest request)
        {
            var requiredCapabilities = request.Capabilities.AlwaysMatch ?? new Dictionary<string, JsonElement>();
            var allFirstMatchCapabilities = request.Capabilities.FirstMatch ?? new List<Dictionary<string, JsonElement>>(new[] { new Dictionary<string, JsonElement>() });
            return allFirstMatchCapabilities.Select(firstMatchCapabilities => MergeCapabilities(firstMatchCapabilities, requiredCapabilities));
        }

        private static IDictionary<string, JsonElement> MergeCapabilities(IDictionary<string, JsonElement> firstMatchCapabilities, IDictionary<string, JsonElement> requiredCapabilities)
        {
            var duplicateKeys = firstMatchCapabilities.Keys.Intersect(requiredCapabilities.Keys);
            if (duplicateKeys.Any())
            {
                throw WebDriverResponseException.InvalidArgument($"Capabilities cannot be merged because there are duplicate capabilities: {string.Join(", ", duplicateKeys)}");
            }

            return firstMatchCapabilities.Concat(requiredCapabilities)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        [HttpDelete("{sessionId}")]
        public async Task<ActionResult> DeleteSession([FromRoute] string sessionId)
        {
            var session = GetSession(sessionId);
            _sessionRepository.Delete(session);
            session.Dispose();
            _logger.LogInformation("Deleted session with ID {SessionId}", sessionId);
            return await Task.FromResult(WebDriverResult.Success());
        }

        [HttpGet("{sessionId}/title")]
        public async Task<ActionResult> GetTitle([FromRoute] string sessionId)
        {
            var session = GetSession(sessionId);
            var title = session.CurrentWindow.Title;
            return await Task.FromResult(WebDriverResult.Success(title));
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