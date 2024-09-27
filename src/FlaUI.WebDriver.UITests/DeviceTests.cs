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
        public void PushFile_FileDoesNotExist_FileIsPushed()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var file = Path.GetTempFileName();
            File.Delete(file);
            var data_to_write = "aaaaaaaaaaaaaaa";
            driver.PushFile(file, data_to_write);
            Assert.That(File.ReadAllText(file) == data_to_write);
        }


        [Test]
        public void PushFile_FileExist_FileIsOverwritten()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var file = Path.GetTempFileName();
            File.WriteAllText(file, "aaaaaaaaaaaaaaaaaaaa");
            var data_to_write = "bbbbbbbbbbbbbbbbbbb";
            driver.PushFile(file, data_to_write);
            Assert.That(File.ReadAllText(file) == data_to_write);
        }

        [Test]
        public void PushFile_PathEmpty_ErrorRaised()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            Assert.That(
                () => driver.PushFile("", "hello world"),
                Throws.TypeOf<WebDriverException>().With.Message.EqualTo("Parameter path must be provided in the request.")
            );

        }

        [Test]
        public void PullFile_FileDoesNotExist_ErrorRaised()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var file = Path.GetTempFileName();
            File.Delete(file);
            Assert.That(
                () => driver.PullFile(file),
                Throws.TypeOf<WebDriverException>().With.Message.EqualTo($"File {file} does not exist.")
            );
        }


        [Test]
        public void PullFile_PathEmpty_ErrorRaised()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            Assert.That(
                () => driver.PullFile(""),
                Throws.TypeOf<WebDriverException>().With.Message.EqualTo("Parameter path must be provided in the request.")
            );

        }


        [Test]
        public void PullFile_FileExists_ContentPulled()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var file = Path.GetTempFileName();
            var content = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            File.WriteAllText(file, content);
            var remoteContent = Encoding.UTF8.GetString(driver.PullFile(file));
            Assert.That(remoteContent == content);

        }

        [Test]
        public void PullFolder_FolderDoesNotExists_ErrorRaised()
        {
            var tempDir = Path.Join(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            Assert.That(
                () => driver.PullFolder(tempDir),
                Throws.TypeOf<WebDriverException>().With.Message.EqualTo($"File {tempDir} does not exist.")
            );
        }


        [Test]
        public void PullFolder_PathEmpty_ErrorRaised()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            Assert.That(
                () => driver.PullFolder(""),
                Throws.TypeOf<WebDriverException>().With.Message.EqualTo($"Parameter path must be provided in the request.")
            );
        }


        [Test]
        public void PullFolder_FolderExists_ContentPulled()
        {
            var driverOptions = FlaUIAppiumDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var dir = Path.Join(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            for (int i = 0; i < 10; i++)
            {
                driver.PushFile(Path.Join(dir, i.ToString()), $"{i} data");
            }
            var zipBytes = driver.PullFolder(dir);
            using var memoryStream = new MemoryStream(zipBytes);
            using var zip = new ZipArchive(memoryStream);

            for (int i = 0; i < 10; i++)
            {
                var entry = zip.GetEntry(i.ToString());
                using var entryStream = new StreamReader(entry.Open());
                Assert.That(entryStream.ReadToEnd() == $"{i} data");
            }
            Directory.Delete(dir, true);
        }



    }
}
