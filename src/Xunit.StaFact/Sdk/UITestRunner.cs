// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

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
        IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes)
    {
        await using UITestRunnerContext ctxt = new(
            settings,
            threadRental,
            test,
            messageBus,
            explicitOption,
            aggregator,
            cancellationTokenSource,
            beforeAfterAttributes,
            constructorArguments);

        await ctxt.InitializeAsync();

        return await this.Run(ctxt);
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
                // At this point, xUnit would set the synchronization context to
                // the one that was used when creating the test class. We have
                // our synchronization context that we use, so we don't do that.
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

                                    if (ctxt.ThreadRental.SyncContextAdapter.CanCompleteOperations)
                                    {
                                        invokeTime += await ExecutionTimer.MeasureAsync(async () =>
                                        {
                                            await ctxt.ThreadRental.SyncContextAdapter.WaitForOperationCompletionAsync(ctxt.ThreadRental.SynchronizationContext).ConfigureAwait(false);
                                        });
                                    }

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
