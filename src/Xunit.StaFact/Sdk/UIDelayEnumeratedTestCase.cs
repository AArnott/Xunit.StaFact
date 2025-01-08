// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Xunit.Sdk;

public class UIDelayEnumeratedTestCase : XunitDelayEnumeratedTheoryTestCase
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public UIDelayEnumeratedTestCase()
    {
    }

    internal UIDelayEnumeratedTestCase(
        UISettingsAttribute settings,
        UITestCase.SyncContextType synchronizationContextType,
        IXunitTestMethod testMethod,
        string testCaseDisplayName,
        string uniqueID,
        bool @explicit,
        bool skipTestWithoutData,
        string? skipReason = null,
        Type? skipType = null,
        string? skipUnless = null,
        string? skipWhen = null,
        Dictionary<string, HashSet<string>>? traits = null,
        string? sourceFilePath = null,
        int? sourceLineNumber = null,
        int? timeout = null)
        : base(testMethod, testCaseDisplayName, uniqueID, @explicit, skipTestWithoutData, skipReason, skipType, skipUnless, skipWhen, traits, sourceFilePath, sourceLineNumber, timeout)
    {
        this.Settings = settings;
        this.SynchronizationContextType = synchronizationContextType;
    }

    internal UISettingsAttribute Settings { get; private set; } = UISettingsAttribute.Default;

    internal UITestCase.SyncContextType SynchronizationContextType { get; private set; }

    public Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        using ThreadRental threadRental = await ThreadRental.CreateAsync(UITestCase.GetAdapter(this.SynchronizationContextType), this.TestMethod);
        await threadRental.SynchronizationContext;

        // TODO: retry here if any test cases failed
        return await new UITestCaseRunner(this, this.DisplayName, this.SkipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, threadRental).RunAsync();
    }

    protected override void Deserialize(IXunitSerializationInfo data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        base.Deserialize(data);

        this.Settings = new UISettingsAttribute() { MaxAttempts = data.GetValue<int>(nameof(UISettingsAttribute.MaxAttempts)) };

        string? syncContextTypeName = data.GetValue<string>("SyncContextType");
        if (syncContextTypeName is not null)
        {
            this.SynchronizationContextType = (UITestCase.SyncContextType)Enum.Parse(typeof(UITestCase.SyncContextType), syncContextTypeName);
        }
    }

    protected override void Serialize(IXunitSerializationInfo data)
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
