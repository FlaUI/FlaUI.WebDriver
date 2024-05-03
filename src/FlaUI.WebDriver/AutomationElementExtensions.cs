using System.Diagnostics.CodeAnalysis;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Patterns.Infrastructure;
using static FlaUI.Core.FrameworkAutomationElementBase;

namespace FlaUI.WebDriver;

/// <summary>
/// Gets properties and patterns from an <see cref="AutomationElement"/>.
/// </summary>
/// <remarks>
/// Not so crazy about using reflection here, but it seems it's the only way to query these objects by string?
/// </remarks>
public static class AutomationElementExtensions
{
    public static bool TryGetPattern(this AutomationElement element, string patternName, [NotNullWhen(true)] out IPattern? pattern)
    {
        if (typeof(IFrameworkPatterns).GetProperty(patternName) is { } propertyInfo &&
            propertyInfo.GetValue(element.Patterns) is { } patterns &&
            patterns.GetType().GetProperty("PatternOrDefault") is { } patternPropertyInfo &&
            patternPropertyInfo.GetValue(patterns) is IPattern patternValue)
        {
            pattern = patternValue;
            return true;
        }

        pattern = null;
        return false;
    }

    public static bool TryGetProperty(this AutomationElement element, string propertyName, out object? value)
    {
        var library = element.FrameworkAutomationElement.PropertyIdLibrary;

        if (library.GetType().GetProperty(propertyName) is { } propertyInfo &&
            propertyInfo.GetValue(library) is PropertyId propertyId)
        {
            element.FrameworkAutomationElement.TryGetPropertyValue(propertyId, out value);
            return true;
        }

        value = null;
        return false;
    }

    public static bool TryGetProperty(this IPattern pattern, string propertyName, out object? value)
    {
        if (pattern.GetType().GetProperty(propertyName) is { } propertyInfo)
        {
            value = propertyInfo.GetValue(pattern);
            return true;
        }

        value = null;
        return false;
    }
}
