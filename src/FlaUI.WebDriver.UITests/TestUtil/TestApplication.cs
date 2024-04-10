using System.IO;
using System.Linq;

namespace FlaUI.WebDriver.UITests.TestUtil
{
    public static class TestApplication
    {

        private static readonly string s_currentDirectory = Directory.GetCurrentDirectory();
        private static readonly string s_solutionDirectory = FindSolutionDirectory(s_currentDirectory);

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