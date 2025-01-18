// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WinFormsFactAttribute"/>.
/// </summary>
public class WinFormsFactDiscoverer : FactDiscoverer
{
    /// <inheritdoc/>
    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
    {
        return WinFormsUtilities.CreateTestCaseForFact(
            discoveryOptions,
            testMethod,
            factAttribute);
    }
}
