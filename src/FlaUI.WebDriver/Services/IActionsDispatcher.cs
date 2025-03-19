
namespace FlaUI.WebDriver.Services
{
    public interface IActionsDispatcher
    {
        Task DispatchAction(Session session, Action action);
        Task DispatchActionsForString(Session session, string inputId, KeyInputSource source, string text);
        Task DispatchActionsForStringUsingFlaUICore(Session session, string inputId, KeyInputSource source, string text);
    }
}
