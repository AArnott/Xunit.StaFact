# Xunit.StaFact

[![Build Status](https://dev.azure.com/andrewarnott/OSS/_apis/build/status/Xunit.StaFact)](https://dev.azure.com/andrewarnott/OSS/_build/latest?definitionId=22)
[![NuGet package](https://img.shields.io/nuget/v/xunit.stafact.svg)][NuPkg]

Run your xunit-based tests on an STA thread with the WPF Dispatcher, a WinForms SynchronizationContext, or even a cross-platform generic UI thread emulation with a SynchronizationContext that keeps code running on a "main thread" for that test.

Simply use `[WpfFact]`, `[WinFormsFact]`, `[StaFact]` or the cross-platform `[UIFact]` on your test method to run your test under conditions that most closely match the main thread in your application.

Theory variants of these attributes allow for parameterized testing. Check out the xunit.combinatorial nuget package for pairwise or combinatorial testing with theories.

## Features

The following attributes are supported:

Xunit test attributes            | Supported OS's   | SynchronizationContext               | STA thread?     |
|--------------------------------|------------------|--------------------------------------| --------------- |
`[UIFact, UITheory]`             | All              | Yes¹                                 | yes²            |
`[WpfFact, WpfTheory]`           | Windows only³    | `DispatcherSynchronizationContext`   | yes             |
`[WinFormsFact, WinFormsTheory]` | Windows only³    | `WindowsFormsSynchronizationContext` | yes             |
`[StaFact, StaTheory]`           | Windows only³    | No                                   | yes             |

¹ This is a private `SynchronizationContext` that works cross-platform and effectively keeps code running on the test's starting thread the way a GUI application's main thread would do.

² STA thread only applies on Windows. On other operating systems, an MTA thread is used.

³ Windows-only attributes result in the test to result in "Skipped" on other operating systems.

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

See more [samples](src/Xunit.StaFact.Tests/Samples.cs).

[NuPkg]: https://www.nuget.org/packages/Xunit.StaFact
