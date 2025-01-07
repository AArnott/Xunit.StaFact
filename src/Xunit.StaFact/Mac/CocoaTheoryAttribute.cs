// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Identifies an xunit theory that starts on with a <see cref="System.Threading.SynchronizationContext"/>
/// running on <see cref="Foundation.NSRunLoop.Main"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(CocoaTheoryDiscoverer))]
public class CocoaTheoryAttribute : TheoryAttribute
{
}
