// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class UITheoryTestCase : XunitTheoryTestCase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public UITheoryTestCase()
        {
        }

        public UITheoryTestCase(UITestCase.SyncContextType synchronizationContextType, IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
        {
            this.SynchronizationContextType = synchronizationContextType;
        }

        internal UITestCase.SyncContextType SynchronizationContextType { get; private set; }

        internal ThreadRental ThreadRental { get; private set; }

        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            this.ThreadRental = await ThreadRental.CreateAsync(UITestCase.GetAdapter(this.SynchronizationContextType), this.TestMethod);
            await this.ThreadRental.SynchronizationContext;
            return await new UITheoryTestCaseRunner(this, this.DisplayName, this.SkipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            this.SynchronizationContextType = (UITestCase.SyncContextType)Enum.Parse(typeof(UITestCase.SyncContextType), data.GetValue<string>("SyncContextType"));
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("SyncContextType", this.SynchronizationContextType.ToString());
        }

        public override void Dispose()
        {
            this.ThreadRental?.Dispose();
            base.Dispose();
        }
    }
}
