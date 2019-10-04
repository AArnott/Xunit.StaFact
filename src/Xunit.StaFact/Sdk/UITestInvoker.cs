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
        private SyncContextAdapter adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="UITestInvoker"/> class.
        /// </summary>
        internal UITestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, SyncContextAdapter syncContextAdapter)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            this.adapter = syncContextAdapter;
        }

        public new Task<decimal> RunAsync()
        {
            return this.RunOnStaIfPossibleOtherwiseUseMta(() =>
            {
                var uiSyncContext = this.adapter.Create();
                SynchronizationContext.SetSynchronizationContext(uiSyncContext);

                try
                {
                    decimal result = 0;
                    this.Aggregator.Run(delegate
                    {
                        if (!this.CancellationTokenSource.IsCancellationRequested)
                        {
                            var testClassInstance = this.CreateTestClass();

                            try
                            {
                                var asyncLifetime = testClassInstance as IAsyncLifetime;
                                if (asyncLifetime != null)
                                {
                                    this.adapter.Run(asyncLifetime.InitializeAsync);
                                }

                                if (!this.CancellationTokenSource.IsCancellationRequested)
                                {
                                    this.adapter.Run(this.BeforeTestMethodInvokedAsync);

                                    if (!this.CancellationTokenSource.IsCancellationRequested && !this.Aggregator.HasExceptions)
                                    {
                                        this.InvokeTestMethod(testClassInstance);
                                    }

                                    this.adapter.Run(this.AfterTestMethodInvokedAsync);
                                }

                                if (asyncLifetime != null)
                                {
                                    this.adapter.Run(() => this.Aggregator.RunAsync(asyncLifetime.DisposeAsync));
                                }
                            }
                            finally
                            {
                                this.Aggregator.Run(() => this.Test.DisposeTestClass(testClassInstance, this.MessageBus, this.Timer, this.CancellationTokenSource));
                            }
                        }

                        result = this.Timer.Total;
                    });

                    return result;
                }
                finally
                {
                    this.adapter.Cleanup();
                }
            });
        }

        private Task<decimal> RunOnStaIfPossibleOtherwiseUseMta(Func<decimal> action)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var tcs = new TaskCompletionSource<decimal>();
                var sta = new Thread(() =>
                {
                    try
                    {
                        tcs.SetResult(action());
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
                sta.SetApartmentState(ApartmentState.STA);
                sta.Start();
                return tcs.Task;
            }
            else
            {
                return Task.Run(action);
            }
        }

        private decimal InvokeTestMethod(object testClassInstance)
        {
            this.Aggregator.Run(
                () => this.Timer.Aggregate(
                    () =>
                    {
                        var parameterCount = this.TestMethod.GetParameters().Length;
                        var valueCount = this.TestMethodArguments == null ? 0 : this.TestMethodArguments.Length;
                        if (parameterCount != valueCount)
                        {
#pragma warning disable SA1119 // StyleCop bug https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/2058
                            this.Aggregator.Add(
                                new InvalidOperationException(
                                    $"The test method expected {parameterCount} parameter value{(parameterCount == 1 ? string.Empty : "s")}, but {valueCount} parameter value{(valueCount == 1 ? string.Empty : "s")} {(valueCount == 1 ? "was" : "were")} provided."));
#pragma warning restore SA1119
                        }
                        else
                        {
                            var result = this.CallTestMethod(testClassInstance);
                            var task = result as Task;
                            if (task != null)
                            {
                                this.adapter.PumpTill(task);
                                if (task.IsFaulted)
                                {
                                    this.Aggregator.Add(task.Exception.Flatten().InnerException ?? task.Exception);
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
                            else if (this.adapter.CanCompleteOperations)
                            {
                                this.adapter.CompleteOperations();
                            }
                        }
                    }));

            return this.Timer.Total;
        }
    }
}
