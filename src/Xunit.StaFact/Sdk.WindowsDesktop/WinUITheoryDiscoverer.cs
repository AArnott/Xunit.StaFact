// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER

using System.Runtime.InteropServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WinUITheoryAttribute"/>.
/// </summary>
public class WinUITheoryDiscoverer : TheoryDiscoverer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WinUITheoryDiscoverer"/> class.
    /// </summary>
    /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
    public WinUITheoryDiscoverer(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
    }

    protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
    {
        yield return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITestCase(UITestCase.SyncContextType.WinUI, this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow)
            : new XunitSkippedDataRowTestCase(this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WinUI only exists on Windows.");
    }

    protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
    {
        yield return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITheoryTestCase(UITestCase.SyncContextType.WinUI, this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod)
            : new XunitSkippedDataRowTestCase(this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WinUI only exists on Windows.");
    }
}

#endif
