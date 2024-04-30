﻿using System.Collections.Generic;
using FlaUI.WebDriver.UITests.TestUtil;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;

namespace FlaUI.WebDriver.UITests
{
    [TestFixture]
    public class ExecuteTests
    {
        [Test]
        public void ExecuteScript_PowerShellCommand_ReturnsResult()
        {
            var driverOptions = FlaUIDriverOptions.RootApp();
            using var driver = new WindowsDriver(WebDriverFixture.WebDriverUrl, driverOptions);

            var executeScriptResult = driver.ExecuteScript("powerShell", new Dictionary<string,string> { ["command"] = "1+1" });

            Assert.That(executeScriptResult, Is.EqualTo("2\r\n"));
        }
    }
}
