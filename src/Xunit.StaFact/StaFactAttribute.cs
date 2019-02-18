// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

#if !NETSTANDARD1_1

namespace Xunit
{
    using System;
    using Sdk;

    /// <summary>
    /// Identifies an xunit test that starts on an STA thread.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.StaFactDiscoverer", ThisAssembly.AssemblyName)]
    public class StaFactAttribute : FactAttribute
    {
    }
}

#endif
