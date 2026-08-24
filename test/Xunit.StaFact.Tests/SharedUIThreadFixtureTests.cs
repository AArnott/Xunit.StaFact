// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;

public class SharedUIThreadFixtureTests : IClassFixture<SharedUIThreadFixtureTests.TrackingUIThreadFixture>
{
    private readonly TrackingUIThreadFixture fixture;

    public SharedUIThreadFixtureTests(TrackingUIThreadFixture fixture)
    {
        this.fixture = fixture;
    }

    [UIFact]
    public void FactUsesFixtureThread()
    {
        Assert.True(this.fixture.Initialized);
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.fixture.Context, SynchronizationContext.Current);
    }

    [UITheory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task TheoryUsesFixtureThread(int value)
    {
        Assert.True(value > 0);
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        await Task.Yield();
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
    }

    [Fact]
    public async Task LifecycleHooksUseFixtureThread()
    {
        var fixture = new TrackingUIThreadFixture();
        await fixture.InitializeAsync();

        Assert.True(fixture.Initialized);
        Assert.NotEqual(Environment.CurrentManagedThreadId, fixture.ThreadId);

        await fixture.DisposeAsync();

        Assert.True(fixture.Disposed);
        Assert.Equal(fixture.ThreadId, fixture.DisposalThreadId);
    }

    [Fact]
    public void IncompatibleFixtureIsRejected()
    {
        var uiFixture = new UIThreadFixture();
        var staFixture = new StaThreadFixture();

        InvalidOperationException exception = InvokeGetSharedThreadFixture([uiFixture], staFixture);

        Assert.Contains("not compatible", exception.Message);
    }

    [Fact]
    public void MultipleFixturesAreRejected()
    {
        var firstFixture = new UIThreadFixture();
        var secondFixture = new UIThreadFixture();

        InvalidOperationException exception = InvokeGetSharedThreadFixture([firstFixture, secondFixture], firstFixture);

        Assert.Contains("only one", exception.Message);
    }

    private static InvalidOperationException InvokeGetSharedThreadFixture(object?[] constructorArguments, UIThreadFixtureBase adapterSource)
    {
        MethodInfo method = typeof(Xunit.Sdk.UITestCaseRunner).GetMethod("GetSharedThreadFixture", BindingFlags.NonPublic | BindingFlags.Static)!;
        PropertyInfo adapterProperty = typeof(UIThreadFixtureBase).GetProperty("SyncContextAdapter", BindingFlags.NonPublic | BindingFlags.Instance)!;
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
            () => method.Invoke(null, [constructorArguments, adapterProperty.GetValue(adapterSource)]));
        return Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    public class TrackingUIThreadFixture : UIThreadFixture
    {
        public SynchronizationContext? Context { get; private set; }

        public int DisposalThreadId { get; private set; }

        public bool Disposed { get; private set; }

        public bool Initialized { get; private set; }

        public int ThreadId { get; private set; }

        protected override ValueTask InitializeOnUIThreadAsync()
        {
            this.Context = SynchronizationContext.Current;
            this.ThreadId = Environment.CurrentManagedThreadId;
            this.Initialized = true;
            return default;
        }

        protected override ValueTask DisposeOnUIThreadAsync()
        {
            this.DisposalThreadId = Environment.CurrentManagedThreadId;
            this.Disposed = true;
            return default;
        }
    }
}
