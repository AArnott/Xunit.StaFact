// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="AppKitFactDiscoverer"/>.
/// </summary>
public class AppKitFactDiscoverer : FactDiscoverer
{
    private readonly IMessageSink diagnosticMessageSink;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppKitFactDiscoverer"/> class.
    /// </summary>
    /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
    public AppKitFactDiscoverer(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        this.diagnosticMessageSink = diagnosticMessageSink;
    }

    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        if (testMethod.Method.ReturnType.Name == "System.Void" &&
            testMethod.Method.GetCustomAttributes(typeof(AsyncStateMachineAttribute)).Any())
        {
            return new ExecutionErrorTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, "Async void methods are not supported.");
        }

        return new UITestCase(UITestCase.SyncContextType.AppKit, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);
    }
}
