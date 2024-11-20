# FlaUI.WebDriver

[![build](https://github.com/FlaUI/FlaUI.WebDriver/actions/workflows/build.yml/badge.svg)](https://github.com/FlaUI/FlaUI.WebDriver/actions/workflows/build.yml)
[![CodeQL](https://github.com/FlaUI/FlaUI.WebDriver/actions/workflows/codeql.yml/badge.svg)](https://github.com/FlaUI/FlaUI.WebDriver/actions/workflows/codeql.yml)
![GitHub License](https://img.shields.io/badge/license-MIT-blue.svg)
![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen)

FlaUI.WebDriver is a [W3C WebDriver2](https://www.w3.org/TR/webdriver2/) implementation using FlaUI's automation. It currently only supports UIA3.

> [!IMPORTANT]
> This WebDriver implementation is EXPERIMENTAL. It is not feature complete and may not implement all features correctly.

## Motivation

- [Microsoft's WinAppDriver](https://github.com/microsoft/WinAppDriver) used by [Appium Windows Driver](https://github.com/appium/appium-windows-driver) has many open issues, is [not actively maintained](https://github.com/microsoft/WinAppDriver/issues/1550) and [is not yet open source after many requests](https://github.com/microsoft/WinAppDriver/issues/1371).
  It implements [the obsolete JSON Wire Protocol](https://www.selenium.dev/documentation/legacy/json_wire_protocol/) by Selenium and not the new W3C WebDriver standard.
  When using it I stumbled upon various very basic issues, such as [that click doesn't always work](https://github.com/microsoft/WinAppDriver/issues/654).
- [kfrajtak/WinAppDriver](https://github.com/kfrajtak/WinAppDriver) is an open source alternative, but its technology stack is outdated (.NET Framework, UIAComWrapper, AutoItX.Dotnet).
- W3C WebDriver is a standard that gives many options of automation frameworks such as [WebdriverIO](https://github.com/webdriverio/webdriverio) and [Selenium](https://github.com/SeleniumHQ/selenium).
  It allows to write test automation in TypeScript, Java or other languages of preference (using FlaUI requires C# knowledge).
- It is open source! Any missing command can be implemented quickly by raising a Pull Request.

## Capabilities

The following capabilities are supported:

| Capability Name                    | Description                                                                                                                                                                                                                                                                                                                       | Example value                                                                      |
| ---------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| platformName                       | Must be set to `windows` (case-insensitive).                                                                                                                                                                                                                                                                                      | `windows`                                                                          |
| appium:automationName              | Must be set to `FlaUI` (case-insensitive).                                                                                                                                                                                                                                                                                        | `FlaUI`                                                                            |
| appium:app                         | The path to the application, or in case of an UWP app, `<package family name>!App`. It is also possible to set app to `Root`. In such case the session will be invoked without any explicit target application. Either this capability, `appTopLevelWindow` or `appTopLevelWindowTitleMatch` must be provided on session startup. | `C:\Windows\System32\notepad.exe`, `Microsoft.WindowsCalculator_8wekyb3d8bbwe!App` |
| appium:appArguments                | Application arguments string, for example `/?`.                                                                                                                                                                                                                                                                                   |                                                                                    |
| appium:appWorkingDir               | Full path to the folder, which is going to be set as the working dir for the application under test. This is only applicable for classic apps. When this is used the `appium:app` may contain a relative file path.                                                                                                               | `C:\MyApp\`                                                                        |
| appium:appTopLevelWindow           | The hexadecimal handle of an existing application top level window to attach to, for example `0x12345` (should be of string type). Either this capability, `appTopLevelWindowTitleMatch` or `app` must be provided on session startup.                                                                                            | `0xC0B46`                                                                          |
| appium:appTopLevelWindowTitleMatch | The title of an existing application top level window to attach to, for example `My App Window Title` (should be of string type). Either this capability, `appTopLevelWindow` or `app` must be provided on session startup.                                                                                                       | `My App Window Title` or `My App Window Title - .*`                                |
| appium:newCommandTimeout           | The number of seconds the to wait for clients to send commands before deciding that the client has gone away and the session should shut down. Default one minute (60).                                                                                                                                                           | `120`                                                                              |

## Getting Started

This driver currently can be downloaded as an executable. Start the web driver service with:

```PowerShell
./FlaUI.WebDriver.exe --urls=http://localhost:4723/
```

After it has started, it can be used via WebDriver clients such as for example:

- [Appium.WebDriver](https://www.nuget.org/packages/Appium.WebDriver)
- [Selenium.WebDriver](https://www.nuget.org/packages/Selenium.WebDriver)
- [WebdriverIO](https://www.npmjs.com/package/webdriverio)

Using the [Appium.WebDriver](https://www.nuget.org/packages/Appium.WebDriver) C# client:

```C#
using OpenQA.Selenium.Appium.Windows;

public class FlaUIDriverOptions : AppiumOptions
{
    public static FlaUIDriverOptions ForApp(string path)
    {
        return new FlaUIDriverOptions()
        {
            PlatformName = "windows",
            AutomationName = "flaui",
            App = path
        };
    }
}

var driver = new WindowsDriver(new Uri("http://localhost:4723"), FlaUIDriverOptions.ForApp("C:\\YourApp.exe"))
```

Using the [Selenium.WebDriver](https://www.nuget.org/packages/Selenium.WebDriver) C# client:

```C#
using OpenQA.Selenium;

public class FlaUIDriverOptions : DriverOptions
{
    public static FlaUIDriverOptions ForApp(string path)
    {
        var options = new FlaUIDriverOptions()
        {
            PlatformName = "windows"
        };
        options.AddAdditionalOption("appium:automationName", "flaui");
        options.AddAdditionalOption("appium:app", path);
        return options;
    }

    public override ICapabilities ToCapabilities()
    {
        return GenerateDesiredCapabilities(true);
    }
}

var driver = new RemoteWebDriver(new Uri("http://localhost:4723"), FlaUIDriverOptions.ForApp("C:\\YourApp.exe"))
```

Using the [WebdriverIO](https://www.npmjs.com/package/webdriverio) JavaScript client:

```JavaScript
import { remote } from 'webdriverio'

const driver = await remote({
    capabilities: {
        platformName: 'windows',
        'appium:automationName': 'flaui'
        'appium:app': 'C:\\YourApp.exe'
    }
});
```

## Selectors

On Windows, the recommended selectors, in order of reliability are:

| Selector                   | Locator strategy keyword | Supported?                                                                           |
| -------------------------- | ------------------------ | ------------------------------------------------------------------------------------ |
| Automation ID              | `"accessibility id"`     | :white_check_mark:                                                                   |
| Name                       | `"name"`                 | :white_check_mark:                                                                   |
| Class name                 | `"class name"`           | :white_check_mark:                                                                   |
| Link text selector         | `"link text"`            | :white_check_mark:                                                                   |
| Partial link text selector | `"partial link text"`    | :white_check_mark:                                                                   |
| Tag name                   | `"tag name"`             | :white_check_mark:                                                                   |
| XPath selector             | `"xpath"`                | :white_check_mark:                                                                   |
| CSS selector               | `"css selector"`         | Only ID, class or `name` attribute selectors. IDs are interpreted as automation IDs. |

Using the Selenium C# client, the selectors are:

```C#
driver.FindElement(By.Id("TextBox")).Click(); // Matches by automation ID
driver.FindElement(By.Name("TextBox")).Click();
driver.FindElement(By.ClassName("TextBox")).Click();
driver.FindElement(By.LinkText("Button")).Click();
driver.FindElement(By.PartialLinkText("Button")).Click();
driver.FindElement(By.TagName("RadioButton")).Click();
driver.FindElement(By.XPath("//RadioButton")).Click();
```

Using the WebdriverIO JavaScript client (see [WebdriverIO Selectors guide](https://webdriver.io/docs/selectors):

```JavaScript
await driver.$('~automationId').click();
await driver.$('[name="Name"]').click();
await driver.$('.TextBox').click();
await driver.$('=Button').click();
await driver.$('*=Button').click();
await driver.$('<RadioButton />').click();
await driver.$('//RadioButton').click();
```

## Windows

The driver supports switching windows. The behavior of windows is as following (identical to behavior of e.g. the Chrome driver):

- By default, the window is the window that the application was started with.
- The window does not change if the app/user opens another window, also not if that window happens to be on the foreground.
- All open window handles from the same app process (same process ID in Windows) can be retrieved.
- Other processes spawned by the app that open windows are not visible as window handles.
  Those can be automated by starting a new driver session with e.g. the `appium:appTopLevelWindow` capability.
- Closing a window does not automatically switch the window handle.
  That means that after closing a window, most commands will return an error "no such window" until the window is switched.
- Switching to a window will set that window in the foreground.

## Running scripts

The driver supports PowerShell commands.

Using the Selenium or Appium WebDriver C# client:

```C#
var result = driver.ExecuteScript("powerShell", new Dictionary<string,string> { ["command"] = "1+1" });
```

Using the WebdriverIO JavaScript client:

```JavaScript
const result = driver.executeScript("powerShell", [{ command: `1+1` }]);
```

## Windows extensions

To enable easy switching from appium-windows-driver, there is a rudimentary implementation of `windows: click`, `windows: hover`, `windows: scroll` and `windows: keys`.

## Supported WebDriver Commands

| Method | URI Template                                                   | Command                        | Implemented                        |
| ------ | -------------------------------------------------------------- | ------------------------------ | ---------------------------------- |
| POST   | /session                                                       | New Session                    | :white_check_mark:                 |
| DELETE | /session/{session id}                                          | Delete Session                 | :white_check_mark:                 |
| GET    | /status                                                        | Status                         | :white_check_mark:                 |
| GET    | /session/{session id}/timeouts                                 | Get Timeouts                   | :white_check_mark:                 |
| POST   | /session/{session id}/timeouts                                 | Set Timeouts                   | :white_check_mark:                 |
| POST   | /session/{session id}/url                                      | Navigate To                    | N/A                                |
| GET    | /session/{session id}/url                                      | Get Current URL                | N/A                                |
| POST   | /session/{session id}/back                                     | Back                           | N/A                                |
| POST   | /session/{session id}/forward                                  | Forward                        | N/A                                |
| POST   | /session/{session id}/refresh                                  | Refresh                        | N/A                                |
| GET    | /session/{session id}/title                                    | Get Title                      | :white_check_mark:                 |
| GET    | /session/{session id}/window                                   | Get Window Handle              | :white_check_mark:                 |
| DELETE | /session/{session id}/window                                   | Close Window                   | :white_check_mark:                 |
| POST   | /session/{session id}/window                                   | Switch To Window               | :white_check_mark:                 |
| GET    | /session/{session id}/window/handles                           | Get Window Handles             | :white_check_mark:                 |
| POST   | /session/{session id}/window/new                               | New Window                     |                                    |
| POST   | /session/{session id}/frame                                    | Switch To Frame                | N/A                                |
| POST   | /session/{session id}/frame/parent                             | Switch To Parent Frame         | N/A                                |
| GET    | /session/{session id}/window/rect                              | Get Window Rect                | :white_check_mark:                 |
| POST   | /session/{session id}/window/rect                              | Set Window Rect                | :white_check_mark:                 |
| POST   | /session/{session id}/window/maximize                          | Maximize Window                |                                    |
| POST   | /session/{session id}/window/minimize                          | Minimize Window                |                                    |
| POST   | /session/{session id}/window/fullscreen                        | Fullscreen Window              |                                    |
| GET    | /session/{session id}/element/active                           | Get Active Element             | :white_check_mark:                 |
| GET    | /session/{session id}/element/{element id}/shadow              | Get Element Shadow Root        | N/A                                |
| POST   | /session/{session id}/element                                  | Find Element                   | :white_check_mark:                 |
| POST   | /session/{session id}/elements                                 | Find Elements                  | :white_check_mark:                 |
| POST   | /session/{session id}/element/{element id}/element             | Find Element From Element      | :white_check_mark:                 |
| POST   | /session/{session id}/element/{element id}/elements            | Find Elements From Element     | :white_check_mark:                 |
| POST   | /session/{session id}/shadow/{shadow id}/element               | Find Element From Shadow Root  | N/A                                |
| POST   | /session/{session id}/shadow/{shadow id}/elements              | Find Elements From Shadow Root | N/A                                |
| GET    | /session/{session id}/element/{element id}/selected            | Is Element Selected            | :white_check_mark:                 |
| GET    | /session/{session id}/element/{element id}/displayed           | Is Element Displayed           | :white_check_mark: [^isdisplayed]  |
| GET    | /session/{session id}/element/{element id}/attribute/{name}    | Get Element Attribute          | :white_check_mark: [^getattribute] |
| GET    | /session/{session id}/element/{element id}/property/{name}     | Get Element Property           | :white_check_mark:                 |
| GET    | /session/{session id}/element/{element id}/css/{property name} | Get Element CSS Value          | N/A                                |
| GET    | /session/{session id}/element/{element id}/text                | Get Element Text               | :white_check_mark:                 |
| GET    | /session/{session id}/element/{element id}/name                | Get Element Tag Name           | :white_check_mark:                 |
| GET    | /session/{session id}/element/{element id}/rect                | Get Element Rect               | :white_check_mark:                 |
| GET    | /session/{session id}/element/{element id}/enabled             | Is Element Enabled             | :white_check_mark:                 |
| GET    | /session/{session id}/element/{element id}/computedrole        | Get Computed Role              |                                    |
| GET    | /session/{session id}/element/{element id}/computedlabel       | Get Computed Label             |                                    |
| POST   | /session/{session id}/element/{element id}/click               | Element Click                  | :white_check_mark:                 |
| POST   | /session/{session id}/element/{element id}/clear               | Element Clear                  | :white_check_mark:                 |
| POST   | /session/{session id}/element/{element id}/value               | Element Send Keys              | :white_check_mark:                 |
| GET    | /session/{session id}/source                                   | Get Page Source                | N/A                                |
| POST   | /session/{session id}/execute/sync                             | Execute Script                 | :white_check_mark:                 |
| POST   | /session/{session id}/execute/async                            | Execute Async Script           |                                    |
| GET    | /session/{session id}/cookie                                   | Get All Cookies                | N/A                                |
| GET    | /session/{session id}/cookie/{name}                            | Get Named Cookie               | N/A                                |
| POST   | /session/{session id}/cookie                                   | Add Cookie                     | N/A                                |
| DELETE | /session/{session id}/cookie/{name}                            | Delete Cookie                  | N/A                                |
| DELETE | /session/{session id}/cookie                                   | Delete All Cookies             | N/A                                |
| POST   | /session/{session id}/actions                                  | Perform Actions                | :white_check_mark:                 |
| DELETE | /session/{session id}/actions                                  | Release Actions                | :white_check_mark:                 |
| POST   | /session/{session id}/alert/dismiss                            | Dismiss Alert                  |                                    |
| POST   | /session/{session id}/alert/accept                             | Accept Alert                   |                                    |
| GET    | /session/{session id}/alert/text                               | Get Alert Text                 |                                    |
| POST   | /session/{session id}/alert/text                               | Send Alert Text                |                                    |
| GET    | /session/{session id}/screenshot                               | Take Screenshot                | :white_check_mark:                 |
| GET    | /session/{session id}/element/{element id}/screenshot          | Take Element Screenshot        | :white_check_mark:                 |
| POST   | /session/{session id}/print                                    | Print Page                     |                                    |

[^getattribute]: In Selenium WebDriver, use `GetDomAttribute` because `GetAttribute` converts to javascript.
[^isdisplayed]: In Selenium WebDriver, the `Displayed` property converts to javascript. Use [Appium WebDriver](https://github.com/appium/dotnet-client) to use this functionality. It uses the [IsOffscreen](https://learn.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.automationelementinformation.isoffscreen) property that however does not seem to take it into account if the element is blocked by another window.

### WebDriver Interpretation

There is an interpretation to use the WebDriver specification to drive native automation. Appium does not seem to describe that interpretation and leaves it up to the implementer as well. Therefore we describe it here:

| WebDriver term                     | Interpretation                                                                                                                         |
| ---------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| browser                            | The Windows OS on which the FlaUI.WebDriver instance is running                                                                        |
| top-level browsing contexts        | Any window of the app under test (modal windows too)                                                                                   |
| current top-level browsing context | The current selected window of the app under test                                                                                      |
| browsing contexts                  | Any window of the app under test (equal to "top-level browsing contexts")                                                              |
| current browsing context           | The current selected window of the app under test (equal to "current top-level browsing context")                                      |
| window                             | Any window of the app under test (modal windows too)                                                                                   |
| frame                              | Not implemented - frames are only relevant for web browsers                                                                            |
| shadow root                        | Not implemented - shadow DOM is only relevant for web browsers                                                                         |
| cookie                             | Not implemented - cookies are only relevant for web browsers                                                                           |
| tag name                           | Control type in Windows                                                                                                                |
| attribute                          | [UI automation element property](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-automation-element-propids) in Windows |

### Deviations from W3C WebDriver Spec

<https://www.w3.org/TR/webdriver2/#element-send-keys> says:

> Set the text insertion caret using set selection range using current text length for both the start and end parameters.

This is impossible using UIA, as there is no API to set the caret position: text instead gets inserted at the beginning of a text box. This is also WinAppDriver's behavior.

### Element Attributes

Attributes are mapped to UI automation element properties. Attributes without a period (`.`) are mapped to [Automation Element Properties](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-automation-element-propids). For example to read the `UIA_ClassNamePropertyId` using Selenium or Appium WebDriver:

```C#
var element = driver.FindElement(By.Id("TextBox"));
var value = element.GetDomAttribute("ClassName");
```

Attributes with a period are treated as [Control Pattern Properties](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-control-pattern-propids) with the form `Pattern.Property`. For example to read the `UIA_ToggleToggleStatePropertyId` using Selenium WebDriver:

```C#
var element = driver.FindElement(By.Id("ToggleButton"));
var value = element.GetDomAttribute("Toggle.ToggleState");
```

## Next Steps

Possible next steps for this project:

- Distribute as [Appium driver](http://appium.io/docs/en/2.1/ecosystem/build-drivers/)
