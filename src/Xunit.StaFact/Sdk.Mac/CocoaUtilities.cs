// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

internal static class CocoaUtilities
{
    internal static IXunitTestCase CreateTestCase(
        TestCaseKind kind,
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        IFactAttribute theoryAttribute,
        object?[]? testMethodArguments)
    {
        return Utilities.CreateTestCase(
            kind,
            UITestCase.SyncContextType.Cocoa,
            null,
            discoveryOptions,
            testMethod,
            theoryAttribute,
            testMethodArguments);
    }
}
