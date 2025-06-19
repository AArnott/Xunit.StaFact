// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Identifies an xunit test that starts on with a <see cref="System.Threading.SynchronizationContext"/>
/// running on <see cref="Foundation.NSRunLoop.Main"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(CocoaFactDiscoverer))]
public class CocoaFactAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CocoaFactAttribute"/> class.
    /// </summary>
    public CocoaFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
    }
}
