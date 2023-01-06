// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The discovery class for <see cref="CocoaFactDiscoverer"/>.
    /// </summary>
    public class CocoaFactDiscoverer : FactDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="CocoaFactDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
        public CocoaFactDiscoverer(IMessageSink diagnosticMessageSink)
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

            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? (IXunitTestCase)new UITestCase(UITestCase.SyncContextType.Cocoa, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod)
                : new XunitSkippedDataRowTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "Cocoa only exists on macOS.");
        }
    }
}