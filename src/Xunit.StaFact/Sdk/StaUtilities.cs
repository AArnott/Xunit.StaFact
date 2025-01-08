// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Xunit.Sdk;

internal static class StaUtilities
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
            UITestCase.SyncContextType.None,
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? null : "STA threads only exist on Windows.",
            discoveryOptions,
            testMethod,
            theoryAttribute,
            testMethodArguments);
    }
}
