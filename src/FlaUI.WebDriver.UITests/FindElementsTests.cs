using FlaUI.WebDriver.UITests.TestUtil;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Linq;

namespace FlaUI.WebDriver.UITests
{
    public class FindElementsTests
    {
        [Test]
        public void FindElement_FromDesktop_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.RootApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(By.Name("Taskbar"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByXPath_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(By.XPath("//Text"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByAccessibilityId_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ById_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(By.Id("TextBox"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByName_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(By.Name("Test Label"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByNativeName_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(ExtendedBy.NonCssName("Test Label"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByNativeClassName_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(ExtendedBy.NonCssClassName("TextBlock"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByClassName_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(By.ClassName("TextBlock"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByLinkText_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(By.LinkText("Invoke me!"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByPartialLinkText_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(By.PartialLinkText("Invoke"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_ByTagName_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var element = driver.FindElement(By.TagName("Text"));

            Assert.That(element, Is.Not.Null);
        }

        [Test]
        public void FindElement_NotExisting_ThrowsNoSuchElementException()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var findElement = () => driver.FindElement(ExtendedBy.AccessibilityId("NotExisting"));

            Assert.That(findElement, Throws.TypeOf<NoSuchElementException>());
        }

        [Test]
        public void FindElementFromElement_InsideElement_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var fromElement = driver.FindElement(By.TagName("Tab"));

            var foundElement = fromElement.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            Assert.That(foundElement, Is.Not.Null);
        }

        [Test]
        public void FindElementFromElement_OutsideElement_ThrowsNoSuchElementException()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var fromElement = driver.FindElement(ExtendedBy.AccessibilityId("ListBox"));

            var findElement = () => fromElement.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            Assert.That(findElement, Throws.TypeOf<NoSuchElementException>());
        }

        [Test]
        public void FindElementsFromElement_InsideElement_ReturnsElement()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var fromElement = driver.FindElement(By.TagName("Tab"));

            var foundElements = fromElement.FindElements(By.TagName("RadioButton"));

            Assert.That(foundElements, Has.Count.EqualTo(2));
        }

        [Test]
        public void FindElementsFromElement_OutsideElement_ReturnsEmptyList()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var fromElement = driver.FindElement(ExtendedBy.AccessibilityId("ListBox"));

            var foundElements = fromElement.FindElements(ExtendedBy.AccessibilityId("TextBox"));

            Assert.That(foundElements, Is.Empty);
        }

        [Test]
        public void FindElements_Default_ReturnsElements()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var elements = driver.FindElements(By.TagName("RadioButton"));

            Assert.That(elements, Has.Count.EqualTo(2));
        }

        [Test]
        public void FindElements_NotExisting_ReturnsEmptyList()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var foundElements = driver.FindElements(ExtendedBy.AccessibilityId("NotExisting"));

            Assert.That(foundElements, Is.Empty);
        }

        [Test]
        public void FindElement_InOtherWindow_ThrowsNoSuchElementException()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            OpenAndSwitchToAnotherWindow(driver);

            var findElement = () => driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            Assert.That(findElement, Throws.TypeOf<NoSuchElementException>());
            var elementInNewWindow = driver.FindElement(ExtendedBy.AccessibilityId("Window1TextBox"));
            Assert.That(elementInNewWindow, Is.Not.Null);
        }

        [Test]
        public void FindElements_InOtherWindow_ReturnsEmptyList()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            OpenAndSwitchToAnotherWindow(driver);

            var foundElements = driver.FindElements(ExtendedBy.AccessibilityId("TextBox"));

            Assert.That(foundElements, Is.Empty);
            var elementsInNewWindow = driver.FindElements(ExtendedBy.AccessibilityId("Window1TextBox"));
            Assert.That(elementsInNewWindow, Has.Count.EqualTo(1));
        }

        [Test]
        public void FindElements_AfterPreviousKnownElementUnavailable_DoesNotThrow()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var initialWindowHandle = driver.CurrentWindowHandle;
            OpenAndSwitchToAnotherWindow(driver);
            var elementInNewWindow = driver.FindElement(ExtendedBy.AccessibilityId("Window1TextBox"));
            // close to make the elementInNewWindow unavailable
            driver.Close();
            driver.SwitchTo().Window(initialWindowHandle);

            var foundElements = driver.FindElements(ExtendedBy.AccessibilityId("TextBox"));

            Assert.That(foundElements, Has.Count.EqualTo(1));
        }

        private static void OpenAndSwitchToAnotherWindow(RemoteWebDriver driver)
        {
            var initialWindowHandles = new[] { driver.CurrentWindowHandle };
            OpenAnotherWindow(driver);
            var windowHandlesAfterOpen = driver.WindowHandles;
            var newWindowHandle = windowHandlesAfterOpen.Except(initialWindowHandles).Single();
            driver.SwitchTo().Window(newWindowHandle);
        }

        private static void OpenAnotherWindow(RemoteWebDriver driver)
        {
            driver.FindElement(ExtendedBy.NonCssName("_File")).Click();
            driver.FindElement(ExtendedBy.NonCssName("Open Window 1")).Click();
        }
    }
}
