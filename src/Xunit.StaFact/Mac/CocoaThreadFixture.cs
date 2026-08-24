// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a shared Cocoa UI thread for <see cref="CocoaFactAttribute"/> and <see cref="CocoaTheoryAttribute"/> tests.
/// </summary>
public class CocoaThreadFixture : UIThreadFixtureBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CocoaThreadFixture"/> class.
    /// </summary>
    public CocoaThreadFixture()
        : base(UITestCase.SyncContextType.Cocoa)
    {
    }
}
