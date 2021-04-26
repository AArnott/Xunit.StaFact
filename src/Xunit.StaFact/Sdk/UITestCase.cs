// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    /// <summary>
    /// Wraps test cases for FactAttribute and TheoryAttribute so the test case runs on the WPF STA thread.
    /// </summary>
    [DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {DisplayName}, skip = {SkipReason} \}")]
    public class UITestCase : XunitTestCase
    {
        private SyncContextType synchronizationContextType;

        /// <summary>
        /// Initializes a new instance of the <see cref="UITestCase"/> class.
        /// </summary>
        /// <param name="synchronizationContextType">The type of <see cref="SynchronizationContext"/> to use.</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages.</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        public UITestCase(
            SyncContextType synchronizationContextType,
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, TestMethodDisplayOptions.None, testMethod, testMethodArguments)
        {
            this.synchronizationContextType = synchronizationContextType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UITestCase"/> class
        /// for deserialization.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public UITestCase()
        {
        }

        public enum SyncContextType
        {
            /// <summary>
            /// No <see cref="SynchronizationContext"/> at all.
            /// </summary>
            None,

            /// <summary>
            /// Use the <see cref="UISynchronizationContext"/>, which works in portable profiles.
            /// </summary>
            Portable,

#if NETFRAMEWORK || NETCOREAPP
            /// <summary>
            /// Use the <see cref="System.Windows.Threading.DispatcherSynchronizationContext"/>, which is only available on Desktop.
            /// </summary>
            WPF,

            /// <summary>
            /// Use the <see cref="System.Windows.Forms.WindowsFormsSynchronizationContext"/>, which is only available on Desktop.
            /// </summary>
            WinForms,
#endif
        }

        private SyncContextAdapter Adapter => GetAdapter(this.synchronizationContextType);

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue(nameof(this.synchronizationContextType), this.synchronizationContextType);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            this.synchronizationContextType = (SyncContextType)data.GetValue(nameof(this.synchronizationContextType), typeof(SyncContextType));
        }

        /// <inheritdoc/>
        public override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            var task = Task.Run(
                async () =>
                {
                    using var threadRental = await ThreadRental.CreateAsync(this.Adapter, this.TestMethod);
                    await threadRental.SynchronizationContext;
                    var runner = new UITestCaseRunner(this, this.DisplayName, this.SkipReason, constructorArguments, this.TestMethodArguments, messageBus, aggregator, cancellationTokenSource, threadRental);
                    return await runner.RunAsync();
                },
                cancellationTokenSource.Token);

            // We need to block the XUnit thread to ensure it concurrency implementation.
            RunSummary runSummary = task.GetAwaiter().GetResult();
            return Task.FromResult(runSummary);
        }

        internal static SyncContextAdapter GetAdapter(SyncContextType syncContextType)
        {
            switch (syncContextType)
            {
                case SyncContextType.None:
                    return NullAdapter.Default;

                case SyncContextType.Portable:
                    return UISynchronizationContext.Adapter.Default;
#if NETFRAMEWORK || NETCOREAPP
                case SyncContextType.WPF:
                    return DispatcherSynchronizationContextAdapter.Default;

                case SyncContextType.WinForms:
                    return WinFormsSynchronizationContextAdapter.Default;
#endif
                default:
                    throw new NotSupportedException("Unsupported type of SynchronizationContext.");
            }
        }

        private class NullAdapter : UISynchronizationContext.Adapter
        {
            internal static new readonly NullAdapter Default = new NullAdapter();

            private NullAdapter()
            {
            }

            internal override bool ShouldSetAsCurrent => false;
        }
    }
}
