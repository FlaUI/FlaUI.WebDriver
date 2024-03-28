using System.IO;

namespace FlaUI.WebDriver.UITests.TestUtil
{
    public static class TestApplication
    {
        public static string FullPath => Path.GetFullPath("..\\..\\..\\..\\TestApplications\\WpfApplication\\bin\\Release\\WpfApplication.exe");
    }
}