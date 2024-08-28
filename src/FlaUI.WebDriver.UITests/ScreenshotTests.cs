using FlaUI.WebDriver.UITests.TestUtil;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using System.Drawing;
using System.IO;

namespace FlaUI.WebDriver.UITests
{
    public class ScreenshotTests
    {
        [Test]
        public void GetScreenshot_FromDesktop_ReturnsScreenshot()
        {
            var driverOptions = FlaUIDriverOptions.RootApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var screenshot = driver.GetScreenshot();

            Assert.That(screenshot.AsByteArray.Length, Is.Not.Zero);
        }

        [Test]
        public void GetScreenshot_FromApplication_ReturnsScreenshotOfCurrentWindow()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var screenshot = driver.GetScreenshot();

            Assert.That(screenshot.AsByteArray.Length, Is.Not.Zero);
            using var stream = new MemoryStream(screenshot.AsByteArray);
            using var image = new Bitmap(stream);
            Assert.That(image.Size, Is.EqualTo(driver.Manage().Window.Size));
        }
    }
}
