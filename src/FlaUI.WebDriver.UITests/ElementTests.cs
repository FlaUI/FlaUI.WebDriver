using System.Threading;
using FlaUI.WebDriver.UITests.TestUtil;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace FlaUI.WebDriver.UITests
{
    [TestFixture]
    public class ElementTests
    {
        [TestCase("TextBox", "Test TextBox")]
        [TestCase("PasswordBox", "●●●●●●●●●●")]
        [TestCase("EditableCombo", "Item 1")]
        [TestCase("NonEditableCombo", "Item 1")]
        [TestCase("ListBox", "ListBox Item #1")]
        [TestCase("SimpleCheckBox", "Test Checkbox")]
        [TestCase("ThreeStateCheckBox", "3-Way Test Checkbox")]
        [TestCase("RadioButton1", "RadioButton1")]
        [TestCase("RadioButton2", "RadioButton2")]
        [TestCase("Slider", "5")]
        [TestCase("InvokableButton", "Invoke me!")]
        [TestCase("PopupToggleButton1", "Popup Toggle 1")]
        [TestCase("Label", "Menu Item Checked")]
        public void GetText_Returns_Correct_Text(string elementAccessibilityId, string expectedValue)
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId(elementAccessibilityId));
            var text = element.Text;

            Assert.That(text, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetText_Returns_Text_For_Multiple_Selection()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("ListBox"));

            new Actions(driver)
                .MoveToElement(element)
                .Click()
                .KeyDown(Keys.Control)
                .KeyDown("a")
                .KeyUp("a")
                .KeyUp(Keys.Control)
                .Perform();

            var text = element.Text;

            // Seems that the order in which the selected items are returned is not guaranteed.
            Assert.That(text, Is.EqualTo("ListBox Item #1, ListBox Item #2").Or.EqualTo("ListBox Item #2, ListBox Item #1"));
        }

        [Test]
        public void GetText_Returns_Empty_String_For_No_Selection()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("ListBox"));
            var item = driver.FindElement(ExtendedBy.Name("ListBox Item #1"));

            new Actions(driver)
                .MoveToElement(item)
                .KeyDown(Keys.Control)
                .Click()
                .KeyUp(Keys.Control)
                .Perform();

            var text = element.Text;

            Assert.That(text, Is.Empty);
        }

        [Test]
        public void Selected_NotCheckedCheckbox_ReturnsFalse()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("SimpleCheckBox"));

            var selected = element.Selected;

            Assert.That(selected, Is.False);
        }

        [Test]
        public void Selected_CheckedCheckbox_ReturnsTrue()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("SimpleCheckBox"));
            element.Click();

            var selected = element.Selected;

            Assert.That(selected, Is.True);
        }

        [Test]
        public void Selected_NotCheckedRadioButton_ReturnsFalse()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("RadioButton1"));

            var selected = element.Selected;

            Assert.That(selected, Is.False);
        }

        [Test]
        public void Selected_CheckedRadioButton_ReturnsTrue()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("RadioButton1"));
            element.Click();

            var selected = element.Selected;

            Assert.That(selected, Is.True);
        }

        [Test]
        public void SendKeys_Default_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            element.SendKeys("Hello World!");

            Assert.That(element.Text, Is.EqualTo("Hello World!Test TextBox"));
        }

        [Test]
        public void SendKeys_ShiftedCharacter_ShiftIsReleased()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            element.SendKeys("!");
            element.SendKeys("1");

            Assert.That(element.Text, Is.EqualTo("!1Test TextBox"));
        }

        [Test]
        public void SendKeys_DownArrow_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("NonEditableCombo"));

            element.SendKeys(Keys.Down);

            Assert.That(element.Text, Is.EqualTo("Item 2"));
        }

        [Test, Ignore("Alt key combinations currently fail due to https://github.com/FlaUI/FlaUI/issues/320")]
        public void SendKeys_AltDownArrowEscape_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("NonEditableCombo"));
            var expandCollapseState = element.GetDomAttribute("ExpandCollapse.ExpandCollapseState");

            Assert.That(expandCollapseState, Is.EqualTo("Collapsed"));

            element.SendKeys(Keys.Alt + Keys.Down);

            Assert.That(expandCollapseState, Is.EqualTo("Expanded"));

            element.SendKeys(Keys.Escape);

            Assert.That(expandCollapseState, Is.EqualTo("Collapsed"));
        }

        [Test]
        public void Clear_Default_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            element.Clear();

            Assert.That(element.Text, Is.EqualTo(""));
        }

        [Test]
        public void Click_Default_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("InvokableButton"));

            element.Click();

            Assert.That(element.Text, Is.EqualTo("Invoked!"));
        }

        [Test]
        public void GetElementRect_Default_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("EditableCombo"));

            var scaling = TestApplication.GetScaling(driver);
            var location = element.Location;
            var size = element.Size;

            var windowLocation = driver.Manage().Window.Position;
            Assert.That(location.X, Is.InRange(windowLocation.X + (253 * scaling), windowLocation.X + (257 * scaling)));
            Assert.That(location.Y, Is.InRange(windowLocation.Y + (132 * scaling), windowLocation.Y + (136 * scaling)));
            Assert.That(size.Width, Is.EqualTo(120 * scaling));
            Assert.That(size.Height, Is.EqualTo(22 * scaling));
        }

        [TestCase("TextBox")]
        [TestCase("PasswordBox")]
        [TestCase("EditableCombo")]
        [TestCase("NonEditableCombo")]
        [TestCase("ListBox")]
        [TestCase("SimpleCheckBox")]
        [TestCase("ThreeStateCheckBox")]
        [TestCase("RadioButton1")]
        [TestCase("RadioButton2")]
        [TestCase("Slider")]
        [TestCase("InvokableButton")]
        [TestCase("PopupToggleButton1")]
        [TestCase("Label")]
        public void GetElementEnabled_Enabled_ReturnsTrue(string elementAccessibilityId)
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId(elementAccessibilityId));

            var enabled = element.Enabled;

            Assert.That(enabled, Is.True);
        }

        [TestCase("TextBox")]
        [TestCase("PasswordBox")]
        [TestCase("EditableCombo")]
        [TestCase("NonEditableCombo")]
        [TestCase("ListBox")]
        [TestCase("SimpleCheckBox")]
        [TestCase("ThreeStateCheckBox")]
        [TestCase("RadioButton1")]
        [TestCase("RadioButton2")]
        [TestCase("Slider")]
        [TestCase("InvokableButton")]
        [TestCase("PopupToggleButton1")]
        [TestCase("Label")]
        public void GetElementEnabled_Disabled_ReturnsFalse(string elementAccessibilityId)
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            driver.FindElement(ExtendedBy.NonCssName("_Edit")).Click();
            driver.FindElement(ExtendedBy.NonCssName("Disable Form")).Click();
            var element = driver.FindElement(ExtendedBy.AccessibilityId(elementAccessibilityId));

            var enabled = element.Enabled;

            Assert.That(enabled, Is.False);
        }

        [Test]
        public void ActiveElement_Default_IsSupported()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("InvokableButton"));
            element.Click();

            var activeElement = driver.SwitchTo().ActiveElement();

            Assert.That(activeElement.Text, Is.EqualTo("Invoked!"));
        }

        [TestCase(["ClassName", "TextBox"])]
        [TestCase(["FrameworkId", "WPF"])]
        [TestCase(["NonExistent", null])]
        public void GetAttribute_TextBox_ReturnsValue(string attributeName, string expectedValue)
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("TextBox"));

            var value = element.GetDomAttribute(attributeName);

            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetAttribute_DesktopElement_ReturnsAttribute()
        {
            var driverOptions = FlaUIDriverOptions.RootApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(By.Name("Taskbar"));

            var result = element.GetDomAttribute("ClassName");

            Assert.That(result, Is.EqualTo("Shell_TrayWnd"));
        }

        [Test]
        public void GetAttribute_PatternProperty_ReturnsValue()
        {
            var driverOptions = FlaUIDriverOptions.TestApp();
            using var driver = new RemoteWebDriver(WebDriverFixture.WebDriverUrl, driverOptions);
            var element = driver.FindElement(ExtendedBy.AccessibilityId("SimpleCheckBox"));

            var value = element.GetDomAttribute("Toggle.ToggleState");

            Assert.That(value, Is.EqualTo("Off"));

            element.Click();

            value = element.GetDomAttribute("Toggle.ToggleState");

            Assert.That(value, Is.EqualTo("On"));

            element.Click();
        }
    }
}
