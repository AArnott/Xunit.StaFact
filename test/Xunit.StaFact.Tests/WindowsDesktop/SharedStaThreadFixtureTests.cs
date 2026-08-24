// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

public class SharedStaThreadFixtureTests : IClassFixture<SharedStaThreadFixtureTests.TrackingStaThreadFixture>
{
    private readonly TrackingStaThreadFixture fixture;

    public SharedStaThreadFixtureTests(TrackingStaThreadFixture fixture)
    {
        this.fixture = fixture;
    }

    [StaFact]
    public void FactUsesFixtureThread()
    {
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        Assert.Null(SynchronizationContext.Current);
    }

    [StaTheory]
    [InlineData(1)]
    [InlineData(2)]
    public void TheoryUsesFixtureThread(int value)
    {
        Assert.True(value > 0);
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
    }

    public class TrackingStaThreadFixture : StaThreadFixture
    {
        public int ThreadId { get; private set; }

        protected override ValueTask InitializeOnUIThreadAsync()
        {
            this.ThreadId = Environment.CurrentManagedThreadId;
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
            Assert.Null(SynchronizationContext.Current);
            return default;
        }
    }
}
