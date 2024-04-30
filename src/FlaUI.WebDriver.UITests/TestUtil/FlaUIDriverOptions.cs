using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace FlaUI.WebDriver.UITests.TestUtil
{
    internal class FlaUIDriverOptions : AppiumOptions
    {
        public static FlaUIDriverOptions TestApp() => ForApp(TestApplication.FullPath);

        public static FlaUIDriverOptions RootApp() => ForApp("Root");

        public static FlaUIDriverOptions ForApp(string path)
        {
            var options = new FlaUIDriverOptions()
            {
                App = path,
                AutomationName = "FlaUI",
                PlatformName = "Windows"
            };
            return options;
        }

        public static FlaUIDriverOptions ForAppTopLevelWindow(string windowHandle)
        {
            var options = new FlaUIDriverOptions()
            {
                AutomationName = "FlaUI",
                PlatformName = "Windows"
            };
            options.AddAdditionalAppiumOption("appium:appTopLevelWindow", windowHandle);
            return options;
        }

        public static FlaUIDriverOptions ForAppTopLevelWindowTitleMatch(string match)
        {
            var options = new FlaUIDriverOptions()
            {
                AutomationName = "FlaUI",
                PlatformName = "Windows"
            };
            options.AddAdditionalAppiumOption("appium:appTopLevelWindowTitleMatch", match);
            return options;
        }

        public static FlaUIDriverOptions Empty()
        {
            return new FlaUIDriverOptions();
        }
    }
}
