// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WinFormsFactAttribute"/>.
/// </summary>
public class WinFormsFactDiscoverer : FactDiscoverer
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
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITestCase(UITestCase.SyncContextType.WinForms, discoveryOptions.MethodDisplayOrDefault(), testMethod, testMethodArguments: null, settings)
            : new XunitSkippedDataRowTestCase(discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WinForms only exists on Windows.");
    }
}
