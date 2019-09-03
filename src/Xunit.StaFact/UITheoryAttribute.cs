// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit
{
    using System;
    using Xunit.Sdk;

    /// <summary>
    /// Identifies an xunit theory that starts on an STA thread
    /// with a WinForms SynchronizationContext.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.UITheoryDiscoverer", ThisAssembly.AssemblyName)]
    public class UITheoryAttribute : TheoryAttribute
    {
    }
}
