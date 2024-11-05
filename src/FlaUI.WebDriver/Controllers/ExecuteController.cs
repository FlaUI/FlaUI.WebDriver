using FlaUI.WebDriver.Models;
using FlaUI.WebDriver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace FlaUI.WebDriver.Controllers
{
    [Route("session/{sessionId}/[controller]")]
    [ApiController]
    public class ExecuteController : ControllerBase
    {
        private readonly ILogger<ExecuteController> _logger;
        private readonly ISessionRepository _sessionRepository;
        private readonly IWindowsExtensionService _windowsExtensionService;

        public ExecuteController(ISessionRepository sessionRepository, IWindowsExtensionService windowsExtensionService, ILogger<ExecuteController> logger)
        {
            _sessionRepository = sessionRepository;
            _windowsExtensionService = windowsExtensionService;
            _logger = logger;
        }

        [HttpPost("sync")]
        public async Task<ActionResult> ExecuteScript([FromRoute] string sessionId, [FromBody] ExecuteScriptRequest executeScriptRequest)
        {
            var session = GetSession(sessionId);
            switch (executeScriptRequest.Script)
            {
                case "powerShell":
                    return await ExecutePowerShellScript(session, executeScriptRequest);
                case "windows: keys":
                    return await ExecuteWindowsKeysScript(session, executeScriptRequest);
                case "windows: click":
                    return await ExecuteWindowsClickScript(session, executeScriptRequest);
                case "windows: hover":
                    return await ExecuteWindowsHoverScript(session, executeScriptRequest);
                case "windows: scroll":
                    return await ExecuteWindowsScrollScript(session, executeScriptRequest);
                default:
                    throw WebDriverResponseException.UnsupportedOperation("Only 'powerShell', 'windows: keys', 'windows: click', 'windows: hover' scripts are supported");
            }
        }

        private async Task<ActionResult> ExecutePowerShellScript(Session session, ExecuteScriptRequest executeScriptRequest)
        {
            if (executeScriptRequest.Args.Count != 1)
            {
                throw WebDriverResponseException.InvalidArgument($"Expected an array of exactly 1 arguments for the PowerShell script, but got {executeScriptRequest.Args.Count} arguments");
            }
            var powerShellArgs = executeScriptRequest.Args[0];
            if (!powerShellArgs.TryGetProperty("command", out var powerShellCommandJson))
            {
                throw WebDriverResponseException.InvalidArgument("Expected a \"command\" property of the first argument for the PowerShell script");
            }
            if (powerShellCommandJson.ValueKind != JsonValueKind.String)
            {
                throw WebDriverResponseException.InvalidArgument($"Powershell \"command\" property must be a string");
            }
            string? powerShellCommand = powerShellCommandJson.GetString();
            if (string.IsNullOrEmpty(powerShellCommand))
            {
                throw WebDriverResponseException.InvalidArgument($"Powershell \"command\" property must be non-empty");
            }

            _logger.LogInformation("Executing PowerShell command {Command} (session {SessionId})", powerShellCommand, session.SessionId);

            var processStartInfo = new ProcessStartInfo("powershell.exe", $"-Command \"{powerShellCommand.Replace("\"", "\\\"")}\"")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            using var process = Process.Start(processStartInfo);
            using var cancellationTokenSource = new CancellationTokenSource();
            if (session.ScriptTimeout.HasValue)
            {
                cancellationTokenSource.CancelAfter(session.ScriptTimeout.Value);
            }
            await process!.WaitForExitAsync(cancellationTokenSource.Token);
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                return WebDriverResult.BadRequest(new ErrorResponse()
                {
                    ErrorCode = "script error",
                    Message = $"Script failed with exit code {process.ExitCode}: {error}"
                });
            }
            var result = await process.StandardOutput.ReadToEndAsync();
            return WebDriverResult.Success(result);
        }

        private async Task<ActionResult> ExecuteWindowsClickScript(Session session, ExecuteScriptRequest executeScriptRequest)
        {
            if (executeScriptRequest.Args.Count != 1)
            {
                throw WebDriverResponseException.InvalidArgument($"Expected an array of exactly 1 arguments for the windows: click script, but got {executeScriptRequest.Args.Count} arguments");
            }
            var action = JsonSerializer.Deserialize<WindowsClickScript>(executeScriptRequest.Args[0], new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (action == null)
            {
                throw WebDriverResponseException.InvalidArgument("Action cannot be null");
            }
            await _windowsExtensionService.ExecuteClickScript(session, action);
            return WebDriverResult.Success();
        }

        private async Task<ActionResult> ExecuteWindowsScrollScript(Session session, ExecuteScriptRequest executeScriptRequest)
        {
            if (executeScriptRequest.Args.Count != 1)
            {
                throw WebDriverResponseException.InvalidArgument($"Expected an array of exactly 1 arguments for the windows: click script, but got {executeScriptRequest.Args.Count} arguments");
            }
            var action = JsonSerializer.Deserialize<WindowsScrollScript>(executeScriptRequest.Args[0], new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (action == null)
            {
                throw WebDriverResponseException.InvalidArgument("Action cannot be null");
            }
            await _windowsExtensionService.ExecuteScrollScript(session, action);
            return WebDriverResult.Success();
        }

        private async Task<ActionResult> ExecuteWindowsHoverScript(Session session, ExecuteScriptRequest executeScriptRequest)
        {
            if (executeScriptRequest.Args.Count != 1)
            {
                throw WebDriverResponseException.InvalidArgument($"Expected an array of exactly 1 arguments for the windows: hover script, but got {executeScriptRequest.Args.Count} arguments");
            }
            var action = JsonSerializer.Deserialize<WindowsHoverScript>(executeScriptRequest.Args[0], new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (action == null)
            {
                throw WebDriverResponseException.InvalidArgument("Action cannot be null");
            }
            await _windowsExtensionService.ExecuteHoverScript(session, action);
            return WebDriverResult.Success();
        }

        private async Task<ActionResult> ExecuteWindowsKeysScript(Session session, ExecuteScriptRequest executeScriptRequest)
        {
            if (executeScriptRequest.Args.Count != 1)
            {
                throw WebDriverResponseException.InvalidArgument($"Expected an array of exactly 1 arguments for the windows: keys script, but got {executeScriptRequest.Args.Count} arguments");
            }
            var windowsKeysArgs = executeScriptRequest.Args[0];
            if (!windowsKeysArgs.TryGetProperty("actions", out var actionsJson))
            {
                throw WebDriverResponseException.InvalidArgument("Expected a \"actions\" property of the first argument for the windows: keys script");
            }
            session.CurrentWindow.FocusNative();
            if (actionsJson.ValueKind == JsonValueKind.Array)
            {
                var actions = JsonSerializer.Deserialize<List<WindowsKeyScript>>(actionsJson, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                if (actions == null)
                {
                    throw WebDriverResponseException.InvalidArgument("Argument \"actions\" cannot be null");
                }
                foreach (var action in actions)
                {
                    await _windowsExtensionService.ExecuteKeyScript(session, action);
                }
            }
            else
            {
                var action = JsonSerializer.Deserialize<WindowsKeyScript>(actionsJson, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                if (action == null)
                {
                    throw WebDriverResponseException.InvalidArgument("Action cannot be null");
                }
                await _windowsExtensionService.ExecuteKeyScript(session, action);
            }
            return WebDriverResult.Success();
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
