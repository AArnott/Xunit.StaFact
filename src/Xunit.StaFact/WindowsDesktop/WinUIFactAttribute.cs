// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

using System.Runtime.CompilerServices;
using Microsoft.UI.Dispatching;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Identifies an xunit test that starts on an STA thread
/// with a WinUI <see cref="DispatcherQueueSynchronizationContext"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(WinUIFactDiscoverer))]
public class WinUIFactAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WinUIFactAttribute"/> class.
    /// </summary>
    public WinUIFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
    }
}

#endif
