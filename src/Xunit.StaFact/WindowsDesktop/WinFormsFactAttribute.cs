// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Identifies an xunit test that starts on an STA thread
/// with a <see cref="WindowsFormsSynchronizationContext" />.
/// Tests will be Skipped on non-Windows operating systems.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(WinFormsFactDiscoverer))]
public class WinFormsFactAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WinFormsFactAttribute"/> class.
    /// </summary>
    public WinFormsFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
    }
}
