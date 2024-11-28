using FlaUI.WebDriver.UITests.TestUtil;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;

namespace FlaUI.WebDriver.UITests
{
    [TestFixture]
    public class ExecuteTests
    {
        [Test]
        public void ExecuteScript_PowerShellCommand_ReturnsResult()
        {
            var driverOptions = FlaUIDriverOptions.RootApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var executeScriptResult = driver.ExecuteScript("powerShell", new Dictionary<string,string> { ["command"] = "1+1" });

            Assert.That(executeScriptResult, Is.EqualTo("2\r\n"));
        }

        [Test]
        public void ExecuteScript_WindowsClickXY_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            driver.ExecuteScript("windows: click", new Dictionary<string, object> { ["x"] = element.Location.X + element.Size.Width / 2, ["y"] = element.Location.Y + element.Size.Height / 2});

            string activeElementText = driver.SwitchTo().ActiveElement().Text;
            Assert.That(activeElementText, Is.EqualTo("Test TextBox"));
        }

        [Test]
        public void ExecuteScript_WindowsGetClipboard_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));
            element.Click();
            new Actions(driver).KeyDown(Keys.Control).SendKeys("a").KeyUp(Keys.Control).Perform();
            new Actions(driver).KeyDown(Keys.Control).SendKeys("c").KeyUp(Keys.Control).Perform();

            var result = driver.ExecuteScript("windows: getClipboard", new Dictionary<string, object> {});

            Assert.That(result, Is.EqualTo("Test TextBox"));
        }

        [Test]
        public void ExecuteScript_WindowsSetClipboard_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var result = driver.ExecuteScript("windows: setClipboard", new Dictionary<string, object> {
                ["b64Content"] = "Pasted!"});

            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));
            element.Click();
            new Actions(driver).KeyDown(Keys.Control).SendKeys("v").KeyUp(Keys.Control).Perform();
            Assert.That(element.Text, Is.EqualTo("Test TextBoxPasted!"));
        }

        [Test]
        public void ExecuteScript_WindowsHoverXY_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("LabelWithHover"));

            driver.ExecuteScript("windows: hover", new Dictionary<string, object> { 
                ["startX"] = element.Location.X + element.Size.Width / 2, 
                ["startY"] = element.Location.Y + element.Size.Height / 2,
                ["endX"] = element.Location.X + element.Size.Width / 2,
                ["endY"] = element.Location.Y + element.Size.Height / 2
            });

            Assert.That(element.Text, Is.EqualTo("Hovered!"));
        }

        [Test]
        public void ExecuteScript_WindowsKeys_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));
            element.Click();

            driver.ExecuteScript("windows: keys", new Dictionary<string, object> { ["actions"] = new[] {
                new Dictionary<string, object> { ["virtualKeyCode"] = 0x11, ["down"]=true }, // CTRL
                new Dictionary<string, object> { ["virtualKeyCode"] = 0x08, ["down"]=true }, // BACKSPACE
                new Dictionary<string, object> { ["virtualKeyCode"] = 0x08, ["down"]=false },
                new Dictionary<string, object> { ["virtualKeyCode"] = 0x11, ["down"]=false }
            } });

            string activeElementText = driver.SwitchTo().ActiveElement().Text;
            Assert.That(activeElementText, Is.EqualTo("Test "));
        }
    }
}
