# Features

The following test attributes are supported:

Xunit test attributes            | Supported OS's   | SynchronizationContext               | STA thread?     |
|--------------------------------|------------------|--------------------------------------| --------------- |
@Xunit.UIFactAttribute, @Xunit.UITheoryAttribute | All              | Yes[^1]                              | yes[^2]         |
@Xunit.WpfFactAttribute, @Xunit.WpfTheoryAttribute           | Windows only[^3] | @System.Windows.Threading.DispatcherSynchronizationContext   | yes             |
@Xunit.WinFormsFactAttribute, @Xunit.WinFormsTheoryAttribute | Windows only[^3] | @System.Windows.Forms.WindowsFormsSynchronizationContext | yes             |
@Xunit.StaFactAttribute, @Xunit.StaTheoryAttribute           | Windows only[^3] | No                                   | yes             |
@Xunit.CocoaFactAttribute, @Xunit.CocoaTheoryAttribute       | Mac OSX only[^3] | Yes[^1]                              | no              |

We also offer a @Xunit.UISettingsAttribute that can be applied to individual test methods or test classes to control the behavior of the various UI test attributes.
This attribute offers a means to add automated retries to a test's execution for unstable tests.


[^1]: This is a private @System.Threading.SynchronizationContext that works cross-platform and effectively keeps code running on the test's starting thread the way a GUI application's main thread would do.

[^2]: STA thread only applies on Windows. On other operating systems, an MTA thread is used.

[^3]: Windows-only attributes result in the test to result in "Skipped" on other operating systems.
