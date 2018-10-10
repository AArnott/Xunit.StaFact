// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using Abstractions;

    /// <summary>
    /// Wraps test cases for FactAttribute and TheoryAttribute so the test case runs on the WPF STA thread
    /// </summary>
    [DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {DisplayName}, skip = {SkipReason} \}")]
    public class WpfTestCase : LongLivedMarshalByRefObject, IXunitTestCase
    {
        private IXunitTestCase testCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfTestCase"/> class.
        /// </summary>
        /// <param name="testCase">The test case.</param>
        public WpfTestCase(IXunitTestCase testCase)
        {
            this.testCase = testCase;
        }

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", error: true)]
        public WpfTestCase()
        {
        }

        /// <inheritdoc/>
        public IMethodInfo Method => this.testCase.Method;

        /// <inheritdoc/>
        public string DisplayName => this.testCase.DisplayName;

        /// <inheritdoc/>
        public string SkipReason => this.testCase.SkipReason;

        /// <inheritdoc/>
        public ISourceInformation SourceInformation
        {
            get { return this.testCase.SourceInformation; }
            set { this.testCase.SourceInformation = value; }
        }

        /// <inheritdoc/>
        public ITestMethod TestMethod => this.testCase.TestMethod;

        /// <inheritdoc/>
        public object[] TestMethodArguments => this.testCase.TestMethodArguments;

        /// <inheritdoc/>
        public Dictionary<string, List<string>> Traits => this.testCase.Traits;

        /// <inheritdoc/>
        public string UniqueID => this.testCase.UniqueID;

#if !NET45

        /// <inheritdoc/>
        public Exception InitializationException { get; set; }

        /// <inheritdoc/>
        public int Timeout => -1;

#endif

        /// <inheritdoc/>
        public Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            var tcs = new TaskCompletionSource<RunSummary>();
            var thread = new Thread(() =>
            {
                try
                {
                    // Set up the SynchronizationContext so that any awaits
                    // resume on the STA thread as they would in a GUI app.
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());

                    // Start off the test method.
                    var testCaseTask = this.testCase.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);

                    // Arrange to pump messages to execute any async work associated with the test.
                    var frame = new DispatcherFrame();
                    Task.Run(async delegate
                    {
                        try
                        {
                            await testCaseTask;
                        }
                        finally
                        {
                            // The test case's execution is done. Terminate the message pump.
                            frame.Continue = false;
                        }
                    });
                    Dispatcher.PushFrame(frame);

                    // Report the result back to the Task we returned earlier.
                    CopyTaskResultFrom(tcs, testCaseTask);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <inheritdoc/>
        public void Deserialize(IXunitSerializationInfo info)
        {
            this.testCase = info.GetValue<IXunitTestCase>("InnerTestCase");
        }

        /// <inheritdoc/>
        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("InnerTestCase", this.testCase);
        }

        private static void CopyTaskResultFrom<T>(TaskCompletionSource<T> tcs, Task<T> template)
        {
            if (tcs == null)
            {
                throw new ArgumentNullException(nameof(tcs));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (!template.IsCompleted)
            {
                throw new ArgumentException("Task must be completed first.", nameof(template));
            }

            if (template.IsFaulted)
            {
                tcs.SetException(template.Exception);
            }
            else if (template.IsCanceled)
            {
                tcs.SetCanceled();
            }
            else
            {
                tcs.SetResult(template.Result);
            }
        }
    }
}