using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace FlaUI.WebDriver
{
    public class Session : IDisposable
    {
        public Session(Application? app, bool isAppOwnedBySession)
        {
            App = app;
            SessionId = Guid.NewGuid().ToString();
            Automation = new UIA3Automation();
            InputState = new InputState();
            TimeoutsConfiguration = new TimeoutsConfiguration();
            IsAppOwnedBySession = isAppOwnedBySession;

            if (app != null)
            {
                // We have to capture the initial window handle to be able to keep it stable
                CurrentWindowWithHandle = GetOrAddKnownWindow(app.GetMainWindow(Automation, PageLoadTimeout));
            }
        }

        public string SessionId { get; }
        public UIA3Automation Automation { get; }
        public Application? App { get; }
        public InputState InputState { get; }
        private Dictionary<string, KnownElement> KnownElementsByElementReference { get; } = new Dictionary<string, KnownElement>();
        private Dictionary<string, KnownWindow> KnownWindowsByWindowHandle { get; } = new Dictionary<string, KnownWindow>();
        public TimeSpan ImplicitWaitTimeout => TimeSpan.FromMilliseconds(TimeoutsConfiguration.ImplicitWaitTimeoutMs);
        public TimeSpan PageLoadTimeout => TimeSpan.FromMilliseconds(TimeoutsConfiguration.PageLoadTimeoutMs);
        public TimeSpan? ScriptTimeout => TimeoutsConfiguration.ScriptTimeoutMs.HasValue ? TimeSpan.FromMilliseconds(TimeoutsConfiguration.ScriptTimeoutMs.Value) : null;
        public bool IsAppOwnedBySession { get; }

        public TimeoutsConfiguration TimeoutsConfiguration { get; set; }

        private KnownWindow? CurrentWindowWithHandle { get; set; }

        public Window CurrentWindow
        {
            get
            {
                if (App == null || CurrentWindowWithHandle == null)
                {
                    throw WebDriverResponseException.UnsupportedOperation("This operation is not supported for Root app");
                }
                return CurrentWindowWithHandle.Window;
            }
            set
            {
                CurrentWindowWithHandle = GetOrAddKnownWindow(value);
            }
        }

        public string CurrentWindowHandle
        {
            get
            {
                if (App == null || CurrentWindowWithHandle == null)
                {
                    throw WebDriverResponseException.UnsupportedOperation("This operation is not supported for Root app");
                }
                return CurrentWindowWithHandle.WindowHandle;
            }
        }

        public bool IsTimedOut => (DateTime.UtcNow - LastNewCommandTimeUtc) > NewCommandTimeout;

        public TimeSpan NewCommandTimeout { get; internal set; } = TimeSpan.FromSeconds(60);
        public DateTime LastNewCommandTimeUtc { get; internal set; } = DateTime.UtcNow;

        public void SetLastCommandTimeToNow()
        {
            LastNewCommandTimeUtc = DateTime.UtcNow;
        }

        public KnownElement GetOrAddKnownElement(AutomationElement element)
        {
            var result = KnownElementsByElementReference.Values.FirstOrDefault(knownElement => knownElement.Element.Equals(element));
            if (result == null)
            {
                result = new KnownElement(element);
                KnownElementsByElementReference.Add(result.ElementReference, result);
            }
            return result;
        }

        public AutomationElement? FindKnownElementById(string elementId)
        {
            if (!KnownElementsByElementReference.TryGetValue(elementId, out var knownElement))
            {
                return null;
            }
            return knownElement.Element;
        }

        public KnownWindow GetOrAddKnownWindow(Window window)
        {
            var result = KnownWindowsByWindowHandle.Values.FirstOrDefault(knownElement => knownElement.Window.Equals(window));
            if (result == null)
            {
                result = new KnownWindow(window);
                KnownWindowsByWindowHandle.Add(result.WindowHandle, result);
            }
            return result;
        }

        public Window? FindKnownWindowByWindowHandle(string windowHandle)
        {
            if (!KnownWindowsByWindowHandle.TryGetValue(windowHandle, out var knownWindow))
            {
                return null;
            }
            return knownWindow.Window;
        }

        public void RemoveKnownWindow(Window window)
        {
            var item = KnownWindowsByWindowHandle.Values.FirstOrDefault(knownElement => knownElement.Window.Equals(window));
            if (item != null)
            {
                KnownWindowsByWindowHandle.Remove(item.WindowHandle);
            }
        }

        public void EvictUnavailableElements()
        {
            // Evict unavailable elements to prevent slowing down
            var unavailableElements = KnownElementsByElementReference.Where(item => !item.Value.Element.IsAvailable).Select(item => item.Key).ToArray();
            foreach (var unavailableElementKey in unavailableElements)
            {
                KnownElementsByElementReference.Remove(unavailableElementKey);
            }
        }

        public void EvictUnavailableWindows()
        {
            // Evict unavailable windows to prevent slowing down
            var unavailableWindows = KnownWindowsByWindowHandle.Where(item => !item.Value.Window.IsAvailable).Select(item => item.Key).ToArray();
            foreach (var unavailableWindowKey in unavailableWindows)
            {
                KnownWindowsByWindowHandle.Remove(unavailableWindowKey);
            }
        }

        public void Dispose()
        {
            if (IsAppOwnedBySession && App != null && !App.HasExited)
            {
                App.Close();
            }
            Automation.Dispose();
            App?.Dispose();
        }
    }
}
