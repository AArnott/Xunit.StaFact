// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Windows.Forms;

public class SharedWinFormsThreadFixtureTests : IClassFixture<SharedWinFormsThreadFixtureTests.TrackingWinFormsThreadFixture>
{
    private readonly TrackingWinFormsThreadFixture fixture;

    public SharedWinFormsThreadFixtureTests(TrackingWinFormsThreadFixture fixture)
    {
        this.fixture = fixture;
    }

    [WinFormsFact]
    public void FactUsesFixtureThread()
    {
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
    }

    public class TrackingWinFormsThreadFixture : WinFormsThreadFixture
    {
        public int ThreadId { get; private set; }

        protected override ValueTask InitializeOnUIThreadAsync()
        {
            this.ThreadId = Environment.CurrentManagedThreadId;
            Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
            return default;
        }
    }
}
