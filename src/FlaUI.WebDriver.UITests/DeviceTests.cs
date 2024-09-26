using FlaUI.WebDriver.UITests.TestUtil;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FlaUI.WebDriver.UITests
{
    [TestFixture]
    public class DeviceTests
    {
        [Test]
        public void PushPullFile()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var file = Path.GetTempFileName();
            var data_to_write = "hello world";
            
            // file was upload to temp directory
            driver.PushFile(file, data_to_write);
            Assert.That(File.ReadAllText(file) == data_to_write);

            // pull the file back
            var pulled_data = Encoding.UTF8.GetString(driver.PullFile(file));
            Assert.That(pulled_data == data_to_write);
            File.Delete(file);
        }

        [Test]
        public void PullFolder()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var dir = Path.Join(Path.GetTempPath(), "FlaUiWDPullDir");
            for (int i = 0; i < 10; i++)
            {
                driver.PushFile(Path.Join(dir, i.ToString()), $"{i} data");
            }
            var zip_bytes = driver.PullFolder(dir);
            using var memoryStream = new MemoryStream(zip_bytes);
            using var zip = new ZipArchive(memoryStream);

            for (int i = 0; i < 10; i++)
            {
                var entry = zip.GetEntry(i.ToString());
                using var entry_stream = new StreamReader(entry.Open());
                Assert.That(entry_stream.ReadToEnd() == $"{i} data");
            }
            Directory.Delete(dir, true);
        }

        [Test]
        public void PushFile_PathEmpty()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(
                () => driver.PushFile("", "hello world"),
                Throws.TypeOf<WebDriverException>().With.Message.EqualTo("Parameter path must be provided in the request.")
            );

        }
        [Test]
        public void PullFile_PathEmpty()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(
                () => driver.PullFile(""),
                Throws.TypeOf<WebDriverException>().With.Message.EqualTo("Parameter path must be provided in the request.")
            );

        }
        [Test]
        public void PullFolder_PathEmpty()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            Assert.That(
                () => driver.PullFolder(""),
                Throws.TypeOf<WebDriverException>().With.Message.EqualTo("Parameter path must be provided in the request.")
            );

        }

    }
}
