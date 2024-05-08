using System.IO;
using System.Linq;
using OpenQA.Selenium.Remote;

namespace FlaUI.WebDriver.UITests.TestUtil
{
    public static class TestApplication
    {

        private static readonly string s_currentDirectory = Directory.GetCurrentDirectory();
        private static readonly string s_solutionDirectory = FindSolutionDirectory(s_currentDirectory);

        public static double GetScaling(RemoteWebDriver driver)
        {
            return double.Parse(driver.FindElement(ExtendedBy.AccessibilityId("DpiScaling")).Text.ToString());
        }

        private static string FindSolutionDirectory(string currentDirectory)
        {
            while (!Directory.GetFiles(currentDirectory, "*.sln").Any())
            {
                currentDirectory = Directory.GetParent(currentDirectory).FullName;
            }
            return currentDirectory;
        }

        public static string FullPath => Path.Combine(s_solutionDirectory, "TestApplications", "WpfApplication", "bin", "Release", "WpfApplication.exe");
    } 
    
}