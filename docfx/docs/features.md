# Features

The following test attributes are supported:

Xunit test attributes            | Supported OS's   | SynchronizationContext               | STA thread?     |
|--------------------------------|------------------|--------------------------------------| --------------- |
@Xunit.UIFactAttribute, @Xunit.UITheoryAttribute | All              | Yes[^1]                              | yes[^2]         |
@Xunit.WpfFactAttribute, @Xunit.WpfTheoryAttribute           | Windows only[^3] | @System.Windows.Threading.DispatcherSynchronizationContext   | yes             |
@Xunit.WinFormsFactAttribute, @Xunit.WinFormsTheoryAttribute | Windows only[^3] | @System.Windows.Forms.WindowsFormsSynchronizationContext | yes             |
@Xunit.WinUIFactAttribute, @Xunit.WinUITheoryAttribute       | Windows 10 1809+[^4] | @Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext | yes       |
@Xunit.StaFactAttribute, @Xunit.StaTheoryAttribute           | Windows only[^3] | No                                   | yes             |
@Xunit.CocoaFactAttribute, @Xunit.CocoaTheoryAttribute       | Mac OSX only[^3] | Yes[^1]                              | no              |

We also offer a @Xunit.UISettingsAttribute that can be applied to individual test methods or test classes to control the behavior of the various UI test attributes.
This attribute offers a means to add automated retries to a test's execution for unstable tests.

## Shared UI thread fixtures

By default, every fact or theory in this package runs on a newly created thread that is disposed when that test finishes.
This avoids sharing thread-affine state and allows unrelated tests to run concurrently.

Some UI frameworks retain thread-affine objects in static caches, or a suite may intentionally host an application-level
object for several tests. In those cases, opt in to a shared thread with an xUnit class or collection fixture.
The fixture owns the thread, and its xUnit lifetime determines how long the thread is shared.

Test attribute | Compatible fixture
|---|---|
@Xunit.UIFactAttribute, @Xunit.UITheoryAttribute | @Xunit.UIThreadFixture
@Xunit.StaFactAttribute, @Xunit.StaTheoryAttribute | @Xunit.StaThreadFixture
@Xunit.WpfFactAttribute, @Xunit.WpfTheoryAttribute | @Xunit.WpfThreadFixture
@Xunit.WinFormsFactAttribute, @Xunit.WinFormsTheoryAttribute | @Xunit.WinFormsThreadFixture
@Xunit.WinUIFactAttribute, @Xunit.WinUITheoryAttribute | @Xunit.WinUIThreadFixture
@Xunit.CocoaFactAttribute, @Xunit.CocoaTheoryAttribute | @Xunit.CocoaThreadFixture

### Class scope

Implement `IClassFixture<TFixture>` and receive that fixture in the test class constructor.
All compatible UI facts and theories in the class then execute on the fixture's thread.

[!code-csharp[](../../samples/SharedThreadSamples.cs#ClassSharedThread)]

The fixture must be present in the constructor arguments so the test runner can identify it.
Merely declaring `IClassFixture<TFixture>` without receiving the fixture does not opt the tests into its thread.

### Collection scope

Use an `ICollectionFixture<TFixture>` when multiple test classes must share the same thread.
Each participating class must belong to that collection and receive the collection fixture in its constructor.

[!code-csharp[](../../samples/SharedThreadSamples.cs#CollectionSharedThread)]

xUnit does not run tests in one collection concurrently, so tests borrowing one collection fixture do not overlap.
Classes using different fixture instances remain eligible for parallel execution. The xUnit execution throttle remains
in effect while a test is running on a fixture thread.

### Fixture initialization and cleanup

xUnit constructs fixtures on its own worker thread, before `Xunit.StaFact` participates in test execution.
Consequently, a fixture's CLR constructor is **not** guaranteed to run on an STA or UI thread.

The simplest usage requires no custom fixture type: register and inject one of the library-provided fixtures directly,
as shown in the class and collection examples above.

For thread-affine setup and cleanup, derive from the compatible fixture type and override
`InitializeOnUIThreadAsync` and `DisposeOnUIThreadAsync`. These hooks are dispatched to the owned thread,
and cleanup runs before that thread is shut down.

[!code-csharp[](../../samples/SharedThreadSamples.cs#SharedThreadFixture)]

The portable UI, WPF, WinForms, WinUI, and Cocoa fixtures install their corresponding synchronization context.
The STA fixture intentionally does not install one, matching @Xunit.StaFactAttribute behavior; after a yielding
`await`, code may resume on a thread-pool thread.

### Compatibility and ownership rules

- A test class may receive at most one @Xunit.UIThreadFixtureBase-derived fixture.
- The fixture must match the fact or theory attribute. For example, a @Xunit.WpfFactAttribute cannot borrow an
  @Xunit.UIThreadFixture.
- Tests that do not receive a shared-thread fixture retain the default fresh-thread-per-test behavior.
- Do not dispose a fixture directly when xUnit owns it. xUnit disposes it at the end of its class or collection lifetime.
- Put only state that is intentionally shared into the fixture. Test class instances are still created separately for each test.


[^1]: This is a private @System.Threading.SynchronizationContext that works cross-platform and effectively keeps code running on the test's starting thread the way a GUI application's main thread would do.

[^2]: STA thread only applies on Windows. On other operating systems, an MTA thread is used.

[^3]: Windows-only attributes result in the test to result in "Skipped" on other operating systems.

[^4]: WinUI attributes require a Windows-versioned target framework such as `net8.0-windows10.0.17763.0`.
