﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

#if NETFRAMEWORK || NETCOREAPP

namespace Xunit
{
    using System;
    using Xunit.Sdk;

    /// <summary>
    /// Identifies an xunit test that starts on an STA thread
    /// with a WPF DispatcherSynchronizationContext.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.WpfFactDiscoverer", ThisAssembly.AssemblyName)]
    public class WpfFactAttribute : FactAttribute
    {
    }
}

#endif
