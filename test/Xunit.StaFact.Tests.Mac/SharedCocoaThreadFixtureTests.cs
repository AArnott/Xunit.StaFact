// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit;

public class SharedCocoaThreadFixtureTests : IClassFixture<SharedCocoaThreadFixtureTests.TrackingCocoaThreadFixture>
{
    private readonly TrackingCocoaThreadFixture fixture;

    public SharedCocoaThreadFixtureTests(TrackingCocoaThreadFixture fixture)
    {
        this.fixture = fixture;
    }

    [CocoaFact]
    public void FactUsesFixtureThread()
    {
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.NotNull(SynchronizationContext.Current);
    }

    public class TrackingCocoaThreadFixture : CocoaThreadFixture
    {
        public int ThreadId { get; private set; }

        protected override ValueTask InitializeOnUIThreadAsync()
        {
            this.ThreadId = Environment.CurrentManagedThreadId;
            Assert.NotNull(SynchronizationContext.Current);
            return default;
        }
    }
}
