// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;

namespace Xunit.Sdk;

public class UITestRunner : XunitTestRunner
{
    private readonly ThreadRental threadRental;

    internal UITestRunner(ThreadRental threadRental)
    {
        this.threadRental = threadRental;
    }

    protected override async ValueTask<TimeSpan> InvokeTest(XunitTestRunnerContext ctxt, object? testClassInstance)
    {
        if (this.threadRental.SynchronizationContext is UISynchronizationContext syncContext)
        {
            syncContext.SetExceptionAggregator(ctxt.Aggregator);
        }

        await ctxt.Aggregator.RunAsync(async delegate
        {
            if (!ctxt.CancellationTokenSource.IsCancellationRequested)
            {
                await this.threadRental.SynchronizationContext;
                var testClassInstance = this.CreateTestClass();

                try
                {
                    var asyncLifetime = testClassInstance as IAsyncLifetime;
                    if (asyncLifetime is object)
                    {
                        await this.threadRental.SynchronizationContext;
                        await asyncLifetime.InitializeAsync();
                    }

                    if (!ctxt.CancellationTokenSource.IsCancellationRequested)
                    {
                        await this.threadRental.SynchronizationContext;
                        ctxt.RunBeforeAttributes();

                        if (!ctxt.Aggregator.HasExceptions)
                        {
                            if (!ctxt.CancellationTokenSource.IsCancellationRequested)
                            {
                                await this.Timer.AggregateAsync(
                                    async () =>
                                    {
                                        var parameterCount = this.TestMethod.GetParameters().Length;
                                        var valueCount = this.TestMethodArguments == null ? 0 : this.TestMethodArguments.Length;
                                        if (parameterCount != valueCount)
                                        {
                                            this.Aggregator.Add(
                                                new InvalidOperationException(
                                                    $"The test method expected {parameterCount} parameter value{(parameterCount == 1 ? string.Empty : "s")}, but {valueCount} parameter value{(valueCount == 1 ? string.Empty : "s")} {(valueCount == 1 ? "was" : "were")} provided."));
                                        }
                                        else
                                        {
                                            await this.threadRental.SynchronizationContext;
                                            object? result = null;
                                            try
                                            {
                                                result = this.CallTestMethod(testClassInstance);
                                            }
                                            catch (TargetInvocationException ex)
                                            {
                                                this.Aggregator.Add(ex.InnerException);
                                            }

                                            if (result is Task task)
                                            {
                                                await task.NoThrowAwaitable();
                                                if (task.IsFaulted)
                                                {
                                                    this.Aggregator.Add(task.Exception!.Flatten().InnerException ?? task.Exception);
                                                }
                                                else if (task.IsCanceled)
                                                {
                                                    try
                                                    {
                                                        // In order to get the original exception, in order to preserve the callstack,
                                                        // we must "rethrow" the exception.
                                                        task.GetAwaiter().GetResult();
                                                    }
                                                    catch (OperationCanceledException ex)
                                                    {
                                                        this.Aggregator.Add(ex);
                                                    }
                                                }
                                            }
                                            else if (this.threadRental.SyncContextAdapter.CanCompleteOperations)
                                            {
                                                await this.threadRental.SyncContextAdapter.WaitForOperationCompletionAsync(this.threadRental.SynchronizationContext).ConfigureAwait(false);
                                            }
                                        }
                                    });
                            }

                            await this.threadRental.SynchronizationContext;
                            ctxt.RunAfterAttributes();
                        }
                    }

                    if (asyncLifetime is object)
                    {
                        await this.threadRental.SynchronizationContext;
                        await ctxt.Aggregator.RunAsync(asyncLifetime.DisposeAsync);
                    }
                }
                finally
                {
                    await this.threadRental.SynchronizationContext;
                    ctxt.Aggregator.Run(() => ctxt.Test.DisposeTestClass(testClassInstance, this.MessageBus, this.Timer, this.CancellationTokenSource));
                }
            }
        });

        return this.Timer.Total;
    }

    private sealed class FilteringMessageBus : IMessageBus
    {
        private readonly IMessageBus messageBus;

        public FilteringMessageBus(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public void Dispose()
        {
            this.messageBus.Dispose();
        }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            if (message is ITestFailed testFailed)
            {
                message = ReportFailure("Test", testFailed);
            }
            else if (message is ITestCleanupFailure testCleanupFailure)
            {
                message = ReportFailure("Test cleanup", testCleanupFailure);
            }

            return this.messageBus.QueueMessage(message);

            static IMessageSinkMessage ReportFailure<T>(string header, T message)
                where T : ITestMessage, IErrorMetadata
            {
                // This test will run again; report it as skipped instead of failed.
                StringBuilder failures = new();
                failures.AppendLine($"{header} failed and will automatically retry. Failure details follow:");

                for (int i = 0; i < message.Messages.Length; i++)
                {
                    failures.AppendLine(message.Messages[i]);
                    failures.AppendLine(message.StackTraces[i]);
                }

                IExecutionMetadata? executionMetadata = message as IExecutionMetadata;
                ITestResultMessage? testResultMessage = message as ITestResultMessage;
                return new TestSkipped
                {
                    Reason = failures.ToString(),
                    AssemblyUniqueID = message.AssemblyUniqueID,
                    TestCollectionUniqueID = message.TestCollectionUniqueID,
                    TestClassUniqueID = message.TestClassUniqueID,
                    TestMethodUniqueID = message.TestMethodUniqueID,
                    TestUniqueID = message.TestUniqueID,
                    TestCaseUniqueID = message.TestCaseUniqueID,
                    Output = executionMetadata?.Output ?? string.Empty,
                    Warnings = executionMetadata?.Warnings ?? [],
                    ExecutionTime = executionMetadata?.ExecutionTime ?? 0,
                    FinishTime = testResultMessage?.FinishTime ?? DateTimeOffset.Now,
                };
            }
        }
    }
}
