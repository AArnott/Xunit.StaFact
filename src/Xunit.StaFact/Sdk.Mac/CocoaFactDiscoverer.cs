// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="CocoaFactDiscoverer"/>.
/// </summary>
public class CocoaFactDiscoverer : FactDiscoverer
{
    /// <inheritdoc/>
    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
    {
        if (testMethod is null)
        {
            throw new ArgumentNullException(nameof(testMethod));
        }

        if (testMethod.Method.ReturnType.Name == "System.Void" &&
            testMethod.Method.GetCustomAttributes(typeof(AsyncStateMachineAttribute)).Any())
        {
            return new ExecutionErrorTestCase(discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, "Async void methods are not supported.");
        }

        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        return new UITestCase(UITestCase.SyncContextType.Cocoa, discoveryOptions.MethodDisplayOrDefault(), testMethod, testMethodArguments: null, settings);
    }
}
