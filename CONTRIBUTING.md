# How to contribute

## Building

Use `dotnet build` to build.

## Testing

Use `dotnet test` to run tests. At the moment the tests are end-to-end UI tests that use [Appium.WebDriver](https://github.com/appium/dotnet-client) to operate a test application, running FlaUI.WebDriver.exe in the background, so they should be run on Windows.

Add UI tests for every feature added and every bug fixed, and feel free to improve existing test coverage.

## Submitting changes

Please send a [GitHub Pull Request](https://github.com/FlaUI/FlaUI.WebDriver/pulls) with a clear list of what you've done (read more about [pull requests](http://help.github.com/pull-requests/)). Please follow our coding conventions (below) and make sure all of your commits are atomic (one feature per commit).

Always write a clear log message for your commits. One-line messages are fine for small changes, but bigger changes should look like this:

    $ git commit -m "A brief summary of the commit
    > 
    > A paragraph describing what changed and its impact."

## Coding conventions

Follow the [.NET Runtime Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md).

## Releasing

To release, simply create a tag and push it, which will trigger the release automatically:

    git tag -a v0.1.0-alpha -m "Your tag message"
    git push origin v0.1.0-alpha
