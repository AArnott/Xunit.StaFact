// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit
{
    using System;
    using System.Windows.Threading;
    using Xunit.Sdk;

    /// <summary>
    /// Identifies an xunit theory that starts on an STA thread
    /// with a WPF <see cref="DispatcherSynchronizationContext"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.WpfTheoryDiscoverer", ThisAssembly.AssemblyName)]
    public class WpfTheoryAttribute : TheoryAttribute
    {
    }
}
