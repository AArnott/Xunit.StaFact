// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Xunit.Sdk;

public class UIDelayEnumeratedTestCase : XunitDelayEnumeratedTheoryTestCase, ISelfExecutingXunitTestCase
{
    private UISettingsAttribute settings = UISettingsAttribute.Default;
    private UITestCase.SyncContextType synchronizationContextType;

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
        this.settings = settings;
        this.synchronizationContextType = synchronizationContextType;
    }

    public ValueTask<RunSummary> Run(
       ExplicitOption explicitOption,
       IMessageBus messageBus,
       object?[] constructorArguments,
       ExceptionAggregator aggregator,
       CancellationTokenSource cancellationTokenSource)
    {
        if (cancellationTokenSource is null)
        {
            throw new ArgumentNullException(nameof(cancellationTokenSource));
        }

        return UITestCaseRunner.Run(
            this,
            UITestCase.GetAdapter(this.synchronizationContextType),
            this.settings,
            explicitOption,
            messageBus,
            constructorArguments,
            aggregator,
            cancellationTokenSource);
    }

    protected override void Serialize(IXunitSerializationInfo data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        base.Serialize(data);
        data.AddValue(nameof(UISettingsAttribute.MaxAttempts), this.settings.MaxAttempts);
        data.AddValue(nameof(this.synchronizationContextType), this.synchronizationContextType);
    }

    protected override void Deserialize(IXunitSerializationInfo data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        base.Deserialize(data);
        this.settings = new()
        {
            MaxAttempts = data.GetValue<int>(nameof(UISettingsAttribute.MaxAttempts)),
        };
        string? syncContextTypeName = data.GetValue<string>(nameof(this.synchronizationContextType));
        if (syncContextTypeName is not null)
        {
            this.synchronizationContextType = (UITestCase.SyncContextType)Enum.Parse(typeof(UITestCase.SyncContextType), syncContextTypeName);
        }
    }
}
