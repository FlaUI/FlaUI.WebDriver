using FlaUI.WebDriver.UITests.TestUtil;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace FlaUI.WebDriver.UITests
{
    [TestFixture]
    public class ActionsTests
    {
        private RemoteWebDriver _driver;

        [SetUp]
        public void Setup()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
        }

        [TearDown]
        public void Teardown()
        {
            driver?.Quit();
        }

        [Test]
        public void PerformActions_KeyDownKeyUp_IsSupported()
        {
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));
            element.Click();

            new Actions(driver).KeyDown(Keys.Control).KeyDown(Keys.Backspace).KeyUp(Keys.Backspace).KeyUp(Keys.Control).Perform();
            string activeElementText = driver.SwitchTo().ActiveElement().Text;
            Assert.That(activeElementText, Is.EqualTo("Test "));
        }

        [Test]
        public void ReleaseActions_Default_ReleasesKeys()
        {
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));
            element.Click();
            new Actions(driver).KeyDown(Keys.Control).Perform();

            driver.ResetInputState();

            new Actions(driver).KeyDown(Keys.Backspace).KeyUp(Keys.Backspace).Perform();
            string activeElmentText = driver.SwitchTo().ActiveElement().Text;
            Assert.That(activeElmentText, Is.EqualTo("Test TextBo"));
        }
    }
}
