// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Xunit.Sdk;

public class UITestRunner : XunitTestRunnerBase<UITestRunnerContext, IXunitTest>
{
    public static UITestRunner Instance { get; } = new();

    internal async ValueTask<RunSummary> Run(
        UISettingsAttribute settings,
        ThreadRental threadRental,
        IXunitTest test,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExplicitOption explicitOption,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager caseFixtureMappings)
    {
        await using UITestRunnerContext ctxt = new(
            settings,
            threadRental,
            test,
            explicitOption,
            messageBus,
            aggregator,
            cancellationTokenSource,
            parallelMode,
            scheduler,
            beforeAfterAttributes,
            constructorArguments,
            caseFixtureMappings);

        await ctxt.InitializeAsync();

        return await this.Run(ctxt);
    }

    protected override async ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(UITestRunnerContext ctxt)
    {
        if (ctxt is null)
        {
            throw new ArgumentNullException(nameof(ctxt));
        }

        Type @class = ctxt.Test.TestMethod.TestClass.Class;

        // Handle static test classes - they don't need to be instantiated
        // Static classes are both Abstract and Sealed in .NET reflection
        if (@class.IsAbstract && @class.IsSealed)
        {
            return (null, SynchronizationContext.Current, ExecutionContext.Capture());
        }

        // For non-static classes, use the base implementation
        return await base.CreateTestClassInstance(ctxt);
    }

    protected async override ValueTask<TimeSpan> RunTest(UITestRunnerContext ctxt)
    {
        if (ctxt is null)
        {
            throw new ArgumentNullException(nameof(ctxt));
        }

        if (ctxt.ThreadRental.SynchronizationContext is UISynchronizationContext uiSyncContext)
        {
            uiSyncContext.SetExceptionAggregator(ctxt.Aggregator);
        }

        int timeout = ctxt.Test.Timeout;
        if (ctxt.Aggregator.HasExceptions || timeout <= 0 || Debugger.IsAttached)
        {
            return await this.RunTestLifecycle(ctxt);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        TaskCompletionSource<TimeSpan> finished = new(TaskCreationOptions.RunContinuationsAsynchronously);
        ExecutionContext? executionContext = ExecutionContext.Capture();
        ctxt.ThreadRental.SynchronizationContext.Post(
            _ =>
            {
                if (executionContext is not null)
                {
                    ExecutionContext.Run(executionContext, RunLifecycle, null);
                }
                else
                {
                    RunLifecycle(null);
                }
            },
            null);

        using CancellationTokenSource delayCancellation = new();
        Task completedTask = await Task.WhenAny(finished.Task, Task.Delay(timeout, delayCancellation.Token)).ConfigureAwait(false);
        delayCancellation.Cancel();
        if (completedTask == finished.Task)
        {
            return await finished.Task.ConfigureAwait(false);
        }

        // Like xunit, report the timeout without waiting for the lifecycle to finish.
        // The UI thread continues until the test completes or observes cancellation.
        try
        {
            TestTimeoutException timeoutException = TestTimeoutException.ForTimedOutTest(timeout);
            ctxt.Aggregator.Add(timeoutException);
            this.UpdateTestContext(null, TestResultState.FromException((decimal)stopwatch.Elapsed.TotalSeconds, timeoutException));
        }
        finally
        {
            TestContext.Current.CancelCurrentTest();
        }

        return stopwatch.Elapsed;

        async void RunLifecycle(object? state)
        {
            try
            {
                SynchronizationContext.SetSynchronizationContext(
                    ctxt.ThreadRental.SyncContextAdapter.ShouldSetAsCurrent ? ctxt.ThreadRental.SynchronizationContext : null);
                finished.TrySetResult(await this.RunTestLifecycle(ctxt));
            }
            catch (Exception ex)
            {
                finished.TrySetException(ex);
            }
        }
    }

    private async ValueTask<TimeSpan> RunTestLifecycle(UITestRunnerContext ctxt)
    {
        object? testClassInstance = null;
        TimeSpan elapsedTime = TimeSpan.Zero;

        if (!ctxt.Aggregator.HasExceptions)
        {
            SynchronizationContext? syncContext = null;
            ExecutionContext? executionContext = null;

            elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(async () =>
            {
                await ctxt.ThreadRental.SynchronizationContext;
                (testClassInstance, syncContext, executionContext) = await this.CreateTestClassInstance(ctxt);
            }));

            TaskCompletionSource<object?> finished = new();

            if (executionContext is not null)
            {
                ExecutionContext.Run(executionContext, RunTest, null);
            }
            else
            {
                RunTest(null);
            }

            await finished.Task;

            async void RunTest(object? state)
            {
                SynchronizationContext.SetSynchronizationContext(syncContext);
                this.UpdateTestContext(testClassInstance);

                try
                {
                    if (!ctxt.Aggregator.HasExceptions)
                    {
                        elapsedTime += await ExecutionTimer.MeasureAsync(async () =>
                        {
                            await ctxt.ThreadRental.SynchronizationContext;
                            ctxt.Aggregator.Run(() => this.PreInvoke(ctxt));
                        });

                        if (!ctxt.Aggregator.HasExceptions)
                        {
                            elapsedTime += await ctxt.Aggregator.RunAsync(
                                async () =>
                                {
                                    await ctxt.ThreadRental.SynchronizationContext;
                                    TimeSpan invokeTime = await this.InvokeTest(ctxt, testClassInstance);
                                    return invokeTime;
                                },
                                TimeSpan.Zero);

                            // Set an early version of TestResultState so anything done in PostInvoke can understand whether
                            // it looks like the test is passing, failing, or dynamically skipped
                            var currentException = ctxt.Aggregator.ToException();
                            var currentSkipReason = ctxt.GetSkipReason(currentException);
                            var currentExecutionTime = (decimal)elapsedTime.TotalMilliseconds;
                            TestResultState testResultState =
                                currentSkipReason is not null
                                    ? TestResultState.ForSkipped(currentExecutionTime)
                                    : TestResultState.FromException(currentExecutionTime, currentException);

                            this.UpdateTestContext(testClassInstance, testResultState);

                            elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(async () =>
                            {
                                await ctxt.ThreadRental.SynchronizationContext;
                                this.PostInvoke(ctxt);
                            }));
                        }

                        elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(async () =>
                        {
                            await ctxt.ThreadRental.SynchronizationContext;
                            await this.DisposeTestClassInstance(ctxt, testClassInstance!);
                        }));

                        this.UpdateTestContext(null, TestContext.Current.TestState);
                    }

                    finished.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    finished.TrySetException(ex);
                }
            }
        }

        return elapsedTime;
    }
}
