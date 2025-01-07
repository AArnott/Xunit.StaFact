// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Identifies an xunit theory that starts on a UI thread-like <see cref="SynchronizationContext" />
/// such that awaited expressions resume on the test's "main thread".
/// On Windows, the test runs on an STA thread.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(UITheoryDiscoverer))]
public class UITheoryAttribute : TheoryAttribute
{
}
