using FlaUI.Core.WindowsAPI;

namespace FlaUI.WebDriver;

internal class Keys
{
    /// <summary>
    /// Normalized key mapping from https://www.w3.org/TR/webdriver2/#keyboard-actions
    /// </summary>
    private static readonly Dictionary<char, string> s_normalizedKeys = new Dictionary<char, string>()
    {
        { '\uE000', "Unidentified" },
        { '\uE001', "Cancel" },
        { '\uE002', "Help" },
        { '\uE003', "Backspace" },
        { '\uE004', "Tab" },
        { '\uE005', "Clear" },
        { '\uE006', "Return" },
        { '\uE007', "Enter" },
        { '\uE008', "Shift" },
        { '\uE009', "Control" },
        { '\uE00A', "Alt" },
        { '\uE00B', "Pause" },
        { '\uE00C', "Escape" },
        { '\uE00D', " " },
        { '\uE00E', "PageUp" },
        { '\uE00F', "PageDown" },
        { '\uE010', "End" },
        { '\uE011', "Home" },
        { '\uE012', "ArrowLeft" },
        { '\uE013', "ArrowUp" },
        { '\uE014', "ArrowRight" },
        { '\uE015', "ArrowDown" },
        { '\uE016', "Insert" },
        { '\uE017', "Delete" },
        { '\uE018', ";" },
        { '\uE019', "=" },
        { '\uE01A', "0" },
        { '\uE01B', "1" },
        { '\uE01C', "2" },
        { '\uE01D', "3" },
        { '\uE01E', "4" },
        { '\uE01F', "5" },
        { '\uE020', "6" },
        { '\uE021', "7" },
        { '\uE022', "8" },
        { '\uE023', "9" },
        { '\uE024', "*" },
        { '\uE025', "+" },
        { '\uE026', "," },
        { '\uE027', "-" },
        { '\uE028', "." },
        { '\uE029', "/" },
        { '\uE031', "F1" },
        { '\uE032', "F2" },
        { '\uE033', "F3" },
        { '\uE034', "F4" },
        { '\uE035', "F5" },
        { '\uE036', "F6" },
        { '\uE037', "F7" },
        { '\uE038', "F8" },
        { '\uE039', "F9" },
        { '\uE03A', "F10" },
        { '\uE03B', "F11" },
        { '\uE03C', "F12" },
        { '\uE03D', "Meta" },
        { '\uE03E', "Command" },
        { '\uE040', "ZenkakuHankaku" },
        { '\uE050', "Shift" },
        { '\uE051', "Control" },
        { '\uE052', "Alt" },
        { '\uE053', "Meta" },
        { '\uE054', "PageUp" },
        { '\uE055', "PageDown" },
        { '\uE056', "End" },
        { '\uE057', "Home" },
        { '\uE058', "ArrowLeft" },
        { '\uE059', "ArrowUp" },
        { '\uE05A', "ArrowRight" },
        { '\uE05B', "ArrowDown" },
        { '\uE05C', "Insert" },
        { '\uE05D', "Delete" },
    };

