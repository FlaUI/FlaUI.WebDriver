namespace FlaUI.WebDriver
{
    public interface ISessionRepository
    {
        void Add(Session session);
        void Delete(Session session);
        List<Session> FindAll();
        Session? FindById(string sessionId);
        List<Session> FindTimedOut();
    }
}
