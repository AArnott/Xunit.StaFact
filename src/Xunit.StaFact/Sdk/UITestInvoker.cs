// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class UITestInvoker : XunitTestInvoker
    {
        private readonly ThreadRental threadRental;

        /// <summary>
        /// Initializes a new instance of the <see cref="UITestInvoker"/> class.
        /// </summary>
        internal UITestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, ThreadRental threadRental)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            this.threadRental = threadRental;
        }

        public new async Task<decimal> RunAsync()
        {
            if (this.threadRental.SynchronizationContext is UISynchronizationContext syncContext)
            {
                syncContext.SetExceptionAggregator(this.Aggregator);
            }

            await this.Aggregator.RunAsync(async delegate
            {
                if (!this.CancellationTokenSource.IsCancellationRequested)
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

                        if (!this.CancellationTokenSource.IsCancellationRequested)
                        {
                            await this.threadRental.SynchronizationContext;
                            await this.BeforeTestMethodInvokedAsync();

                            if (!this.CancellationTokenSource.IsCancellationRequested && !this.Aggregator.HasExceptions)
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
                                            var result = this.CallTestMethod(testClassInstance);
                                            if (result is Task task)
                                            {
                                                await task;
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
                            await this.Aggregator.RunAsync(this.AfterTestMethodInvokedAsync);
                        }

                        if (asyncLifetime is object)
                        {
                            await this.threadRental.SynchronizationContext;
                            await this.Aggregator.RunAsync(asyncLifetime.DisposeAsync);
                        }
                    }
                    finally
                    {
                        await this.threadRental.SynchronizationContext;
                        this.Aggregator.Run(() => this.Test.DisposeTestClass(testClassInstance, this.MessageBus, this.Timer, this.CancellationTokenSource));
                    }
                }
            });

            return this.Timer.Total;
        }
    }
}
