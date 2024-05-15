using FlaUI.WebDriver.Models;

namespace FlaUI.WebDriver.Services
{
    public interface IWindowsExtensionService
    {
        Task ExecuteClickScript(Session session, WindowsClickScript action);
        Task ExecuteHoverScript(Session session, WindowsHoverScript action);
        Task ExecuteKeyScript(Session session, WindowsKeyScript action);
    }
}