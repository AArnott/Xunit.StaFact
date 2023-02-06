# Contributing

This project has adopted the [Microsoft Open Source Code of
Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct
FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com)
with any additional questions or comments.

## Best practices

* Use Windows PowerShell or [PowerShell Core][pwsh] (including on Linux/OSX) to run .ps1 scripts.
  Some scripts set environment variables to help you, but they are only retained if you use PowerShell as your shell.

## Prerequisites

The only prerequisite for building, testing, and deploying from this repository
is the [.NET SDK](https://get.dot.net/).
You should install the version specified in `global.json` or a later version within
the same major.minor.Bxx "hundreds" band.
For example if 2.2.300 is specified, you may install 2.2.300, 2.2.301, or 2.2.310
while the 2.2.400 version would not be considered compatible by .NET SDK.
See [.NET Core Versioning](https://docs.microsoft.com/en-us/dotnet/core/versions/) for more information.

All dependencies can be installed by running the `init.ps1` script at the root of the repository
using Windows PowerShell or [PowerShell Core][pwsh] (on any OS).

This repository can be built on Windows, Linux, and OSX.

## Package restore

The easiest way to restore packages may be to run `init.ps1` which automatically authenticates
to the feeds that packages for this repo come from, if any.
`dotnet restore` or `nuget restore` also work but may require extra steps to authenticate to any applicable feeds.

MacOS specific code that was added in #68 requires that `init.ps1` be run in the repo within a terminal *and devenv.exe must be spawned from that same window* in order for Visual Studio for Windows to be able to find the required macos workload.
Note that `init.ps1 -InstallLocality machine` in broken due to dotnet workloads problems such as https://github.com/dotnet/sdk/issues/30230.

## Building

Building, testing, and packing this repository can be done by using the standard dotnet CLI commands (e.g. `dotnet build`, `dotnet test`, `dotnet pack`, etc.).

[pwsh]: https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-6
