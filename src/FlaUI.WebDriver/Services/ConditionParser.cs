using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System.Text.RegularExpressions;

namespace FlaUI.WebDriver.Services
{
    public class ConditionParser : IConditionParser
    {
        /// <summary>
        /// Based on https://www.w3.org/TR/CSS21/grammar.html (see also https://www.w3.org/TR/CSS22/grammar.html)
        /// Limitations: 
        /// - Multiple selectors are not supported.
        /// </summary>
        private static Regex SimpleCssIdSelectorRegex = new Regex(@"^#(?<name>(?<nmchar>[_a-z0-9-]|[\240-\377]|(?<escape>\\[^\r\n\f0-9a-f])|(?<unicode>\\[0-9a-fA-F]{1,6}\s?))+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Based on https://www.w3.org/TR/CSS21/grammar.html (see also https://www.w3.org/TR/CSS22/grammar.html)
        /// Limitations: 
        /// - Multiple selectors are not supported.
        /// </summary>
        private static Regex SimpleCssClassSelectorRegex = new Regex(@"^\.(?<ident>-?(?<nmstart>[_a-z]|[\240-\377])(?<nmchar>[_a-z0-9-]|[\240-\377]|(?<escape>\\[^\r\n\f0-9a-f])|(?<unicode>\\[0-9a-fA-F]{1,6}\s?))*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Based on https://www.w3.org/TR/CSS21/grammar.html (see also https://www.w3.org/TR/CSS22/grammar.html)
        /// Limitations: 
        /// - Escape characters in the attribute name are not supported.
        /// - Multiple selectors are not supported.
        /// - Attribute presence selector (e.g. `[name]`) not supported.
        /// - Attribute equals attribute (e.g. `[name=value]`) not supported.
        /// - ~= or |= not supported.
        /// </summary>
        private static Regex SimpleCssAttributeSelectorRegex = new Regex(@"^\*?\[\s*(?<ident>-?(?<nmstart>[_a-z]|[\240-\377])(?<nmchar>[_a-z0-9-]|[\240-\377])*)\s*=\s*(?<string>(?<string1>""(?<string1value>([^\n\r\f\\""]|(?<escape>\\[^\r\n\f0-9a-f])|(?<unicode>\\[0-9a-fA-F]{1,6}\s?))*)"")|(?<string2>'(?<string2value>([^\n\r\f\\']|(?<escape>\\[^\r\n\f0-9a-f])|(?<unicode>\\[0-9a-fA-F]{1,6}\s?))*)'))\s*\]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Based on https://www.w3.org/TR/CSS21/grammar.html (see also https://www.w3.org/TR/CSS22/grammar.html)
        /// Matches simple escape characters (e.g., \#)
        /// </summary>
        private static Regex SimpleCssEscapeCharacterRegex = new Regex(@"\\[^\r\n\f0-9a-f]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches CSS unicode escape sequences (e.g., \34 or \000034 followed by optional space)
        /// </summary>
        private static Regex CssUnicodeEscapeRegex = new Regex(@"\\([0-9a-fA-F]{1,6})\s?", RegexOptions.Compiled);

        public PropertyCondition ParseCondition(ConditionFactory conditionFactory, string @using, string value)
        {
            switch (@using)
            {
                case "id":
                case "accessibility id":
                    return conditionFactory.ByAutomationId(value);
                case "name":
                    return conditionFactory.ByName(value);
                case "class name":
                    return conditionFactory.ByClassName(value);
                case "link text":
                    return conditionFactory.ByText(value);
                case "partial link text":
                    return conditionFactory.ByText(value, PropertyConditionFlags.MatchSubstring);
                case "tag name":
                    return conditionFactory.ByControlType(Enum.Parse<ControlType>(value));
                case "css selector":
                    var cssIdSelectorMatch = SimpleCssIdSelectorRegex.Match(value);
                    if (cssIdSelectorMatch.Success)
                    {
                        return conditionFactory.ByAutomationId(ReplaceCssEscapedCharacters(value.Substring(1)));
                    }
                    var cssClassSelectorMatch = SimpleCssClassSelectorRegex.Match(value);
                    if (cssClassSelectorMatch.Success)
                    {
                        return conditionFactory.ByClassName(ReplaceCssEscapedCharacters(value.Substring(1)));
                    }
                    var cssAttributeSelectorMatch = SimpleCssAttributeSelectorRegex.Match(value);
                    if (cssAttributeSelectorMatch.Success)
                    {
                        var attributeValue = ReplaceCssEscapedCharacters(cssAttributeSelectorMatch.Groups["string1value"].Success ?
                            cssAttributeSelectorMatch.Groups["string1value"].Value :
                            cssAttributeSelectorMatch.Groups["string2value"].Value);
                        if (cssAttributeSelectorMatch.Groups["ident"].Value == "name")
                        {
                            return conditionFactory.ByName(attributeValue);
                        }
                    }
                    throw WebDriverResponseException.UnsupportedOperation($"Selector strategy 'css selector' with value '{value}' is not supported");
                default:
                    throw WebDriverResponseException.UnsupportedOperation($"Selector strategy '{@using}' is not supported");
            }
        }

        private static string ReplaceCssEscapedCharacters(string value)
        {
            var result = CssUnicodeEscapeRegex.Replace(value, m => 
            {
                var hexValue = m.Groups[1].Value;
                var decodedChar = ((char)Convert.ToInt32(hexValue, 16)).ToString();
                return decodedChar;
            });

            result = SimpleCssEscapeCharacterRegex.Replace(result, match => match.Value.Substring(1));
            
            return result;
        }
    }
}
