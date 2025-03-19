﻿using System.Drawing;
using System.Globalization;
using System.Text;
using FlaUI.Core.Input;
using FlaUI.WebDriver.Models;

namespace FlaUI.WebDriver.Services
{
    public class ActionsDispatcher : IActionsDispatcher
    {
        private readonly ILogger<ActionsDispatcher> _logger;

        public ActionsDispatcher(ILogger<ActionsDispatcher> logger)
        {
            _logger = logger;
        }

        public async Task DispatchAction(Session session, Action action)
        {
            switch (action.Type)
            {
                case "pointer":
                    await DispatchPointerAction(session, action);
                    return;
                case "key":
                    await DispatchKeyAction(session, action);
                    return;
                case "wheel":
                    await DispatchWheelAction(session, action);
                    return;
                case "none":
                    await DispatchNullAction(session, action);
                    return;
                default:
                    throw WebDriverResponseException.UnsupportedOperation($"Action type {action.Type} not supported");
            }
        }

        /// <summary>
        /// Implements "dispatch actions for a string" from https://www.w3.org/TR/webdriver2/#element-send-keys
        /// </summary>
        public async Task DispatchActionsForString(
            Session session,
            string inputId,
            KeyInputSource source,
            string text)
        {
            var clusters = StringInfo.GetTextElementEnumerator(text);
            var currentTypeableText = new StringBuilder();

            while (clusters.MoveNext())
            {
                var cluster = clusters.GetTextElement();

                if (cluster == Keys.Null.ToString())
                {
                    await DispatchTypeableString(session, inputId, source, currentTypeableText.ToString());
                    currentTypeableText.Clear();
                    await ClearModifierKeyState(session, inputId);
                }
                else if (Keys.IsModifier(Keys.GetNormalizedKeyValue(cluster)))
                {
                    await DispatchTypeableString(session, inputId, source, currentTypeableText.ToString());
                    currentTypeableText.Clear();

                    var keyDownAction = new Action(
                        new ActionSequence 
                        { 
                            Id = inputId,
                            Type = "key" 
                        },
                        new ActionItem
                        {
                            Type = "keyDown",
                            Value = cluster
                        });

                    await DispatchAction(session, keyDownAction);

                    var undo = keyDownAction.Clone();
                    undo.SubType = "keyUp";

                    // NOTE: According to the spec, the undo action should be added to an "undo actions" list,
                    // but that may be an oversight in the spec: we already have such a thing in the input cancel
                    // list which won't get cleared correctly if we're using a separate "undo actions" list. See
                    // https://github.com/w3c/webdriver/issues/1809.
                    session.InputState.InputCancelList.Add(undo);
                }
                else if (Keys.IsTypeable(cluster))
                {
                    currentTypeableText.Append(cluster);
                }
                else
                {
                    await DispatchTypeableString(session, inputId, source, currentTypeableText.ToString());
                    currentTypeableText.Clear();
                    // TODO: Dispatch composition events.
                }
            }

            if (currentTypeableText.Length > 0)
            {
                await DispatchTypeableString(session, inputId, source, currentTypeableText.ToString());
            }

            await ClearModifierKeyState(session, inputId);
        }

