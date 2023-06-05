// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Xunit.Sdk;

public class UITheoryTestCase : XunitTheoryTestCase
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public UITheoryTestCase()
    {
    }

    internal UITheoryTestCase(UITestCase.SyncContextType synchronizationContextType, IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
        : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
    {
        this.Settings = UIFactDiscoverer.GetSettings(testMethod, theoryAttribute);
        this.SynchronizationContextType = synchronizationContextType;
    }

    internal UISettingsKey Settings { get; private set; }

    internal UITestCase.SyncContextType SynchronizationContextType { get; private set; }

    public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        using ThreadRental threadRental = await ThreadRental.CreateAsync(UITestCase.GetAdapter(this.SynchronizationContextType), this.TestMethod);
        await threadRental.SynchronizationContext;
        return await new UITheoryTestCaseRunner(this, this.DisplayName, this.SkipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, this.Settings, threadRental).RunAsync();
    }

    public override void Deserialize(IXunitSerializationInfo data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        base.Deserialize(data);

        this.Settings = new UISettingsKey(data.GetValue<int>(nameof(UISettingsAttribute.MaxAttempts)));
        this.SynchronizationContextType = (UITestCase.SyncContextType)Enum.Parse(typeof(UITestCase.SyncContextType), data.GetValue<string>("SyncContextType"));
    }

    public override void Serialize(IXunitSerializationInfo data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        base.Serialize(data);

        data.AddValue(nameof(UISettingsAttribute.MaxAttempts), this.Settings.MaxAttempts);
        data.AddValue("SyncContextType", this.SynchronizationContextType.ToString());
    }
}
