// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a shared WinUI thread for <see cref="WinUIFactAttribute"/> and <see cref="WinUITheoryAttribute"/> tests.
/// </summary>
public class WinUIThreadFixture : UIThreadFixtureBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WinUIThreadFixture"/> class.
    /// </summary>
    public WinUIThreadFixture()
        : base(UITestCase.SyncContextType.WinUI)
    {
    }
}

#endif
