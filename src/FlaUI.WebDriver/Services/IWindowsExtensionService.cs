using FlaUI.WebDriver.Models;

namespace FlaUI.WebDriver.Services
{
    public interface IWindowsExtensionService
    {
        Task ExecuteClickScript(Session session, WindowsClickScript action);
        Task ExecuteScrollScript(Session session, WindowsScrollScript action);
        Task ExecuteHoverScript(Session session, WindowsHoverScript action);
        Task ExecuteKeyScript(Session session, WindowsKeyScript action);
        Task<string> ExecuteGetClipboardScript(Session session, WindowsGetClipboardScript action);
        Task ExecuteSetClipboardScript(Session session, WindowsSetClipboardScript action);
        Task ExecuteClearClipboardScript(Session session);
    }
}