// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a shared WPF UI thread for <see cref="WpfFactAttribute"/> and <see cref="WpfTheoryAttribute"/> tests.
/// </summary>
public class WpfThreadFixture : UIThreadFixtureBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WpfThreadFixture"/> class.
    /// </summary>
    public WpfThreadFixture()
        : base(UITestCase.SyncContextType.WPF)
    {
    }
}
