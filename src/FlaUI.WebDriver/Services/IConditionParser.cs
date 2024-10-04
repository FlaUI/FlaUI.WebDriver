using FlaUI.Core.Conditions;

namespace FlaUI.WebDriver.Services
{
    public interface IConditionParser
    {
        PropertyCondition ParseCondition(ConditionFactory conditionFactory, string @using, string value);
    }
}
