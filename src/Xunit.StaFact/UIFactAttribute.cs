// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit
{
    using System;
    using Sdk;

    /// <summary>
    /// Identifies an xunit test that starts on an STA thread
    /// with a UI thread-like SynchronizationContext.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.UIFactDiscoverer", "Xunit.StaFact.{Platform}")]
    public class UIFactAttribute : FactAttribute
    {
    }
}
