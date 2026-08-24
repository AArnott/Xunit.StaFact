// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WinUIFactAttribute"/>.
/// </summary>
public class WinUIFactDiscoverer : FactDiscoverer
{
    /// <inheritdoc/>
    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
    {
        return WinUIUtilities.CreateTestCaseForFact(
            discoveryOptions,
            testMethod,
            factAttribute);
    }
}

#endif
