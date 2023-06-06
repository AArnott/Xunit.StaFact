// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WpfTheoryAttribute"/>.
/// </summary>
public class WpfTheoryDiscoverer : TheoryDiscoverer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WpfTheoryDiscoverer"/> class.
    /// </summary>
    /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
    public WpfTheoryDiscoverer(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
    }

    protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
    {
        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        yield return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITestCase(UITestCase.SyncContextType.WPF, this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow, settings)
            : new XunitSkippedDataRowTestCase(this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WPF only exists on Windows.");
    }

    protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
    {
        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        yield return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITheoryTestCase(UITestCase.SyncContextType.WPF, this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, settings)
            : new XunitSkippedDataRowTestCase(this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WPF only exists on Windows.");
    }
}
