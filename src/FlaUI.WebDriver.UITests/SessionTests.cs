using NUnit.Framework;
using OpenQA.Selenium.Remote;
using FlaUI.WebDriver.UITests.TestUtil;
using OpenQA.Selenium;
using System;

namespace FlaUI.WebDriver.UITests
{
    [TestFixture]
    public class SessionTests
    {
        [Test]
        public void NewSession_PlatformNameMissing_ReturnsError()
        {
            var emptyOptions = FlaUIDriverOptions.Empty();

            var newSession = () => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, emptyOptions);

            Assert.That(newSession, Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Missing capability 'platformName' with value 'windows' (SessionNotCreated)"));
        }

        [Test]
        public void NewSession_AutomationNameMissing_ReturnsError()
        {
            var emptyOptions = FlaUIDriverOptions.Empty();
            emptyOptions.PlatformName = "Windows";

            var newSession = () => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, emptyOptions);

            Assert.That(newSession, Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Missing capability 'appium:automationName' with value 'flaui' (SessionNotCreated)"));
        }

        [Test]
        public void NewSession_AllAppCapabilitiesMissing_ReturnsError()
        {
            var emptyOptions = FlaUIDriverOptions.Empty();
            emptyOptions.PlatformName = "Windows";
            emptyOptions.AddAdditionalOption("appium:automationName", "windows");

            var newSession = () => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, emptyOptions);

            Assert.That(newSession, Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Missing capability 'appium:automationName' with value 'flaui' (SessionNotCreated)"));
        }

        [Test]
        public void NewSession_App_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var title = driver.Title;

            Assert.That(title, Is.EqualTo("FlaUI WPF Test App"));
        }

        [Test]
        public void NewSession_AppNotExists_ReturnsError()
        {
            var driverOptions = FlaUIDriverOptions.App("C:\\NotExisting.exe");

            var newSession = () => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(newSession, Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Starting app 'C:\\NotExisting.exe' with arguments '' threw an exception: An error occurred trying to start process 'C:\\NotExisting.exe' with working directory '.'. The system cannot find the file specified."));
        }

        [TestCase(123)]
        [TestCase(false)]
        public void NewSession_AppNotAString_Throws(object value)
        {
            var driverOptions = new FlaUIDriverOptions()
            {
                PlatformName = "Windows"
            };
            driverOptions.AddAdditionalOption("appium:automationName", "FlaUI");
            driverOptions.AddAdditionalOption("appium:app", value);

            Assert.That(() => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions),
                Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Capability appium:app must be a string"));
        }

        [Test]
        public void NewSession_AppWorkingDir_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driverOptions.AddAdditionalOption("appium:appWorkingDir", "C:\\");
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var title = driver.Title;

            Assert.That(title, Is.EqualTo("FlaUI WPF Test App"));
        }

        [Test]
        public void NewSession_Timeouts_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driverOptions.ScriptTimeout = TimeSpan.FromSeconds(10);
            driverOptions.PageLoadTimeout = TimeSpan.FromSeconds(50);
            driverOptions.ImplicitWaitTimeout = TimeSpan.FromSeconds(3);

            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(driver.Manage().Timeouts().AsynchronousJavaScript, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(driver.Manage().Timeouts().PageLoad, Is.EqualTo(TimeSpan.FromSeconds(50)));
            Assert.That(driver.Manage().Timeouts().ImplicitWait, Is.EqualTo(TimeSpan.FromSeconds(3)));
        }

        [Test]
        public void NewSession_NotSupportedAppiumCapability_Throws()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driverOptions.AddAdditionalOption("appium:unknown", "value");

