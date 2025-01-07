// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for the <see cref="StaFactAttribute"/>.
/// </summary>
public class StaFactDiscoverer : FactDiscoverer
{
    /// <inheritdoc/>
    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
    {
        if (testMethod is null)
        {
            throw new ArgumentNullException(nameof(testMethod));
        }

        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITestCase(UITestCase.SyncContextType.None, discoveryOptions.MethodDisplayOrDefault(), testMethod, testMethodArguments: null, settings)
            : new XunitSkippedDataRowTestCase(discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "STA threads only exist on Windows.");
    }
}
