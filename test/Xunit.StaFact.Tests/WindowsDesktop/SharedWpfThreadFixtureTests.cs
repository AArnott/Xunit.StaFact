// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Windows.Threading;

public class SharedWpfThreadFixtureTests : IClassFixture<SharedWpfThreadFixtureTests.TrackingWpfThreadFixture>
{
    private readonly TrackingWpfThreadFixture fixture;

    public SharedWpfThreadFixtureTests(TrackingWpfThreadFixture fixture)
    {
        this.fixture = fixture;
    }

    [WpfFact]
    public async Task FactsUseFixtureDispatcher()
    {
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.IsType<DispatcherSynchronizationContext>(this.fixture.Context);
        Assert.IsType<DispatcherSynchronizationContext>(SynchronizationContext.Current);

        await Task.Yield();

        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
    }

    public class TrackingWpfThreadFixture : WpfThreadFixture
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
