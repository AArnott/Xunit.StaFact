// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Identifies an xunit test that starts on a UI thread-like <see cref="SynchronizationContext" />
/// such that awaited expressions resume on the test's "main thread".
/// On Windows, the test runs on an STA thread.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(UIFactDiscoverer))]
public class UIFactAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UIFactAttribute"/> class.
    /// </summary>
    public UIFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
    }
}
