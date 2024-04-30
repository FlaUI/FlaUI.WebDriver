using System.Diagnostics.CodeAnalysis;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Patterns;
using FlaUI.Core.Patterns.Infrastructure;

namespace FlaUI.WebDriver;

public static class AutomationElementExtensions
{
    public static bool TryGetPattern(this AutomationElement element, string patternName, [NotNullWhen(true)] out IPattern? pattern)
    {
        static bool TryGet<T>(IAutomationPattern<T> pattern, out IPattern result) where T : IPattern
        {
            pattern.TryGetPattern(out var p);
            result = p;
            return p is not null;
        }

        if (patternName == nameof(element.Patterns.Annotation))
        {
            return TryGet(element.Patterns.Annotation, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Dock))
        {
            return TryGet(element.Patterns.Dock, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Drag))
        {
            return TryGet(element.Patterns.Drag, out pattern);
        }
        else if (patternName == nameof(element.Patterns.DropTarget))
        {
            return TryGet(element.Patterns.DropTarget, out pattern);
        }
        else if (patternName == nameof(element.Patterns.ExpandCollapse))
        {
            return TryGet(element.Patterns.ExpandCollapse, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Grid))
        {
            return TryGet(element.Patterns.Grid, out pattern);
        }
        else if (patternName == nameof(element.Patterns.GridItem))
        {
            return TryGet(element.Patterns.GridItem, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Invoke))
        {
            return TryGet(element.Patterns.Invoke, out pattern);
        }
        else if (patternName == nameof(element.Patterns.ItemContainer))
        {
            return TryGet(element.Patterns.ItemContainer, out pattern);
        }
        else if (patternName == nameof(element.Patterns.MultipleView))
        {
            return TryGet(element.Patterns.MultipleView, out pattern);
        }
        else if (patternName == nameof(element.Patterns.RangeValue))
        {
            return TryGet(element.Patterns.RangeValue, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Scroll))
        {
            return TryGet(element.Patterns.Scroll, out pattern);
        }
        else if (patternName == nameof(element.Patterns.ScrollItem))
        {
            return TryGet(element.Patterns.ScrollItem, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Selection))
        {
            return TryGet(element.Patterns.Selection, out pattern);
        }
        else if (patternName == nameof(element.Patterns.SelectionItem))
        {
            return TryGet(element.Patterns.SelectionItem, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Spreadsheet))
        {
            return TryGet(element.Patterns.Spreadsheet, out pattern);
        }
        else if (patternName == nameof(element.Patterns.SpreadsheetItem))
        {
            return TryGet(element.Patterns.SpreadsheetItem, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Styles))
        {
            return TryGet(element.Patterns.Styles, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Table))
        {
            return TryGet(element.Patterns.Table, out pattern);
        }
        else if (patternName == nameof(element.Patterns.TableItem))
        {
            return TryGet(element.Patterns.TableItem, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Text))
        {
            return TryGet(element.Patterns.Text, out pattern);
        }
        else if (patternName == nameof(element.Patterns.TextChild))
        {
            return TryGet(element.Patterns.TextChild, out pattern);
        }
        else if (patternName == nameof(element.Patterns.TextEdit))
        {
            return TryGet(element.Patterns.TextEdit, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Text2))
        {
            return TryGet(element.Patterns.Text2, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Toggle))
        {
            return TryGet(element.Patterns.Toggle, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Transform))
        {
            return TryGet(element.Patterns.Transform, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Value))
        {
            return TryGet(element.Patterns.Value, out pattern);
        }
        else if (patternName == nameof(element.Patterns.Window))
        {
            return TryGet(element.Patterns.Window, out pattern);
        }

        pattern = null;
        return false;
    }

    public static bool TryGetProperty(this AutomationElement element, string propertyName, out object? value)
    {
        var library = element.FrameworkAutomationElement.PropertyIdLibrary;

        // Not so crazy about using reflection here, but it seems it's the only way to get the property ID from a string?
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
