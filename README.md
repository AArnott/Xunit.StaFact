# Xunit.StaFact

[![Build Status](https://dev.azure.com/andrewarnott/OSS/_apis/build/status/Xunit.StaFact)](https://dev.azure.com/andrewarnott/OSS/_build/latest?definitionId=22)
[![NuGet package](https://img.shields.io/nuget/v/xunit.stafact.svg)][NuPkg]

Run your xunit-based tests on an STA thread with the WPF Dispatcher, a WinForms SynchronizationContext, or even a cross-platform generic UI thread emulation with a SynchronizationContext that keeps code running on a "main thread" for that test.

Simply use `[WpfFact]`, `[WinFormsFact]`, `[StaFact]` or the cross-platform `[UIFact]` on your test method to run your test under conditions that most closely match the main thread in your application.

Theory variants of these attributes allow for parameterized testing. Check out the xunit.combinatorial nuget package for pairwise or combinatorial testing with theories.

## xUnit v2 vs. v3

With xUnit v3 now available with breaking changes from v2, see the following table for how we support both versions:

xUnit | Xunit.STAFact
--|--
For xUnit v2 support | Use Xunit.STAFact 1.x
For xUnit.v3 1.x support | Use Xunit.STAFact 2.0
For xUnit.v3 2.x support | Use Xunit.STAFact 2.1+
For xUnit.v3 3.x support | Use Xunit.STAFact 3.x

## Features

The following test attributes are supported:

Xunit test attributes            | Supported OS's   | SynchronizationContext               | STA thread?     |
|--------------------------------|------------------|--------------------------------------| --------------- |
`[UIFact, UITheory]`             | All              | Yes<sup>1</sup>                                 | yes<sup>2</sup>            |
`[WpfFact, WpfTheory]`           | Windows only<sup>3</sup>    | `DispatcherSynchronizationContext`   | yes             |
`[WinFormsFact, WinFormsTheory]` | Windows only<sup>3</sup>    | `WindowsFormsSynchronizationContext` | yes             |
`[StaFact, StaTheory]`           | Windows only<sup>3</sup>    | No                                   | yes             |
`[CocoaFact, CocoaTheory]`       | Mac OSX only<sup>3</sup>    | Yes<sup>1</sup>                                 | no              |

<sup>1</sup> This is a private `SynchronizationContext` that works cross-platform and effectively keeps code running on the test's starting thread the way a GUI application's main thread would do.

<sup>2</sup> STA thread only applies on Windows. On other operating systems, an MTA thread is used.

<sup>3</sup> Windows-only attributes result in the test to result in "Skipped" on other operating systems.

We also offer a `[UISettingsAttribute]` that can be applied to individual test methods or test classes to control the behavior of the various UI test attributes.
This attribute offers a means to add automated retries to a test's execution for unstable tests.

## Samples

```csharp
[UIFact] // or [WinFormsFact] or [WpfFact]
public async Task WpfFact_OnSTAThread()
{
    Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
    int originalThread = Environment.CurrentManagedThreadId;
    await Task.Yield();
    Assert.Equal(originalThread, Environment.CurrentManagedThreadId); // still there
}
```

See more in [our getting started doc](http://aarnott.github.io/Xunit.StaFact/docs/getting-started.html).

## Sponsorships

[GitHub Sponsors](https://github.com/sponsors/AArnott)
[Zcash](zcash:u1vv2ws6xhs72faugmlrasyeq298l05rrj6wfw8hr3r29y3czev5qt4ugp7kylz6suu04363ze92dfg8ftxf3237js0x9p5r82fgy47xkjnw75tqaevhfh0rnua72hurt22v3w3f7h8yt6mxaa0wpeeh9jcm359ww3rl6fj5ylqqv54uuwrs8q4gys9r3cxdm3yslsh3rt6p7wznzhky7)

[NuPkg]: https://www.nuget.org/packages/Xunit.StaFact
