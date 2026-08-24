// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a shared WinForms UI thread for <see cref="WinFormsFactAttribute"/> and <see cref="WinFormsTheoryAttribute"/> tests.
/// </summary>
public class WinFormsThreadFixture : UIThreadFixtureBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WinFormsThreadFixture"/> class.
    /// </summary>
    public WinFormsThreadFixture()
        : base(UITestCase.SyncContextType.WinForms)
    {
    }
}
