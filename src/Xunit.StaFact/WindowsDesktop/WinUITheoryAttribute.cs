// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER

namespace Xunit;

/// <summary>
/// Identifies an xunit theory that starts on an STA thread
/// with a WinUI DispatcherQueueSynchronizationContext.
/// Tests will be Skipped on operating systems other than Windows 10 or later.
/// </summary>
/// <remarks>
/// Use of this attribute requires that the App.OnLaunched method in the test project be modified
/// to set the DispatcherQueue static property on the <see cref="WinUIFactAttribute"/> class.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("Xunit.Sdk.WinUITheoryDiscoverer", ThisAssembly.AssemblyName)]
public class WinUITheoryAttribute : TheoryAttribute
{
}

#endif
