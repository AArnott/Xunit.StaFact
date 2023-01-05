// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Identifies an xunit test that starts on an STA thread
    /// with a <see cref="WindowsFormsSynchronizationContext" />.
    /// Tests will be Skipped on non-Windows operating systems.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.WinFormsFactDiscoverer", ThisAssembly.AssemblyName)]
    public class WinFormsFactAttribute : FactAttribute
    {
    }
}
