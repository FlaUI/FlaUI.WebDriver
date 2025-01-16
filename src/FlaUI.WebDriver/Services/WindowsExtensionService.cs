using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.WebDriver.Models;
using System.Drawing;

namespace FlaUI.WebDriver.Services
{
    public class WindowsExtensionService : IWindowsExtensionService
    {
        private readonly ILogger<WindowsExtensionService> _logger;

        public WindowsExtensionService(ILogger<WindowsExtensionService> logger)
        {
            _logger = logger;
        }

        public Task<string> ExecuteGetClipboardScript(Session session, WindowsGetClipboardScript action)
        {
            switch (action.ContentType)
            {
                default:
                case "plaintext":
                    return Task.FromResult(ExecuteOnClipboardThread(
                        () => System.Windows.Forms.Clipboard.GetText(System.Windows.Forms.TextDataFormat.UnicodeText)
                       ));
                case "image":
                    return Task.FromResult(ExecuteOnClipboardThread(() =>
                    {
                        using var image = System.Windows.Forms.Clipboard.GetImage();
                        if (image == null)
                        {
                            return "";
                        }
                        using var stream = new MemoryStream();
                        image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        return Convert.ToBase64String(stream.ToArray());
                    }));
            }
        }

        public Task ExecuteSetClipboardScript(Session session, WindowsSetClipboardScript action)
        {
            switch (action.ContentType)
            {
                default:
                case "plaintext":
                    ExecuteOnClipboardThread(() => System.Windows.Forms.Clipboard.SetText(action.B64Content));
                    break;
                case "image":
                    ExecuteOnClipboardThread(() =>
                    {
                        using var stream = new MemoryStream(Convert.FromBase64String(action.B64Content));
                        using var image = Image.FromStream(stream);
                        System.Windows.Forms.Clipboard.SetImage(image);
                    });
                    break;
            }
            return Task.CompletedTask;
        }

        public Task ExecuteClearClipboardScript(Session session)
        {
            ExecuteOnClipboardThread(() => System.Windows.Forms.Clipboard.Clear());
            return Task.CompletedTask;
        }

        private void ExecuteOnClipboardThread(System.Action action)
        {
            // See https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.clipboard?view=windowsdesktop-8.0#remarks
            var thread = new Thread(() => action());
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private string ExecuteOnClipboardThread(Func<string> method)
        {
            // See https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.clipboard?view=windowsdesktop-8.0#remarks
            string result = "";
            var thread = new Thread(() => { result = method(); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return result;
        }

        public async Task ExecuteClickScript(Session session, WindowsClickScript action)
        {
            if (action.DurationMs.HasValue)
            {
                throw WebDriverResponseException.UnsupportedOperation("Duration is not yet supported");
            }
            if (action.Times.HasValue)
            {
                throw WebDriverResponseException.UnsupportedOperation("Times is not yet supported");
            }
            if (action.ModifierKeys != null)
            {
                throw WebDriverResponseException.UnsupportedOperation("Modifier keys are not yet supported");
            }
            var point = GetPoint(action.ElementId, action.X, action.Y, session);
            var mouseButton = action.Button != null ? Enum.Parse<MouseButton>(action.Button, true) : MouseButton.Left;
            _logger.LogDebug("Clicking point ({X}, {Y}) with mouse button {MouseButton}", point.X, point.Y, mouseButton);
            Mouse.Click(point, mouseButton);
            await Task.Yield();
        }

        public async Task ExecuteScrollScript(Session session, WindowsScrollScript action)
        {
            if (action.ModifierKeys != null)
            {
                throw WebDriverResponseException.UnsupportedOperation("Modifier keys are not yet supported");
            }
            var point = GetPoint(action.ElementId, action.X, action.Y, session);
            _logger.LogDebug("Scrolling at point ({X}, {Y})", point.X, point.Y);
            Mouse.Position = point;
            if (action.DeltaY.HasValue && action.DeltaY.Value != 0)
            {
                Mouse.Scroll(action.DeltaY.Value);
            }
            if (action.DeltaX.HasValue && action.DeltaX.Value != 0)
            {
                Mouse.HorizontalScroll(-action.DeltaX.Value);
            }
            await Task.Yield();
        }

        private Point GetPoint(string? elementId, int? x, int? y, Session session)
        {
            if (elementId != null)
            {
                var element = session.FindKnownElementById(elementId);
                if (element == null)
                {
                    throw WebDriverResponseException.ElementNotFound(elementId);
                }

                if (x.HasValue && y.HasValue)
                {
                    return new Point
                    {
                        X = element.BoundingRectangle.Left + x.Value,
                        Y = element.BoundingRectangle.Top + y.Value
                    };
                }

                return element.BoundingRectangle.Center();
            }

            if (x.HasValue && y.HasValue)
            {
                return new Point { X = x.Value, Y = y.Value };
            }

            throw WebDriverResponseException.InvalidArgument("Either element ID or x and y must be provided");
        }

        public async Task ExecuteHoverScript(Session session, WindowsHoverScript action)
        {
            if (action.ModifierKeys != null)
            {
                throw WebDriverResponseException.UnsupportedOperation("Modifier keys are not yet supported");
            }
            var startingPoint = GetPoint(action.StartElementId, action.StartX, action.StartY, session);
            var endPoint = GetPoint(action.EndElementId, action.EndX, action.EndY, session);

            _logger.LogDebug("Moving mouse to starting point ({X}, {Y})", startingPoint.X, startingPoint.Y);
            Mouse.Position = startingPoint;

            if (endPoint == startingPoint)
            {
                // Hover for specified time
                await Task.Delay(action.DurationMs ?? 100);
                return;
            }

            _logger.LogDebug("Moving mouse to end point ({X}, {Y})", endPoint.X, endPoint.Y);
            if (action.DurationMs.HasValue)
            {
                if (action.DurationMs.Value <= 0)
                {
                    throw WebDriverResponseException.UnsupportedOperation("Duration less than or equal to zero is not supported");
                }
                Mouse.MovePixelsPerMillisecond = endPoint.Distance(startingPoint) / action.DurationMs.Value;
            }
            Mouse.MoveTo(endPoint);
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
