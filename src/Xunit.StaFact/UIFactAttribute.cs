// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit
{
    using System;
    using System.Threading;
    using Xunit.Sdk;

    /// <summary>
    /// Identifies an xunit test that starts on a UI thread-like <see cref="SynchronizationContext" />
    /// such that awaited expressions resume on the test's "main thread".
    /// On Windows, the test runs on an STA thread.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.UIFactDiscoverer", ThisAssembly.AssemblyName)]
    public class UIFactAttribute : FactAttribute
    {
    }
}
