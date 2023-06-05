// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WinUIFactAttribute"/>.
/// </summary>
public class WinUIFactDiscoverer : FactDiscoverer
{
    private readonly IMessageSink diagnosticMessageSink;

    /// <summary>
    /// Initializes a new instance of the <see cref="WinUIFactDiscoverer"/> class.
    /// </summary>
    /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
    public WinUIFactDiscoverer(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        this.diagnosticMessageSink = diagnosticMessageSink;
    }

    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        if (testMethod is null)
        {
            throw new ArgumentNullException(nameof(testMethod));
        }

        if (testMethod.Method.ReturnType.Name == "System.Void" &&
            testMethod.Method.GetCustomAttributes(typeof(AsyncStateMachineAttribute)).Any())
        {
            return new ExecutionErrorTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, "Async void methods are not supported.");
        }

        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITestCase(UITestCase.SyncContextType.WinUI, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod)
            : new XunitSkippedDataRowTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WinUI only exists on Windows.");
    }
}

#endif
