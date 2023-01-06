// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit
{
    using System;
    using Xunit.Sdk;

    /// <summary>
    /// Identifies an xunit theory that starts on with a <see cref="System.Threading.SynchronizationContext"/>
    /// running on <see cref="Foundation.NSRunLoop.Main"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.CocoaTheoryDiscoverer", ThisAssembly.AssemblyName)]
    public class CocoaTheoryAttribute : TheoryAttribute
    {
    }
}
