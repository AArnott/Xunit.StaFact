// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Identifies an xunit theory that starts on an STA thread.
    /// Tests will be Skipped on non-Windows operating systems.
    /// </summary>
    /// <remarks>
    /// The test does *not* apply a <see cref="SynchronizationContext" />, so an async test
    /// will resume on a standard MTA thread from the thread pool.
    /// To get an STA thread even after awaiting expressions, use <see cref="UITheoryAttribute" />.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.StaTheoryDiscoverer", ThisAssembly.AssemblyName)]
    public class StaTheoryAttribute : TheoryAttribute
    {
    }
}
