// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

/// <summary>
/// Verifies timeout failures emitted by the UI test runner.
/// </summary>
public class TimeoutTests
{
    private const int TimeoutMilliseconds = 500;

    /// <summary>
    /// Verifies that a blocked lifecycle fails before the blocked operation is released.
    /// </summary>
    /// <param name="methodName">The fixture method to execute.</param>
    /// <param name="stage">The lifecycle stage to block.</param>
    [Theory]
    [InlineData(nameof(TimeoutFixture.FactSync), "Constructor")]
    [InlineData(nameof(TimeoutFixture.FactSync), "InitializeSync")]
    [InlineData(nameof(TimeoutFixture.FactSync), "InitializeAsync")]
    [InlineData(nameof(TimeoutFixture.FactSync), "DisposeSync")]
    [InlineData(nameof(TimeoutFixture.FactSync), "DisposeAsync")]
    [InlineData(nameof(TimeoutFixture.FactSync), "Test")]
    [InlineData(nameof(TimeoutFixture.FactAsync), "Test")]
    [InlineData(nameof(TimeoutFixture.TheorySync), "Test")]
    [InlineData(nameof(TimeoutFixture.TheoryAsync), "Test")]
    [InlineData(nameof(TimeoutFixture.DelayedTheory), "Test")]
    public async Task Timeout_ReportsFailure(string methodName, string stage)
    {
        MethodInfo method = typeof(TimeoutFixture).GetMethod(methodName)!;
        XunitTestAssembly assembly = new(typeof(TimeoutFixture).Assembly, configFilePath: null, assemblyName: null);
        XunitTestCollection collection = new(assembly, null, disableParallelization: true, displayName: nameof(TimeoutTests));
        XunitTestClass testClass = new(typeof(TimeoutFixture), collection);
        XunitTestMethod testMethod = new(testClass, method, []);
        IFactAttribute attribute = Assert.Single(testMethod.FactAttributes);
        ITestFrameworkDiscoveryOptions options = TestFrameworkOptions.ForDiscovery(new TestAssemblyConfiguration());
        options.SetPreEnumerateTheories(true);
        IXunitTestCase testCase = Assert.Single(await (attribute is ITheoryAttribute
            ? new UITheoryDiscoverer().Discover(options, testMethod, attribute)
            : new UIFactDiscoverer().Discover(options, testMethod, attribute)));

        using BlockingState state = new(stage);
        using RecordingMessageBus messageBus = new();
        using CancellationTokenSource cancellation = new();
        await using ExecutionScheduler scheduler = ExecutionScheduler.CreateUnlimited();
        await using FixtureMappingManager fixtures = new("Method");
        Task<RunSummary> run = Task.Run(
            async () => await Assert.IsAssignableFrom<ISelfExecutingXunitTestCase>(testCase).Run(
                ExplicitOption.On,
                messageBus,
                [state],
                new ExceptionAggregator(),
                cancellation,
                ParallelMode.None,
                scheduler,
                fixtures),
            TestContext.Current.CancellationToken);

        try
        {
            Task completed = await Task.WhenAny(run, Task.Delay(10_000, TestContext.Current.CancellationToken));
            Assert.Same(run, completed);
            RunSummary summary = await run;
            Assert.Equal(1, summary.Total);
            Assert.Equal(1, summary.Failed);
            ITestFailed failure = Assert.Single(messageBus.Failures);
            Assert.Equal(typeof(TestTimeoutException).FullName, Assert.Single(failure.ExceptionTypes));
            Assert.Equal($"Test execution timed out after {TimeoutMilliseconds} milliseconds", Assert.Single(failure.Messages));
            Assert.True(state.Entered);
            Assert.True(state.CancellationToken.IsCancellationRequested);
            Assert.False(TestContext.Current.CancellationToken.IsCancellationRequested);
        }
        finally
        {
            state.Release();
            if (state.Entered)
            {
                Assert.Same(state.Unblocked, await Task.WhenAny(state.Unblocked, Task.Delay(10_000, TestContext.Current.CancellationToken)));
            }

            await run;
        }
    }

    /// <summary>
    /// An explicitly run fixture with independently blockable lifecycle stages.
    /// </summary>
    public sealed class TimeoutFixture : IAsyncLifetime
    {
        private readonly BlockingState state;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutFixture"/> class.
        /// </summary>
        /// <param name="state">The state controlling the blocked stage.</param>
        public TimeoutFixture(BlockingState state)
        {
            this.state = state;
            if (state.Stage == "Constructor")
            {
                state.Block();
            }
        }