    private static readonly Dictionary<char, string> s_keyToCode = new()
    {
        { '`', "Backquote" },
        { '\\', "Backslash" },
        { '\uE003', "Backspace" },
        { '[', "BracketLeft" },
        { ']', "BracketRight" },
        { ',', "Comma" },
        { '0', "Digit0" },
        { '1', "Digit1" },
        { '2', "Digit2" },
        { '3', "Digit3" },
        { '4', "Digit4" },
        { '5', "Digit5" },
        { '6', "Digit6" },
        { '7', "Digit7" },
        { '8', "Digit8" },
        { '9', "Digit9" },
        { '=', "Equal" },
        { 'a', "KeyA" },
        { 'b', "KeyB" },
        { 'c', "KeyC" },
        { 'd', "KeyD" },
        { 'e', "KeyE" },
        { 'f', "KeyF" },
        { 'g', "KeyG" },
        { 'h', "KeyH" },
        { 'i', "KeyI" },
        { 'j', "KeyJ" },
        { 'k', "KeyK" },
        { 'l', "KeyL" },
        { 'm', "KeyM" },
        { 'n', "KeyN" },
        { 'o', "KeyO" },
        { 'p', "KeyP" },
        { 'q', "KeyQ" },
        { 'r', "KeyR" },
        { 's', "KeyS" },
        { 't', "KeyT" },
        { 'u', "KeyU" },
        { 'v', "KeyV" },
        { 'w', "KeyW" },
        { 'x', "KeyX" },
        { 'y', "KeyY" },
        { 'z', "KeyZ" },
        { '-', "Minus" },
        { '.', "Period" },
        { '\'', "Quote" },
        { ';', "Semicolon" },
        { '/', "Slash" },
        { '\uE00A', "AltLeft" },
        { '\uE052', "AltRight" },
        { '\uE009', "ControlLeft" },
        { '\uE051', "ControlRight" },
        { '\uE006', "Enter" },
        { '\uE00B', "Pause" },
        { '\uE03D', "MetaLeft" },
        { '\uE053', "MetaRight" },
        { '\uE008', "ShiftLeft" },
        { '\uE050', "ShiftRight" },
        { ' ', "Space" },
        { '\uE004', "Tab" },
        { '\uE017', "Delete" },
        { '\uE010', "End" },
        { '\uE002', "Help" },
        { '\uE011', "Home" },
        { '\uE016', "Insert" },
        { '\uE00F', "PageDown" },
        { '\uE00E', "PageUp" },
        { '\uE015', "ArrowDown" },
        { '\uE012', "ArrowLeft" },
        { '\uE014', "ArrowRight" },
        { '\uE013', "ArrowUp" },
        { '\uE00C', "Escape" },
        { '\uE031', "F1" },
        { '\uE032', "F2" },
        { '\uE033', "F3" },
        { '\uE034', "F4" },
        { '\uE035', "F5" },
        { '\uE036', "F6" },
        { '\uE037', "F7" },
        { '\uE038', "F8" },
        { '\uE039', "F9" },
        { '\uE03A', "F10" },
        { '\uE03B', "F11" },
        { '\uE03C', "F12" },
        { '\uE019', "NumpadEqual" },
        { '\uE01A', "Numpad0" },
        { '\uE01B', "Numpad1" },
        { '\uE01C', "Numpad2" },
        { '\uE01D', "Numpad3" },
        { '\uE01E', "Numpad4" },
        { '\uE01F', "Numpad5" },
        { '\uE020', "Numpad6" },
        { '\uE021', "Numpad7" },
        { '\uE022', "Numpad8" },
        { '\uE023', "Numpad9" },
        { '\uE025', "NumpadAdd" },
        { '\uE026', "NumpadComma" },
        { '\uE028', "NumpadDecimal" },
        { '\uE029', "NumpadDivide" },
        { '\uE007', "NumpadEnter" },
        { '\uE024', "NumpadMultiply" },
        { '\uE027', "NumpadSubtract" },
    };

    private static readonly Dictionary<char, string> s_shiftedKeyToCode = new()
    {
        { '~', "Backquote" },
        { '|', "Backslash" },
        { '{', "BracketLeft" },
        { '}', "BracketRight" },
        { '<', "Comma" },
        { ')', "Digit0" },
        { '!', "Digit1" },
        { '@', "Digit2" },
        { '#', "Digit3" },
        { '$', "Digit4" },
        { '%', "Digit5" },
        { '^', "Digit6" },
        { '&', "Digit7" },
        { '*', "Digit8" },
        { '(', "Digit9" },
        { '+', "Equal" },
        { 'A', "KeyA" },
        { 'B', "KeyB" },
        { 'C', "KeyC" },
        { 'D', "KeyD" },
        { 'E', "KeyE" },
        { 'F', "KeyF" },
        { 'G', "KeyG" },
        { 'H', "KeyH" },
        { 'I', "KeyI" },
        { 'J', "KeyJ" },
        { 'K', "KeyK" },
        { 'L', "KeyL" },
        { 'M', "KeyM" },
        { 'N', "KeyN" },
        { 'O', "KeyO" },
        { 'P', "KeyP" },
        { 'Q', "KeyQ" },
        { 'R', "KeyR" },
        { 'S', "KeyS" },
        { 'T', "KeyT" },
        { 'U', "KeyU" },
        { 'V', "KeyV" },
        { 'W', "KeyW" },
        { 'X', "KeyX" },
        { 'Y', "KeyY" },
        { 'Z', "KeyZ" },
        { '_', "Minus" },
        { '>', "Period" },
        { '"', "Quote" },
        { ':', "Semicolon" },
        { '?', "Slash" },
        { '\uE00D', "Space" },
        { '\uE05C', "Numpad0" },
        { '\uE056', "Numpad1" },
        { '\uE05B', "Numpad2" },
        { '\uE055', "Numpad3" },
        { '\uE058', "Numpad4" },
        { '\uE05A', "Numpad6" },
        { '\uE057', "Numpad7" },
        { '\uE059', "Numpad8" },
        { '\uE054', "Numpad9" },
        { '\uE05D', "NumpadDecimal" },
    };

