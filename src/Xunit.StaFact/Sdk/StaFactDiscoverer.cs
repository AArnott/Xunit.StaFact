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
    /// <summary>
    /// Initializes a new instance of the <see cref="StaFactDiscoverer"/> class.
    /// </summary>
    public StaFactDiscoverer()
    {
    }

    /// <inheritdoc/>
    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        if (testMethod is null)
        {
            throw new ArgumentNullException(nameof(testMethod));
        }

        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITestCase(UITestCase.SyncContextType.None, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, testMethodArguments: null, settings)
            : new XunitSkippedDataRowTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "STA threads only exist on Windows.");
    }
}
