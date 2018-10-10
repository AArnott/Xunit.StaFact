// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Abstractions;

    /// <summary>
    /// The discovery class for the <see cref="StaFactAttribute"/>.
    /// </summary>
    public class StaFactDiscoverer : FactDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaFactDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
        public StaFactDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <inheritdoc/>
        protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            if (testMethod.Method.ReturnType.Name == "System.Void" &&
                testMethod.Method.GetCustomAttributes(typeof(AsyncStateMachineAttribute)).Any())
            {
#if NET45
                return new ExecutionErrorTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, "Async void methods are not supported.");
#else
                return new ExecutionErrorTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, "Async void methods are not supported.");
#endif
            }

            return new UITestCase(UITestCase.SyncContextType.None, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);
        }
    }
}
