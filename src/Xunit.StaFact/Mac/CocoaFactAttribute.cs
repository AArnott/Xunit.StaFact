// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Threading;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Identifies an xunit test that starts on with a <see cref="System.Threading.SynchronizationContext"/>
    /// running on <see cref="Foundation.NSRunLoop.Main"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.CocoaFactDiscoverer", ThisAssembly.AssemblyName)]
    public class CocoaFactAttribute : FactAttribute
    {
    }
}
