# Getting Started

## Installation

Consume this library via its NuGet Package.
Click on the badge to find its latest version and the instructions for consuming it that best apply to your project.

[![NuGet package](https://img.shields.io/nuget/v/xunit.stafact.svg)](https://www.nuget.org/packages/Xunit.StaFact)

## Default Fact behavior

[!code-csharp[](../../samples/SampleTests.cs#Fact)]

## Usage

### Portable UI

Best when you need basic UI thread semantics for tests that may run on any OS.
You'll get an STA thread on Windows.

[!code-csharp[](../../samples/SampleTests.cs#UIFact)]

### WPF

More closely resembles WPF-specific semantics including a WPF-specific @System.Threading.SynchronizationContext.

[!code-csharp[](../../samples/SampleTests.cs#WpfFact)]

### WinForms

More closely resembles WinForms-specific semantics including a WinForms-specific @System.Threading.SynchronizationContext.

[!code-csharp[](../../samples/SampleTests.cs#WinFormsFact)]

### WinUI

Use @Xunit.WinUIFactAttribute or @Xunit.WinUITheoryAttribute to initialize WinUI XAML and run a test with a
@Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext. The test project must use a Windows-versioned
target framework such as `net8.0-windows10.0.17763.0`.

```csharp
[WinUIFact]
public async Task ControlStaysOnDispatcherQueue()
{
    var button = new Microsoft.UI.Xaml.Controls.Button();
    Assert.True(button.DispatcherQueue.HasThreadAccess);

    await Task.Yield();

    Assert.True(button.DispatcherQueue.HasThreadAccess);
}
```

### STA thread

Guarantees the test to run on an STA thread.
Applicable only on Windows.
Because no @System.Threading.SynchronizationContext is applied by default, an async test will resume on a threadpool thread instead of the test thread after a yielding await.

[!code-csharp[](../../samples/SampleTests.cs#STAFact)]

## Sharing a UI thread between tests

By default, each UI fact or theory gets a fresh thread. When thread-affine state must outlive one test,
register a shared-thread fixture and receive it in the test class constructor:

[!code-csharp[](../../samples/SharedThreadSamples.cs#ClassSharedThread)]

See [Shared UI thread fixtures](features.md#shared-ui-thread-fixtures) for collection scope,
fixture lifecycle hooks, supported fixture types, and concurrency behavior.
