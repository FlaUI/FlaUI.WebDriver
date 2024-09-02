using OpenQA.Selenium.Appium;

namespace FlaUI.WebDriver.UITests.TestUtil
{
    internal class FlaUIAppiumDriverOptions : AppiumOptions
    {
        public static FlaUIAppiumDriverOptions TestApp() => ForApp(TestApplication.FullPath);

        public static FlaUIAppiumDriverOptions RootApp() => ForApp("Root");

        public static FlaUIAppiumDriverOptions ForApp(string path)
        {
            return new FlaUIAppiumDriverOptions()
            {
                AutomationName = "FlaUI",
                PlatformName = "Windows",
                App = path
            };
        }

        public static FlaUIAppiumDriverOptions ForAppTopLevelWindow(string windowHandle)
        {
            var options = new FlaUIAppiumDriverOptions()
            {
                AutomationName = "FlaUI",
                PlatformName = "Windows"
            };
            options.AddAdditionalAppiumOption("appium:appTopLevelWindow", windowHandle);
            return options;
        }

        public static FlaUIAppiumDriverOptions ForAppTopLevelWindowTitleMatch(string match)
        {
            var options = new FlaUIAppiumDriverOptions()
            {
                AutomationName = "FlaUI",
                PlatformName = "Windows"
            };
            options.AddAdditionalAppiumOption("appium:appTopLevelWindowTitleMatch", match);
            return options;
        }

        public static FlaUIAppiumDriverOptions Empty()
        {
            return new FlaUIAppiumDriverOptions();
        }
    }
}
