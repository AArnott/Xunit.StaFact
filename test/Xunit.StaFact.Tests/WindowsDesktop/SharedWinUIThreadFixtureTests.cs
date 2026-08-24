// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

using Microsoft.UI.Dispatching;

public class SharedWinUIThreadFixtureTests : IClassFixture<SharedWinUIThreadFixtureTests.TrackingWinUIThreadFixture>
{
    private readonly TrackingWinUIThreadFixture fixture;

    public SharedWinUIThreadFixtureTests(TrackingWinUIThreadFixture fixture)
    {
        this.fixture = fixture;
    }

    [WinUIFact]
    public async Task FactsUseFixtureDispatcher()
    {
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.IsType<DispatcherQueueSynchronizationContext>(this.fixture.Context);
        Assert.IsType<DispatcherQueueSynchronizationContext>(SynchronizationContext.Current);

        await Task.Yield();

        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
    }

    public class TrackingWinUIThreadFixture : WinUIThreadFixture
    {
        public SynchronizationContext? Context { get; private set; }

        public int ThreadId { get; private set; }

        protected override ValueTask InitializeOnUIThreadAsync()
        {
            this.Context = SynchronizationContext.Current;
            this.ThreadId = Environment.CurrentManagedThreadId;
            return default;
        }
    }
}

#endif
