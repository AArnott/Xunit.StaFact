// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;

    /// <summary>
    /// The discovery class for <see cref="WpfTheoryAttribute"/>
    /// </summary>
    public class WpfTheoryDiscoverer : TheoryDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfTheoryDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
        public WpfTheoryDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override IXunitTestCase CreateTestCaseForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
        {
            return new UITestCase(UITestCase.SyncContextType.WPF, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow);
        }
    }
}
