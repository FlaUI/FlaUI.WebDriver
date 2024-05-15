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
            var mergedCapabilities = GetMergedCapabilities(request);
            MergedCapabilities? matchedCapabilities = null;
            IEnumerable<MergedCapabilities> matchingCapabilities = mergedCapabilities
                .Where(capabilities => TryMatchCapabilities(capabilities, out matchedCapabilities, out _))
                .Select(capabillities => matchedCapabilities!);

            Core.Application? app;
            var isAppOwnedBySession = false;
            var capabilities = matchingCapabilities.FirstOrDefault();
            if (capabilities == null)
            {
                var mismatchIndications = mergedCapabilities
                    .Select(capabilities => GetMismatchIndication(capabilities));

                return WebDriverResult.Error(new ErrorResponse
                {
                    ErrorCode = "session not created",
                    Message = string.Join("; ", mismatchIndications)
                });
            }
            if (capabilities.TryGetStringCapability("appium:app", out var appPath))
            {
                if (appPath == "Root")
                {
                    app = null;
                }
                else 
                {
                    capabilities.TryGetStringCapability("appium:appArguments", out var appArguments);
                    try
                    {
                        if (appPath.EndsWith("!App"))
                        {
                            app = Core.Application.LaunchStoreApp(appPath, appArguments);
                        }
                        else
                        {
                            var processStartInfo = new ProcessStartInfo(appPath, appArguments ?? "");
                            if(capabilities.TryGetStringCapability("appium:appWorkingDir", out var appWorkingDir))
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
            else if (capabilities.TryGetStringCapability("appium:appTopLevelWindow", out var appTopLevelWindowString))
            {
                Process process = GetProcessByMainWindowHandle(appTopLevelWindowString);
                app = Core.Application.Attach(process);
            }
            else if (capabilities.TryGetStringCapability("appium:appTopLevelWindowTitleMatch", out var appTopLevelWindowTitleMatch))
            {
                Process? process = GetProcessByMainWindowTitle(appTopLevelWindowTitleMatch);
                app = Core.Application.Attach(process);
            }
            else
            {
                throw WebDriverResponseException.InvalidArgument("One of appium:app, appium:appTopLevelWindow or appium:appTopLevelWindowTitleMatch must be passed as a capability");
            }
            var session = new Session(app, isAppOwnedBySession);
            if(capabilities.TryGetNumberCapability("appium:newCommandTimeout", out var newCommandTimeout))
            {
                session.NewCommandTimeout = TimeSpan.FromSeconds(newCommandTimeout);
            }
            if (capabilities.TryGetCapability<TimeoutsConfiguration>("timeouts", out var timeoutsConfiguration))
            {
                session.TimeoutsConfiguration = timeoutsConfiguration!;
            }
            _sessionRepository.Add(session);
            _logger.LogInformation("Created session with ID {SessionId} and capabilities {Capabilities}", session.SessionId, capabilities);
            return await Task.FromResult(WebDriverResult.Success(new CreateSessionResponse()
            {
                SessionId = session.SessionId,
                Capabilities = capabilities.Capabilities
            }));
        }

        private string? GetMismatchIndication(MergedCapabilities capabilities)
        {
            TryMatchCapabilities(capabilities, out _, out var mismatchIndication);
            return mismatchIndication;
        }

        private bool TryMatchCapabilities(MergedCapabilities capabilities, [MaybeNullWhen(false)] out MergedCapabilities matchedCapabilities, [MaybeNullWhen(true)] out string? mismatchIndication)
        {
            matchedCapabilities = new MergedCapabilities(new Dictionary<string, JsonElement>());
            if (capabilities.TryGetStringCapability("platformName", out var platformName)
                && platformName.ToLowerInvariant() == "windows")
            {
                matchedCapabilities.Copy("platformName", capabilities);
            }
            else
            {
                mismatchIndication = "Missing capability 'platformName' with value 'windows'";
                return false;
            }

            if (capabilities.TryGetStringCapability("appium:automationName", out var automationName)
                && automationName.ToLowerInvariant() == "flaui")
            {
                matchedCapabilities.Copy("appium:automationName", capabilities);
            }
            else
            {
                mismatchIndication = "Missing capability 'appium:automationName' with value 'flaui'";
                return false;
            }

            if (capabilities.TryGetStringCapability("appium:app", out var appPath))
            {
                matchedCapabilities.Copy("appium:app", capabilities);

                if (appPath != "Root")
                {
                    if (capabilities.Contains("appium:appArguments"))
                    {
                        matchedCapabilities.Copy("appium:appArguments", capabilities);
                    }
                    if (!appPath.EndsWith("!App"))
                    {
                        if (capabilities.Contains("appium:appWorkingDir"))
                        {
                            matchedCapabilities.Copy("appium:appWorkingDir", capabilities);
                        }
                    }
                }
            }
            else if (capabilities.Contains("appium:appTopLevelWindow"))
            {
                matchedCapabilities.Copy("appium:appTopLevelWindow", capabilities);
            }
            else if (capabilities.Contains("appium:appTopLevelWindowTitleMatch"))
            {   
                matchedCapabilities.Copy("appium:appTopLevelWindowTitleMatch", capabilities);
            }
            else
            {
                mismatchIndication = "One of 'appium:app', 'appium:appTopLevelWindow' or 'appium:appTopLevelWindowTitleMatch' should be specified";
                return false;
            }

            if (capabilities.Contains("appium:newCommandTimeout"))
            {
                matchedCapabilities.Copy("appium:newCommandTimeout", capabilities); ;
            }

            if (capabilities.Contains("timeouts"))
            {
                matchedCapabilities.Copy("timeouts", capabilities);
            }

            var notMatchedCapabilities = capabilities.Capabilities.Keys.Except(matchedCapabilities.Capabilities.Keys);
            if (notMatchedCapabilities.Any())
            {
                mismatchIndication = $"The following capabilities could not be matched: '{string.Join("', '", notMatchedCapabilities)}'";
                return false;
            }

            mismatchIndication = null;
            return true;
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

        private static IEnumerable<MergedCapabilities> GetMergedCapabilities(CreateSessionRequest request)
        {
            var requiredCapabilities = request.Capabilities.AlwaysMatch ?? new Dictionary<string, JsonElement>();
            var allFirstMatchCapabilities = request.Capabilities.FirstMatch ?? new List<Dictionary<string, JsonElement>>(new[] { new Dictionary<string, JsonElement>() });
            return allFirstMatchCapabilities.Select(firstMatchCapabilities => new MergedCapabilities(firstMatchCapabilities, requiredCapabilities));
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