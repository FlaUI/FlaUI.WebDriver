using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using FlaUI.WebDriver.Services;
using NUnit.Framework;

namespace FlaUI.WebDriver.UnitTests.Services
{
    public class ConditionParserTests
    {
        private ConditionParser _conditionParser;
        private ConditionFactory _conditionFactory;

        [SetUp]
        public void Setup()
        {
            _conditionParser = new ConditionParser();
            var automation = new UIA3Automation();
            _conditionFactory = automation.ConditionFactory;
        }

        [Test]
        public void ParseCondition_CssSelectorWithNumericIdUsingUnicodeEscape_ReturnsAutomationIdCondition()
        {
            var cssSelector = @"#\34 b090d48-e3a5-4eb4-bd37-4bd62dfa6e5b";

            var condition = _conditionParser.ParseCondition(_conditionFactory, "css selector", cssSelector);

            Assert.That(condition.Value, Is.EqualTo("4b090d48-e3a5-4eb4-bd37-4bd62dfa6e5b"));
        }

        [Test]
        public void ParseCondition_CssSelectorWithSimpleNumericId_ReturnsAutomationIdCondition()
        {
            var cssSelector = @"#\31 ";

            var condition = _conditionParser.ParseCondition(_conditionFactory, "css selector", cssSelector);

            Assert.That(condition.Value, Is.EqualTo("1"));
        }

        [Test]
        public void ParseCondition_CssSelectorWithEscapedSpecialChars_ReturnsNameCondition()
        {
            var cssSelector = @"*[name=""ListBox\ Item\ \#1""]";

            var condition = _conditionParser.ParseCondition(_conditionFactory, "css selector", cssSelector);

            Assert.That(condition.Value, Is.EqualTo("ListBox Item #1"));
        }

        [Test]
        public void ParseCondition_CssSelectorCompoundSelector_ThrowsUnsupportedOperation()
        {
            var cssSelector = "#foo.bar";

            Assert.Throws<WebDriverResponseException>(() =>
                _conditionParser.ParseCondition(_conditionFactory, "css selector", cssSelector));
        }

        [Test]
        public void ParseCondition_PlainIdStrategy_ReturnsAutomationIdCondition()
        {
            var id = "TextBox";

            var condition = _conditionParser.ParseCondition(_conditionFactory, "id", id);

            Assert.That(condition.Value, Is.EqualTo("TextBox"));
        }

        [Test]
        public void ParseCondition_CssSelectorCompoundAttributeSelector_ThrowsUnsupportedOperation()
        {
            var cssSelector = "[name=\"test\"][class=\"test2\"]";

            Assert.Throws<WebDriverResponseException>(() =>
                _conditionParser.ParseCondition(_conditionFactory, "css selector", cssSelector));
        }
    }
}