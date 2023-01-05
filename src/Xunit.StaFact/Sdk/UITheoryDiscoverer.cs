// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk
{
    /// <summary>
    /// The discovery class for <see cref="UITheoryAttribute"/>.
    /// </summary>
    public class UITheoryDiscoverer : TheoryDiscoverer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UITheoryDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
        public UITheoryDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
        {
            yield return new UITestCase(UITestCase.SyncContextType.Portable, this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow);
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
        {
            yield return new UITheoryTestCase(UITestCase.SyncContextType.Portable, this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod);
        }
    }
}
