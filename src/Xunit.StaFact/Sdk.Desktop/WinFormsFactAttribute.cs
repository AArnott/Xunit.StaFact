// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

#if NETFRAMEWORK || NETCOREAPP

namespace Xunit
{
    using System;
    using Xunit.Sdk;

    /// <summary>
    /// Identifies an xunit test that starts on an STA thread
    /// with a WindowsFormsSynchronizationContext.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
#pragma warning disable CS0436 // Type conflicts with imported type
    [XunitTestCaseDiscoverer("Xunit.Sdk.WinFormsFactDiscoverer", ThisAssembly.AssemblyName)]
#pragma warning restore CS0436 // Type conflicts with imported type
    public class WinFormsFactAttribute : FactAttribute
    {
    }
}

#endif
