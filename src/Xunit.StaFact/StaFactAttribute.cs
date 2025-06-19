// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Identifies an xunit test that starts on an STA thread.
/// Tests will be Skipped on non-Windows operating systems.
/// </summary>
/// <remarks>
/// The test does *not* apply a <see cref="SynchronizationContext" />, so an async test
/// will resume on a standard MTA thread from the thread pool.
/// To get an STA thread even after awaiting expressions, use <see cref="UIFactAttribute" />.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(StaFactDiscoverer))]
public class StaFactAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StaFactAttribute"/> class.
    /// </summary>
    public StaFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
    }
}