            Assert.That(() => { using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions); },
                Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("The following capabilities could not be matched: 'appium:unknown' (SessionNotCreated)"));
        }

        [Test]
        public void NewSession_UnknownExtensionCapability_Ignores()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driverOptions.AddAdditionalOption("unknown:unknown", "value");

            Assert.That(() => { using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions); },
                Throws.Nothing);
        }

        [Test]
        public void NewSession_AppTopLevelWindow_IsSupported()
        {
            using var testAppProcess = new TestAppProcess();
            var windowHandle = string.Format("0x{0:x}", testAppProcess.Process.MainWindowHandle);
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindow(windowHandle);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var title = driver.Title;

            Assert.That(title, Is.EqualTo("FlaUI WPF Test App"));
        }

        [Test]
        public void EndSession_AppTopLevelWindow_DoesNotKillApp()
        {
            using var testAppProcess = new TestAppProcess();
            var windowHandle = string.Format("0x{0:x}", testAppProcess.Process.MainWindowHandle);
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindow(windowHandle);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            driver.Quit();

            Assert.That(testAppProcess.Process.HasExited, Is.False);
        }

        [Test]
        public void NewSession_AppTopLevelWindowNotFound_ReturnsError()
        {
            using var testAppProcess = new TestAppProcess();
            var windowHandle = string.Format("0x{0:x}", testAppProcess.Process.MainWindowHandle);
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindow(windowHandle);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var title = driver.Title;

            Assert.That(title, Is.EqualTo("FlaUI WPF Test App"));
        }

        [Test]
        public void NewSession_AppTopLevelWindowZero_ReturnsError()
        {
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindow("0x0");

            var newSession = () => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(newSession, Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Capability appium:appTopLevelWindow '0x0' should not be zero"));
        }

        [TestCase("FlaUI WPF Test App")]
        [TestCase("FlaUI WPF .*")]
        public void NewSession_AppTopLevelWindowTitleMatch_IsSupported(string match)
        {
            using var testAppProcess = new TestAppProcess();
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindowTitleMatch(match);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var title = driver.Title;

            Assert.That(title, Is.EqualTo("FlaUI WPF Test App"));
        }

        [Explicit("Sometimes multiple processes are left open")]
        [Test]
        public void EndSession_AppTopLevelWindowTitleMatch_DoesNotKillApp()
        {
            using var testAppProcess = new TestAppProcess();
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindowTitleMatch("FlaUI WPF Test App");
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            driver.Quit();

            Assert.That(testAppProcess.Process.HasExited, Is.False);
        }

        [Test]
        public void NewSession_AppTopLevelWindowTitleMatchMultipleMatching_ReturnsError()
        {
            using var testAppProcess = new TestAppProcess();
            using var testAppProcess1 = new TestAppProcess();
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindowTitleMatch("FlaUI WPF Test App");
            
            var newSession = () => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(newSession, Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Found multiple (2) processes with main window title matching 'FlaUI WPF Test App'"));
        }

        [Test, Explicit("GitHub actions runner doesn't have calculator installed")]
        public void NewSession_UwpApp_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.App("Microsoft.WindowsCalculator_8wekyb3d8bbwe!App");
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var title = driver.Title;

            Assert.That(title, Is.EqualTo("Calculator"));
        }

        [Test]
        public void NewSession_AppTopLevelWindowTitleMatchNotFound_ReturnsError()
        {
            using var testAppProcess = new TestAppProcess();
            using var testAppProcess1 = new TestAppProcess();
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindowTitleMatch("FlaUI Not Existing");

            var newSession = () => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(newSession, Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Process with main window title matching 'FlaUI Not Existing' could not be found"));
        }

        [TestCase(123)]
        [TestCase(false)]
        public void NewSession_AppTopLevelWindowTitleMatchNotAString_Throws(object value)
        {
            var driverOptions = new FlaUIDriverOptions()
            {
                PlatformName = "Windows"
            };
            driverOptions.AddAdditionalOption("appium:automationName", "FlaUI");
            driverOptions.AddAdditionalOption("appium:appTopLevelWindowTitleMatch", value);

            Assert.That(() => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions),
                Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Capability appium:appTopLevelWindowTitleMatch must be a string"));
        }

        [TestCase("(invalid")]
        public void NewSession_AppTopLevelWindowTitleMatchInvalidRegex_Throws(string value)
        {
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindowTitleMatch(value);

            Assert.That(() => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions),
                Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Capability appium:appTopLevelWindowTitleMatch '(invalid' is not a valid regular expression: Invalid pattern '(invalid' at offset 8. Not enough )'s."));
        }

        [TestCase("")]
        [TestCase("FlaUI")]
        public void NewSession_AppTopLevelWindowInvalidFormat_ReturnsError(string appTopLevelWindowString)
        {
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindow(appTopLevelWindowString);

            var newSession = () => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(newSession, Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo($"Capability appium:appTopLevelWindow '{appTopLevelWindowString}' is not a valid hexadecimal string"));
        }

        [TestCase(123)]
        [TestCase(false)]
        public void NewSession_AppTopLevelWindowNotAString_ReturnsError(object value)
        {
            var driverOptions = new FlaUIDriverOptions()
            {
                PlatformName = "Windows"
            };
            driverOptions.AddAdditionalOption("appium:automationName", "FlaUI");
            driverOptions.AddAdditionalOption("appium:appTopLevelWindow", value);

            Assert.That(() => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions),
                Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Capability appium:appTopLevelWindow must be a string"));
        }

        [Test]
        public void GetTitle_Default_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var title = driver.Title;

            Assert.That(title, Is.EqualTo("FlaUI WPF Test App"));
        }

        [Test, Explicit("Takes too long (one minute)")]
        public void NewCommandTimeout_DefaultValue_OneMinute()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(60) + WebDriverFixture.SessionCleanupInterval*2);

            Assert.That(() => driver.Title, Throws.TypeOf<WebDriverException>().With.Message.Matches("No active session with ID '.*'"));
        }

        [Test]
        public void NewCommandTimeout_Expired_EndsSession()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driverOptions.AddAdditionalOption("appium:newCommandTimeout", 1);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1) + WebDriverFixture.SessionCleanupInterval * 2);

            Assert.That(() => driver.Title, Throws.TypeOf<WebDriverException>().With.Message.Matches("No active session with ID '.*'"));
        }

        [Test]
        public void NewCommandTimeout_ReceivedCommandsBeforeExpiry_DoesNotEndSession()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driverOptions.AddAdditionalOption("appium:newCommandTimeout", WebDriverFixture.SessionCleanupInterval.TotalSeconds * 4);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            System.Threading.Thread.Sleep(WebDriverFixture.SessionCleanupInterval * 2);
            _ = driver.Title;
            System.Threading.Thread.Sleep(WebDriverFixture.SessionCleanupInterval * 2);
            _ = driver.Title;
            System.Threading.Thread.Sleep(WebDriverFixture.SessionCleanupInterval * 2);

            Assert.That(() => driver.Title, Throws.Nothing);
        }

        [Test]
        public void NewCommandTimeout_NotExpired_DoesNotEndSession()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driverOptions.AddAdditionalOption("appium:newCommandTimeout", 240);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            System.Threading.Thread.Sleep(WebDriverFixture.SessionCleanupInterval * 2);

            Assert.That(() => driver.Title, Throws.Nothing);
        }

        [Test]
        public void NewCommandTimeout_SessionWithAppTopLevelWindowTitleMatch_ClosesSessionButDoesNotCloseApp()
        {
            using var testAppProcess = new TestAppProcess();
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindowTitleMatch("FlaUI WPF Test App");
            driverOptions.AddAdditionalOption("appium:newCommandTimeout", 1);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1) + WebDriverFixture.SessionCleanupInterval * 2);

            Assert.That(testAppProcess.Process.HasExited, Is.False);
            Assert.That(() => driver.Title, Throws.TypeOf<WebDriverException>().With.Message.Matches("No active session with ID '.*'"));
        }

        [Test]
        public void NewCommandTimeout_SessionWithAppTopLevelWindow_ClosesSessionButDoesNotCloseApp()
        {
            using var testAppProcess = new TestAppProcess();
            var windowHandle = string.Format("0x{0:x}", testAppProcess.Process.MainWindowHandle);
            var driverOptions = FlaUIDriverOptions.AppTopLevelWindow(windowHandle);
            driverOptions.AddAdditionalOption("appium:newCommandTimeout", 1);
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1) + WebDriverFixture.SessionCleanupInterval * 2);

            Assert.That(testAppProcess.Process.HasExited, Is.False);
            Assert.That(() => driver.Title, Throws.TypeOf<WebDriverException>().With.Message.Matches("No active session with ID '.*'"));
        }

        [TestCase("123")]
        [TestCase(false)]
        [TestCase("not a number")]
        public void NewCommandTimeout_InvalidValue_Throws(object value)
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driverOptions.AddAdditionalOption("appium:newCommandTimeout", value);

            Assert.That(() => new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions), 
                Throws.TypeOf<WebDriverArgumentException>().With.Message.EqualTo("Capability appium:newCommandTimeout must be a number"));
        }

        [Test]
        public void UnknownCommand_Default_ReturnsError()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(() => driver.Manage().Cookies.DeleteAllCookies(),
                Throws.TypeOf<System.NotImplementedException>().With.Message.EqualTo("Unknown command"));
        }
    }
}
