using FlaUI.WebDriver.Services;
using NUnit.Framework;

namespace FlaUI.WebDriver.UnitTests.Services
{
    public class ConditionParserTests
    {
        [TestCase("[name=\"2\"]")]
        [TestCase("*[name=\"2\"]")]
        [TestCase("*[name = \"2\"]")]
        public void ParseCondition_ByCssAttributeName_ReturnsCondition(string selector)
        {
            var parser = new ConditionParser();
            var uia3 = new UIA3.UIA3Automation();

            var result = parser.ParseCondition(uia3.ConditionFactory, "css selector", selector);

            Assert.That(result.Property, Is.EqualTo(uia3.PropertyLibrary.Element.Name));
            Assert.That(result.Value, Is.EqualTo("2"));
        }
    }
}
