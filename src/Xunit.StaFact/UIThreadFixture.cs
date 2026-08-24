// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a shared portable UI thread for <see cref="UIFactAttribute"/> and <see cref="UITheoryAttribute"/> tests.
/// </summary>
public class UIThreadFixture : UIThreadFixtureBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UIThreadFixture"/> class.
    /// </summary>
    public UIThreadFixture()
        : base(UITestCase.SyncContextType.Portable)
    {
    }
}
