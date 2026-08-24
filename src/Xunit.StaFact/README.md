Run your xunit-based tests on an STA thread with the WPF Dispatcher, a WinForms or WinUI SynchronizationContext, or even a cross-platform generic UI thread emulation with a SynchronizationContext that keeps code running on a "main thread" for that test.

Simply use `[WpfFact]`, `[WinFormsFact]`, `[WinUIFact]`, `[StaFact]` or the cross-platform `[UIFact]` on your test method to run your test under conditions that most closely match the main thread in your application.

WinUI attributes require a Windows-versioned target framework such as `net8.0-windows10.0.17763.0`.

Theory variants of these attributes allow for parameterized testing. Check out the xunit.combinatorial NuGet package for pairwise or combinatorial testing with theories.

Use v1.x versions of this package for use with xUnit v2.
Use v2.0 versions of this package for use with xUnit.v3 v1.x.
Use v2.1+ versions of this package for use with xUnit.v3 v2+.
Use v3.0+ versions of this package for use with xunit.v3 v3+
Use v4.0+ versions of this package for use with xunit.v3 v4+

[See our full product documentation](https://aarnott.github.io/Xunit.StaFact/).
