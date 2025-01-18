# Getting Started

## Installation

Consume this library via its NuGet Package.
Click on the badge to find its latest version and the instructions for consuming it that best apply to your project.

[![NuGet package](https://img.shields.io/nuget/v/xunit.stafact.svg)](https://www.nuget.org/packages/Xunit.StaFact)

## Default Fact behavior

[!code-csharp[](../../samples/Samples.cs#Fact)]

## Usage

### Portable UI

Best when you need basic UI thread semantics for tests that may run on any OS.
You'll get an STA thread on Windows.

[!code-csharp[](../../samples/Samples.cs#UIFact)]

### WPF

More closely resembles WPF-specific semantics including a WPF-specific @System.Threading.SynchronizationContext.

[!code-csharp[](../../samples/Samples.cs#WpfFact)]

### WinForms

More closely resembles WinForms-specific semantics including a WinForms-specific @System.Threading.SynchronizationContext.

[!code-csharp[](../../samples/Samples.cs#WinFormsFact)]

### STA thread

Guarantees the test to run on an STA thread.
Applicable only on Windows.
Because no @System.Threading.SynchronizationContext is applied by default, an async test will resume on a threadpool thread instead of the test thread after a yielding await.

[!code-csharp[](../../samples/Samples.cs#STAFact)]