        /// <summary>
        /// Dispatches the release actions for the given input ID.
        /// </summary>
        /// <remarks>
        /// The only part of the spec that mentions this is https://www.w3.org/TR/webdriver2/#release-actions, but the spec
        /// mentions that the input cancel list must be empty before removing an input source in
        /// https://www.w3.org/TR/webdriver2/#input-state so I can only assume that there was an oversight in the spec.
        /// </remarks>
        public async Task DispatchReleaseActions(Session session, string inputId)
        {
            for (var i = session.InputState.InputCancelList.Count - 1; i >= 0; i--)
            {
                var cancelAction = session.InputState.InputCancelList[i];

                if (cancelAction.Id == inputId)
                {
                    await DispatchAction(session, cancelAction);
                    session.InputState.InputCancelList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Implements a variation on "clear the modifier key state" from 
        /// https://www.w3.org/TR/webdriver2/#element-send-keys.
        /// </summary>
        /// <remarks>
        /// https://github.com/w3c/webdriver/issues/1809 
        /// </remarks>
        private Task ClearModifierKeyState(Session session, string inputId) => DispatchReleaseActions(session, inputId);

        /// <summary>
        /// Implements "dispatch the events for a typeable string" from https://www.w3.org/TR/webdriver2/#element-send-keys
        /// </summary>
        private async Task DispatchTypeableString(
            Session session,
            string inputId,
            KeyInputSource source,
            string text)
        {
            foreach (var c in text)
            {
                var isShifted = Keys.IsShiftedChar(c);

                if (isShifted != source.Shift)
                {
                    var action = new Action(
                        new ActionSequence 
                        { 
                            Id = inputId,
                            Type = "key" 
                        },
                        new ActionItem
                        {
                            Type = source.Shift ? "keyUp" : "keyDown",
                            Value = Keys.LeftShift.ToString(),
                        });
                    await DispatchAction(session, action);
                }

                var keyDownAction = new Action(
                    new ActionSequence 
                    {
                        Id = inputId,
                        Type = "key" 
                    },
                    new ActionItem
                    {
                        Type = "keyDown",
                        Value = c.ToString(),
                    });

                var keyUpAction = keyDownAction.Clone();
                keyUpAction.SubType = "keyUp";

                await DispatchAction(session, keyDownAction);
                await DispatchAction(session, keyUpAction);
            }
        }

        private static async Task DispatchNullAction(Session session, Action action)
        {
            switch (action.SubType)
            {
                case "pause":
                    await Task.Yield();
                    return;
                default:
                    throw WebDriverResponseException.InvalidArgument($"Null action subtype {action.SubType} unknown");
            }
        }

        /// <summary>
        /// Dispatches a keyDown, keyUp or pause action from https://www.w3.org/TR/webdriver2/#keyboard-actions
        /// </summary>
        private async Task DispatchKeyAction(Session session, Action action)
        {
            if (action.Value == null)
            {
                return;
            }

            var source = session.InputState.GetInputSource<KeyInputSource>(action.Id) ?? 
                throw WebDriverResponseException.UnknownError($"Input source for key action '{action.Id}' not found.");

            switch (action.SubType)
            {
                case "keyDown":
                    {
                        var key = Keys.GetNormalizedKeyValue(action.Value);
                        var code = Keys.GetCode(action.Value);
                        var virtualKey = Keys.GetVirtualKey(code);
                        _logger.LogDebug("Dispatching key down action, key '{Value}' with ID '{Id}'", code, action.Id);

                        if (key == "Alt")
                        {
                            source.Alt = true;
                        }
                        else if (key == "Shift")
                        {
                            source.Shift = true;
                        }
                        else if (key == "Control")
                        {
                            source.Ctrl = true;
                        }
                        else if (key == "Meta")
                        {
                            source.Meta = true;
                        }

                        source.Pressed.Add(action.Value);

                        Keyboard.Press(virtualKey);

                        var cancelAction = action.Clone();
                        cancelAction.SubType = "keyUp";
                        session.InputState.InputCancelList.Add(cancelAction);

                        // HACK: Adding a small delay after each key press because otherwise the key press
                        // seems to sometimes appear after the key action completes.
                        await Task.Delay(10);
                        await Task.Yield();
                        return;
                    }
                case "keyUp":
                    {
                        var key = Keys.GetNormalizedKeyValue(action.Value);
                        var code = Keys.GetCode(action.Value);
                        var virtualKey = Keys.GetVirtualKey(code);
                        _logger.LogDebug("Dispatching key up action, key '{Value}' with ID '{Id}'", code, action.Id);

                        if (key == "Alt")
                        {
                            source.Alt = false;
                        }
                        else if (key == "Shift")
                        {
                            source.Shift = false;
                        }
                        else if (key == "Control")
                        {
                            source.Ctrl = false;
                        }
                        else if (key == "Meta")
                        {
                            source.Meta = false;
                        }

                        source.Pressed.Remove(action.Value);

                        Keyboard.Release(virtualKey);

                        // HACK: Adding a small delay after each key press because otherwise the key press
                        // seems to sometimes appear after the key action completes.
                        await Task.Delay(10);

                        await Task.Yield();
                        return;
                    }
                case "pause":
                    await Task.Yield();
                    return;
                default:
                    throw WebDriverResponseException.InvalidArgument($"Pointer action subtype {action.SubType} unknown");
            }
        }

        private async Task DispatchWheelAction(Session session, Action action)
        {

            switch (action.SubType)
            {
                case "scroll":
                    _logger.LogDebug("Dispatching wheel scroll action, coordinates ({X},{Y}), delta ({DeltaX},{DeltaY}) with ID '{Id}'", action.X, action.Y, action.DeltaX, action.DeltaY, action.Id);
                    if (action.X == null || action.Y == null)
                    {
                        throw WebDriverResponseException.InvalidArgument("For wheel scroll, X and Y are required");
                    }
                    Mouse.MoveTo(action.X.Value, action.Y.Value);
                    if (action.DeltaX == null || action.DeltaY == null)
                    {
                        throw WebDriverResponseException.InvalidArgument("For wheel scroll, delta X and delta Y are required");
                    }
                    if (action.DeltaY != 0)
                    {
                        Mouse.Scroll(action.DeltaY.Value);
                    }
                    if (action.DeltaX != 0)
                    {
                        Mouse.HorizontalScroll(action.DeltaX.Value);
                    }
                    return;
                case "pause":
                    await Task.Yield();
                    return;
                default:
                    throw WebDriverResponseException.InvalidArgument($"Wheel action subtype {action.SubType} unknown");
            }
        }

        private async Task DispatchPointerAction(Session session, Action action)
        {

            switch (action.SubType)
            {
                case "pointerMove":
                    _logger.LogDebug("Dispatching pointer move action, coordinates ({X},{Y}) with origin {Origin}, with ID '{Id}'", action.X, action.Y, action.Origin, action.Id);
                    var point = GetCoordinates(session, action);
                    Mouse.MoveTo(point);
                    await Task.Yield();
                    return;
                case "pointerDown":
                    _logger.LogDebug("Dispatching pointer down action, button {Button}, with ID '{Id}'", action.Button, action.Id);
                    Mouse.Down(GetMouseButton(action.Button));
                    var cancelAction = action.Clone();
                    cancelAction.SubType = "pointerUp";
                    session.InputState.InputCancelList.Add(cancelAction);
                    await Task.Yield();
                    return;
                case "pointerUp":
                    _logger.LogDebug("Dispatching pointer up action, button {Button}, with ID '{Id}'", action.Button, action.Id);
                    Mouse.Up(GetMouseButton(action.Button));
                    await Task.Yield();
                    return;
                case "pause":
                    await Task.Yield();
                    return;
                default:
                    throw WebDriverResponseException.UnsupportedOperation($"Pointer action subtype {action.Type} not supported");
            }
        }

        private static Point GetCoordinates(Session session, Action action)
        {
            var origin = action.Origin ?? "viewport";

            switch (origin)
            {
                case "viewport":
                    if (action.X == null || action.Y == null)
                    {
                        throw WebDriverResponseException.InvalidArgument("For pointer move, X and Y are required");
                    }

                    return new Point(action.X.Value, action.Y.Value);
                case "pointer":
                    if (action.X == null || action.Y == null)
                    {
                        throw WebDriverResponseException.InvalidArgument("For pointer move, X and Y are required");
                    }

                    var current = Mouse.Position;
                    return new Point(current.X + action.X.Value, current.Y + action.Y.Value);
                case Dictionary<string, string> originMap:
                    if (originMap.TryGetValue("element-6066-11e4-a52e-4f735466cecf", out var elementId))
                    {
                        if (session.FindKnownElementById(elementId) is { } element)
                        {
                            var bounds = element.BoundingRectangle;
                            var x = bounds.Left + (bounds.Width / 2) + (action.X ?? 0);
                            var y = bounds.Top + (bounds.Height / 2) + (action.Y ?? 0);
                            return new(x, y);
                        }

                        throw WebDriverResponseException.InvalidArgument(
                            $"An unknown element ID '{elementId}' provided for action item '{action.Type}'.");
                    }

                    throw WebDriverResponseException.InvalidArgument(
                        $"An unknown element '{origin}' provided for action item '{action.Type}'.");
                default:
                    throw WebDriverResponseException.InvalidArgument(
                        $"Unknown origin type '{origin}' provided for action item '{action.Type}'.");
            }
        }

        private static MouseButton GetMouseButton(int? button)
        {
            if (button == null)
            {
                throw WebDriverResponseException.InvalidArgument($"Pointer action button argument missing");
            }
            switch (button)
            {
                case 0: return MouseButton.Left;
                case 1: return MouseButton.Middle;
                case 2: return MouseButton.Right;
                case 3: return MouseButton.XButton1;
                case 4: return MouseButton.XButton2;
                default:
                    throw WebDriverResponseException.UnsupportedOperation($"Pointer button {button} not supported");
            }
        }

        private async Task<bool> KeyboardTypeWithTimeout(string text, int timeoutMilliseconds = 50)
        {
            var typeTask = Task.Run(() => Keyboard.Type(text));
            var delayTask = Task.Delay(timeoutMilliseconds);
            var completedTask = await Task.WhenAny(typeTask, delayTask);
            if (completedTask == typeTask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task DispatchActionsForStringUsingFlaUICore(Session session, string inputId, KeyInputSource source, string text)
        {
            if (!await KeyboardTypeWithTimeout(text)) {
                _logger.LogDebug("Keyboard typing error for {text}", text);
                throw WebDriverResponseException.UnknownError($"Keyboard typing error for {text}");
            }
            await Task.Delay(50);
        }
    }
}