        /// <inheritdoc/>
        public async ValueTask InitializeAsync()
        {
            if (this.state.Stage == "InitializeSync")
            {
                this.state.Block();
            }
            else if (this.state.Stage == "InitializeAsync")
            {
                await this.state.BlockAsync();
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (this.state.Stage == "DisposeSync")
            {
                this.state.Block();
            }
            else if (this.state.Stage == "DisposeAsync")
            {
                await this.state.BlockAsync();
            }
        }

#pragma warning disable xUnit1069 // Cancellation is recorded by BlockingState but deliberately not honored.
        /// <summary>
        /// Blocks a synchronous fact when invocation is the selected stage.
        /// </summary>
        [UIFact(Explicit = true, Timeout = TimeoutMilliseconds)]
        public void FactSync()
        {
            if (this.state.Stage == "Test")
            {
                this.state.Block();
            }
        }

        /// <summary>
        /// Blocks an asynchronous fact.
        /// </summary>
        [UIFact(Explicit = true, Timeout = TimeoutMilliseconds)]
        public async Task FactAsync()
        {
            await this.state.BlockAsync();
        }

        /// <summary>
        /// Blocks a pre-enumerated synchronous theory row.
        /// </summary>
        /// <param name="arg">The discovered row argument.</param>
        [UITheory(Explicit = true, Timeout = TimeoutMilliseconds)]
        [InlineData(0)]
        public void TheorySync(int arg)
        {
            Assert.Equal(0, arg);
            this.state.Block();
        }

        /// <summary>
        /// Blocks a pre-enumerated asynchronous theory row.
        /// </summary>
        /// <param name="arg">The discovered row argument.</param>
        [UITheory(Explicit = true, Timeout = TimeoutMilliseconds)]
        [InlineData(0)]
        public async Task TheoryAsync(int arg)
        {
            Assert.Equal(0, arg);
            await this.state.BlockAsync();
        }

        /// <summary>
        /// Blocks a theory row enumerated at execution time.
        /// </summary>
        /// <param name="arg">The discovered row argument.</param>
        [UITheory(Explicit = true, Timeout = TimeoutMilliseconds, DisableDiscoveryEnumeration = true)]
        [InlineData(0)]
        public async Task DelayedTheory(int arg)
        {
            Assert.Equal(0, arg);
            await this.state.BlockAsync();
        }
#pragma warning restore xUnit1069
    }

    /// <summary>
    /// Controls a blocked operation without relying on sleeps or unconditional failures.
    /// </summary>
    public sealed class BlockingState : IDisposable
    {
        private readonly ManualResetEventSlim release = new();
        private readonly TaskCompletionSource<object?> releaseAsync = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<object?> unblocked = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockingState"/> class.
        /// </summary>
        /// <param name="stage">The lifecycle stage to block.</param>
        public BlockingState(string stage)
        {
            this.Stage = stage;
        }

        /// <summary>
        /// Gets the stage to block.
        /// </summary>
        public string Stage { get; }

        /// <summary>
        /// Gets a value indicating whether the selected stage was entered.
        /// </summary>
        public bool Entered { get; private set; }

        /// <summary>
        /// Gets the cancellation token of the nested test.
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Gets a task that completes when the blocked operation exits.
        /// </summary>
        public Task Unblocked => this.unblocked.Task;

        /// <summary>
        /// Blocks the UI thread until the test releases it.
        /// </summary>
        public void Block()
        {
            this.CancellationToken = TestContext.Current.CancellationToken;
            this.Entered = true;
            this.release.Wait();
            this.unblocked.TrySetResult(null);
        }

        /// <summary>
        /// Asynchronously blocks until the test releases it.
        /// </summary>
        /// <returns>A task that completes when released.</returns>
        public async Task BlockAsync()
        {
            this.CancellationToken = TestContext.Current.CancellationToken;
            this.Entered = true;
            await this.releaseAsync.Task.ConfigureAwait(false);
            this.unblocked.TrySetResult(null);
        }

        /// <summary>
        /// Releases either kind of blocked operation.
        /// </summary>
        public void Release()
        {
            this.release.Set();
            this.releaseAsync.TrySetResult(null);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.release.Dispose();
        }
    }

    private sealed class RecordingMessageBus : IMessageBus
    {
        /// <summary>
        /// Gets the failure messages emitted by the nested runner.
        /// </summary>
        internal ConcurrentQueue<ITestFailed> Failures { get; } = new();

        /// <inheritdoc/>
        public bool QueueMessage(IMessageSinkMessage message)
        {
            if (message is ITestFailed failure)
            {
                this.Failures.Enqueue(failure);
            }

            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
