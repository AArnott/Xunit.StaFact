// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

#if !NET45 && !NETSTANDARD1_1

namespace Xunit
{
    using System;
    using Xunit.Sdk;

    /// <summary>
    /// Identifies an xunit theory that starts on an STA thread.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.StaTheoryDiscoverer", ThisAssembly.AssemblyName)]
    public class StaTheoryAttribute : TheoryAttribute
    {
    }
}

#endif
