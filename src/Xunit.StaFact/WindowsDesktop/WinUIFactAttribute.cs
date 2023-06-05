// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER

namespace Xunit;

/// <summary>
/// Identifies an xunit test that starts on an STA thread
/// with a WinUI DispatcherQueueSynchronizationContext.
/// Tests will be Skipped on operating systems other than Windows 10 or later.
/// </summary>
/// <remarks>
/// Use of this attribute requires that the App.OnLaunched method in the test project be modified
/// to set the DispatcherQueue static property on this class.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("Xunit.Sdk.WinUIFactDiscoverer", ThisAssembly.AssemblyName)]
public class WinUIFactAttribute : FactAttribute
{
#if WINDOWS10_0_17763_0_OR_GREATER
    /// <summary>
    /// Gets or sets a single <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> instance from the test project's main window
    /// that will be used for all tests that use the <see cref="WinUIFactAttribute"/> or <see cref="WinUITheoryAttribute"/>.
    /// </summary>
    public static Microsoft.UI.Dispatching.DispatcherQueue? DispatcherQueue { get; set; }
#endif
}

#endif
