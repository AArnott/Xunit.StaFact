// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Xunit.Abstractions;

    /// <summary>
    /// The discovery class for <see cref="WinFormsTheoryAttribute"/>.
    /// </summary>
    public class WinFormsTheoryDiscoverer : TheoryDiscoverer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsTheoryDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
        public WinFormsTheoryDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
        {
            yield return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? (IXunitTestCase)new UITestCase(UITestCase.SyncContextType.WinForms, this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow)
                : new XunitSkippedDataRowTestCase(this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WinForms only exists on Windows.");
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
        {
            yield return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? (IXunitTestCase)new UITheoryTestCase(UITestCase.SyncContextType.WinForms, this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod)
                : new XunitSkippedDataRowTestCase(this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WinForms only exists on Windows.");
        }
    }
}