    public const char Null = '\uE000';
    public const char Cancel = '\uE001';
    public const char Help = '\uE002';
    public const char Backspace = '\uE003';
    public const char Tab = '\uE004';
    public const char Clear = '\uE005';
    public const char Return = '\uE006';
    public const char Enter = '\uE007';
    public const char Shift = '\uE008';
    public const char LeftShift = '\uE008';
    public const char Control = '\uE009';
    public const char LeftControl = '\uE009';
    public const char Alt = '\uE00A';
    public const char LeftAlt = '\uE00A';
    public const char Pause = '\uE00B';
    public const char Escape = '\uE00C';
    public const char Space = '\uE00D';
    public const char PageUp = '\uE00E';
    public const char PageDown = '\uE00F';
    public const char End = '\uE010';
    public const char Home = '\uE011';
    public const char Left = '\uE012';
    public const char ArrowLeft = '\uE012';
    public const char Up = '\uE013';
    public const char ArrowUp = '\uE013';
    public const char Right = '\uE014';
    public const char ArrowRight = '\uE014';
    public const char Down = '\uE015';
    public const char ArrowDown = '\uE015';
    public const char Insert = '\uE016';
    public const char Delete = '\uE017';
    public const char Semicolon = '\uE018';
    public const char Equal = '\uE019';
    public const char NumberPad0 = '\uE01A';
    public const char NumberPad1 = '\uE01B';
    public const char NumberPad2 = '\uE01C';
    public const char NumberPad3 = '\uE01D';
    public const char NumberPad4 = '\uE01E';
    public const char NumberPad5 = '\uE01F';
    public const char NumberPad6 = '\uE020';
    public const char NumberPad7 = '\uE021';
    public const char NumberPad8 = '\uE022';
    public const char NumberPad9 = '\uE023';
    public const char Multiply = '\uE024';
    public const char Add = '\uE025';
    public const char Separator = '\uE026';
    public const char Subtract = '\uE027';
    public const char Decimal = '\uE028';
    public const char Divide = '\uE029';
    public const char F1 = '\uE031';
    public const char F2 = '\uE032';
    public const char F3 = '\uE033';
    public const char F4 = '\uE034';
    public const char F5 = '\uE035';
    public const char F6 = '\uE036';
    public const char F7 = '\uE037';
    public const char F8 = '\uE038';
    public const char F9 = '\uE039';
    public const char F10 = '\uE03A';
    public const char F11 = '\uE03B';
    public const char F12 = '\uE03C';
    public const char Meta = '\uE03D';
    public const char Command = '\uE03D';
    public const char ZenkakuHankaku = '\uE040';

    /// <summary>
    /// Gets a value indicating whether a key attribute value represents a modifier key.
    /// </summary>
    /// <param name="key">The key attribute value.</param>
    /// <remarks>
    /// Defined in https://www.w3.org/TR/uievents-key/#keys-modifier
    /// </remarks>
    public static bool IsModifier(string key)
    {
        return key is "Alt" or "AltGraph" or "CapsLock" or "Control" or "Fn" or "FnLock" or
            "Meta" or "NumLock" or "ScrollLock" or "Shift" or "Symbol" or "SymbolLock";
    }

    /// <summary>
    /// Gets a value indicating whether a character is shifted.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <remarks>
    /// Defined in https://www.w3.org/TR/webdriver2/#keyboard-actions
    /// </remarks>
    internal static bool IsShiftedChar(char c) => s_shiftedKeyToCode.ContainsKey(c);

