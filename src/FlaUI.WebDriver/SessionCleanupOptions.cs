namespace FlaUI.WebDriver
{
    public class SessionCleanupOptions
    {
        public const string OptionsSectionName = "SessionCleanup";

        public double SchedulingIntervalSeconds { get; set; } = 60;
    }
}