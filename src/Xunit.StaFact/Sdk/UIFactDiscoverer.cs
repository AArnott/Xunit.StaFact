// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for the <see cref="UIFactAttribute"/>.
/// </summary>
public class UIFactDiscoverer : IXunitTestCaseDiscoverer
{
    /// <inheritdoc/>
    public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
    {
        return new(UIUtilities.CreateTestCasesForFact(discoveryOptions, testMethod, factAttribute));
    }
}