    /// <summary>
    /// Gets a value indicating whether a graphene cluster is typeable.
    /// </summary>
    /// <param name="c">The graphene cluster</param>
    /// <remarks>
    /// Defined in https://www.w3.org/TR/webdriver2/#element-send-keys
    /// </remarks>
    public static bool IsTypeable(string c)
    {
        return c.Length == 1 && (s_keyToCode.ContainsKey(c[0]) || s_shiftedKeyToCode.ContainsKey(c[0]));
    }

    /// <summary>
    /// Gets the code for a raw key.
    /// </summary>
    /// <param name="key">The raw key.</param>
    /// <remarks>
    /// Defined in https://www.w3.org/TR/webdriver2/#keyboard-actions
    /// </remarks>
    public static string? GetCode(string key)
    {
        if (key.Length == 1)
        {
            var c = key[0];

            if (s_keyToCode.TryGetValue(c, out var code))
            {
                return code;
            }
            else if (s_shiftedKeyToCode.TryGetValue(c, out code))
            {
                return code;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a normalized key value.
    /// </summary>
    /// <param name="key">The raw key.</param>
    /// <remarks>
    /// Defined in https://www.w3.org/TR/webdriver2/#keyboard-actions
    /// </remarks>
    public static string GetNormalizedKeyValue(string key)
    {
        return key.Length == 1 && s_normalizedKeys.TryGetValue(key[0], out var value) ? value : key;
    }

    /// <summary>
    /// Gets the win32 virtual key code for a key code returned by <see cref="GetCode(string)"/>.
    /// </summary>
    public static VirtualKeyShort GetVirtualKey(string? code)
    {
        return code switch
        {
            "Backquote" => VirtualKeyShort.OEM_3,
            "Backslash" => VirtualKeyShort.OEM_5,
            "Backspace" => VirtualKeyShort.BACK,
            "BracketLeft" => VirtualKeyShort.OEM_4,
            "BracketRight" => VirtualKeyShort.OEM_6,
            "Comma" => VirtualKeyShort.OEM_COMMA,
            "Digit0" => VirtualKeyShort.KEY_0,
            "Digit1" => VirtualKeyShort.KEY_1,
            "Digit2" => VirtualKeyShort.KEY_2,
            "Digit3" => VirtualKeyShort.KEY_3,
            "Digit4" => VirtualKeyShort.KEY_4,
            "Digit5" => VirtualKeyShort.KEY_5,
            "Digit6" => VirtualKeyShort.KEY_6,
            "Digit7" => VirtualKeyShort.KEY_7,
            "Digit8" => VirtualKeyShort.KEY_8,
            "Digit9" => VirtualKeyShort.KEY_9,
            "Equal" => VirtualKeyShort.OEM_PLUS,
            "IntlBackslash" => VirtualKeyShort.OEM_102,
            "KeyA" => VirtualKeyShort.KEY_A,
            "KeyB" => VirtualKeyShort.KEY_B,
            "KeyC" => VirtualKeyShort.KEY_C,
            "KeyD" => VirtualKeyShort.KEY_D,
            "KeyE" => VirtualKeyShort.KEY_E,
            "KeyF" => VirtualKeyShort.KEY_F,
            "KeyG" => VirtualKeyShort.KEY_G,
            "KeyH" => VirtualKeyShort.KEY_H,
            "KeyI" => VirtualKeyShort.KEY_I,
            "KeyJ" => VirtualKeyShort.KEY_J,
            "KeyK" => VirtualKeyShort.KEY_K,
            "KeyL" => VirtualKeyShort.KEY_L,
            "KeyM" => VirtualKeyShort.KEY_M,
            "KeyN" => VirtualKeyShort.KEY_N,
            "KeyO" => VirtualKeyShort.KEY_O,
            "KeyP" => VirtualKeyShort.KEY_P,
            "KeyQ" => VirtualKeyShort.KEY_Q,
            "KeyR" => VirtualKeyShort.KEY_R,
            "KeyS" => VirtualKeyShort.KEY_S,
            "KeyT" => VirtualKeyShort.KEY_T,    
            "KeyU" => VirtualKeyShort.KEY_U,
            "KeyV" => VirtualKeyShort.KEY_V,
            "KeyW" => VirtualKeyShort.KEY_W,
            "KeyX" => VirtualKeyShort.KEY_X,
            "KeyY" => VirtualKeyShort.KEY_Y,
            "KeyZ" => VirtualKeyShort.KEY_Z,    
            "Minus" => VirtualKeyShort.OEM_MINUS,
            "Period" => VirtualKeyShort.OEM_PERIOD,
            "Quote" => VirtualKeyShort.OEM_7,
            "Semicolon" => VirtualKeyShort.OEM_1,
            "Slash" => VirtualKeyShort.OEM_2,
            "AltLeft" => VirtualKeyShort.ALT,
            "AltRight" => VirtualKeyShort.ALT,
            "ControlLeft" => VirtualKeyShort.CONTROL,
            "ControlRight" => VirtualKeyShort.CONTROL,
            "Enter" => VirtualKeyShort.ENTER,
            "Pause" => VirtualKeyShort.PAUSE,
            "MetaLeft" => VirtualKeyShort.LWIN,
            "MetaRight" => VirtualKeyShort.RWIN,
            "ShiftLeft" => VirtualKeyShort.LSHIFT,
            "ShiftRight" => VirtualKeyShort.RSHIFT,
            "Space" => VirtualKeyShort.SPACE,
            "Tab" => VirtualKeyShort.TAB,
            "Delete" => VirtualKeyShort.DELETE,
            "End" => VirtualKeyShort.END,
            "Help" => VirtualKeyShort.HELP,
            "Home" => VirtualKeyShort.HOME,
            "Insert" => VirtualKeyShort.INSERT,
            "PageDown" => VirtualKeyShort.NEXT,
            "PageUp" => VirtualKeyShort.PRIOR,
            "ArrowDown" => VirtualKeyShort.DOWN,
            "ArrowLeft" => VirtualKeyShort.LEFT,
            "ArrowRight" => VirtualKeyShort.RIGHT,
            "ArrowUp" => VirtualKeyShort.UP,
            "Escape" => VirtualKeyShort.ESCAPE,
            "F1" => VirtualKeyShort.F1,
            "F2" => VirtualKeyShort.F2,
            "F3" => VirtualKeyShort.F3,
            "F4" => VirtualKeyShort.F4,
            "F5" => VirtualKeyShort.F5,
            "F6" => VirtualKeyShort.F6,
            "F7" => VirtualKeyShort.F7,
            "F8" => VirtualKeyShort.F8,
            "F9" => VirtualKeyShort.F9,
            "F10" => VirtualKeyShort.F10,
            "F11" => VirtualKeyShort.F11,
            "F12" => VirtualKeyShort.F12,
            "NumpadEqual" => VirtualKeyShort.SEPARATOR,
            "Numpad0" => VirtualKeyShort.NUMPAD0,
            "Numpad1" => VirtualKeyShort.NUMPAD1,
            "Numpad2" => VirtualKeyShort.NUMPAD2,
            "Numpad3" => VirtualKeyShort.NUMPAD3,
            "Numpad4" => VirtualKeyShort.NUMPAD4,
            "Numpad5" => VirtualKeyShort.NUMPAD5,
            "Numpad6" => VirtualKeyShort.NUMPAD6,
            "Numpad7" => VirtualKeyShort.NUMPAD7,
            "Numpad8" => VirtualKeyShort.NUMPAD8,
            "Numpad9" => VirtualKeyShort.NUMPAD9,
            "NumpadAdd" => VirtualKeyShort.ADD,
            "NumpadComma" => VirtualKeyShort.OEM_COMMA,
            "NumpadDecimal" => VirtualKeyShort.DECIMAL,
            "NumpadDivide" => VirtualKeyShort.DIVIDE,
            "NumpadEnter" => VirtualKeyShort.ENTER,
            "NumpadMultiply" => VirtualKeyShort.MULTIPLY,
            "NumpadSubtract" => VirtualKeyShort.SUBTRACT,
            _ => throw WebDriverResponseException.UnsupportedOperation($"Key '{code}' is not supported"),
        };
    }
}