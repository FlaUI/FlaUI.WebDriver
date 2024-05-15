using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.WebDriver.Models;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace FlaUI.WebDriver.Services
{
    public class WindowsExtensionService : IWindowsExtensionService
    {
        private readonly ILogger<WindowsExtensionService> _logger;

        public WindowsExtensionService(ILogger<WindowsExtensionService> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteClickScript(Session session, WindowsClickScript action)
        {
            var mouseButton = action.Button != null ? Enum.Parse<MouseButton>(action.Button, true) : MouseButton.Left;
            if (action.ElementId != null)
            {
                var element = session.FindKnownElementById(action.ElementId);
                if (element == null)
                {
                    throw WebDriverResponseException.ElementNotFound(action.ElementId);
                }
                Mouse.Click(element.BoundingRectangle.Location, mouseButton);
            }
            else if (action.X.HasValue && action.Y.HasValue)
            {
                Mouse.Click(new Point { X = action.X.Value, Y = action.Y.Value }, mouseButton);
            }
            else
            {
                throw WebDriverResponseException.InvalidArgument("Either \"elementId\" or \"x\" and \"y\" must be provided");
            }
            await Task.Yield();
        }

        public async Task ExecuteHoverScript(Session session, WindowsHoverScript action)
        {
            if (action.StartX.HasValue && action.StartY.HasValue)
            {
                Mouse.MoveTo(action.StartX.Value, action.StartY.Value);
            }
            else if (action.StartElementId != null)
            {
                var element = session.FindKnownElementById(action.StartElementId);
                if (element == null)
                {
                    throw WebDriverResponseException.ElementNotFound(action.StartElementId);
                }
                Mouse.MoveTo(element.BoundingRectangle.Location);
            }
            else
            {
                throw WebDriverResponseException.InvalidArgument("Either \"startElementId\" or \"startX\" and \"startY\" must be provided");
            }

            if (action.DurationMs.HasValue)
            {
                await Task.Delay(action.DurationMs.Value);
            }

            if (action.EndX.HasValue && action.EndY.HasValue)
            {
                Mouse.MoveTo(action.EndX.Value, action.EndY.Value);
            }
            else if (action.EndElementId != null)
            {
                var element = session.FindKnownElementById(action.EndElementId);
                if (element == null)
                {
                    throw WebDriverResponseException.ElementNotFound(action.EndElementId);
                }
                Mouse.MoveTo(element.BoundingRectangle.Location);
            }
            else
            {
                throw WebDriverResponseException.InvalidArgument("Either \"endElementId\" or \"endX\" and \"endY\" must be provided");
            }
        }

        public async Task ExecuteKeyScript(Session session, WindowsKeyScript action)
        {
            if (action.VirtualKeyCode.HasValue)
            {
                if (action.Down.HasValue)
                {
                    if (action.Down.Value == true)
                    {
                        _logger.LogDebug("Pressing key {VirtualKeyCode}", action.VirtualKeyCode.Value);
                        Keyboard.Press((VirtualKeyShort)action.VirtualKeyCode.Value);
                        await Task.Delay(10);
                    }
                    else
                    {
                        _logger.LogDebug("Releasing key {VirtualKeyCode}", action.VirtualKeyCode.Value);
                        Keyboard.Release((VirtualKeyShort)action.VirtualKeyCode.Value);
                        await Task.Delay(10);
                    }
                }
                else
                {
                    _logger.LogDebug("Pressing and releasing key {VirtualKeyCode}", action.VirtualKeyCode.Value);
                    Keyboard.Press((VirtualKeyShort)action.VirtualKeyCode.Value);
                    await Task.Delay(10);
                    Keyboard.Release((VirtualKeyShort)action.VirtualKeyCode.Value);
                    await Task.Delay(10);
                }
            }
            else if (action.Text != null)
            {
                _logger.LogDebug("Typing {Text}", action.Text);
                Keyboard.Type(action.Text);
            }
            else if (action.Pause.HasValue)
            {
                _logger.LogDebug("Pausing for {Pause} milliseconds", action.Pause.Value);
                await Task.Delay(action.Pause.Value);
            }
            else
            {
                throw WebDriverResponseException.InvalidArgument("Action must have either \"text\", \"virtualKeyCode\" or \"pause\"");
            }
        }
    }
}
