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
            _driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
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

        [Test]
        public void PerformActions_MoveToElementAndClick_SelectsElement()
        {
            var element = _driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            new Actions(_driver).MoveToElement(element).Click().Perform();
            string activeElementText = _driver.SwitchTo().ActiveElement().Text;
            Assert.That(activeElementText, Is.EqualTo("Test TextBox"));
        }

        [Test]
        public void PerformActions_MoveToElement_IsSupported()
        {
            var element = _driver.FindElement(ExtendedBy.AccessibilityId("LabelWithHover"));

            new Actions(_driver).MoveToElement(element).Perform();

            Assert.That(element.Text, Is.EqualTo("Hovered!"));
        }

        [Test]
        public void PerformActions_MoveToElementMoveByOffsetAndClick_SelectsElement()
        {
            var element = _driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            new Actions(_driver).MoveToElement(element).MoveByOffset(5, 0).Click().Perform();
            string activeElementText = _driver.SwitchTo().ActiveElement().Text;
            Assert.That(activeElementText, Is.EqualTo("Test TextBox"));
        }
    }
}
