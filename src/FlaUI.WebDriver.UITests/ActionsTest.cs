using FlaUI.WebDriver.UITests.TestUtil;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace FlaUI.WebDriver.UITests
{
    [TestFixture]
    public class ActionsTests
    {
        private WindowsDriver _driver;

        [SetUp]
        public void Setup()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            _driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
        }

        [TearDown]
        public void Teardown()
        {
            _driver?.Dispose();
        }

        [Test]
        public void PerformActions_KeyDownKeyUp_IsSupported()
        {
            var element = _driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));
            element.Click();

            new Actions(_driver).KeyDown(Keys.Control).KeyDown(Keys.Backspace).KeyUp(Keys.Backspace).KeyUp(Keys.Control).Perform();
            string activeElementText = _driver.SwitchTo().ActiveElement().Text;
            Assert.That(activeElementText, Is.EqualTo("Test "));
        }

        [Test]
        public void ReleaseActions_Default_ReleasesKeys()
        {
            var element = _driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));
            element.Click();
            new Actions(_driver).KeyDown(Keys.Control).Perform();

            _driver.ResetInputState();

            new Actions(_driver).KeyDown(Keys.Backspace).KeyUp(Keys.Backspace).Perform();
            string activeElmentText = _driver.SwitchTo().ActiveElement().Text;
            Assert.That(activeElmentText, Is.EqualTo("Test TextBo"));
        }
    }
}
