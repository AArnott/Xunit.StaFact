// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a shared STA thread for <see cref="StaFactAttribute"/> and <see cref="StaTheoryAttribute"/> tests.
/// </summary>
/// <remarks>
/// This fixture does not install a synchronization context. Code after a yielding <see langword="await"/>
/// may resume on a thread-pool thread.
/// </remarks>
public class StaThreadFixture : UIThreadFixtureBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StaThreadFixture"/> class.
    /// </summary>
    public StaThreadFixture()
        : base(UITestCase.SyncContextType.None)
    {
    }
}
