// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;

namespace Xunit.Sdk;

/// <summary>
/// Wraps test cases for FactAttribute and TheoryAttribute so the test case runs on the WPF STA thread.
/// </summary>
[DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {DisplayName}, skip = {SkipReason} \}")]
public class UITestCase : XunitTestCase, ISelfExecutingXunitTestCase
{
    private UISettingsAttribute settings = UISettingsAttribute.Default;
    private SyncContextType synchronizationContextType;

    /// <summary>
    /// Initializes a new instance of the <see cref="UITestCase"/> class
    /// for deserialization.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public UITestCase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UITestCase"/> class.
    /// </summary>
    /// <param name="settings">The test settings to apply.</param>
    /// <param name="synchronizationContextType">The type of <see cref="SynchronizationContext"/> to use.</param>
    /// <param name="testMethod">The test method this test case belongs to.</param>
    /// <param name="testCaseDisplayName">The display name for the test case.</param>
    /// <param name="uniqueID">The unique ID for the test case.</param>
    /// <param name="explicit">Indicates whether the test case was marked as explicit.</param>
    /// <param name="skipExceptions">The value obtained from <see cref="IFactAttribute.SkipExceptions"/>.</param>
    /// <param name="skipReason">The value obtained from <see cref="IFactAttribute.Skip"/>.</param>
    /// <param name="skipType">The value obtained from <see cref="IFactAttribute.SkipType"/>.</param>
    /// <param name="skipUnless">The value obtained from <see cref="IFactAttribute.SkipUnless"/>.</param>
    /// <param name="skipWhen">The value obtained from <see cref="IFactAttribute.SkipWhen"/>.</param>
    /// <param name="traits">The optional traits list.</param>
    /// <param name="testMethodArguments">The optional arguments for the test method.</param>
    /// <param name="sourceFilePath">The optional source file in where this test case originated.</param>
    /// <param name="sourceLineNumber">The optional source line number where this test case originated.</param>
    /// <param name="timeout">The optional timeout for the test case (in milliseconds).</param>
    internal UITestCase(
        UISettingsAttribute settings,
        SyncContextType synchronizationContextType,
        IXunitTestMethod testMethod,
        string testCaseDisplayName,
        string uniqueID,
        bool @explicit,
        Type[]? skipExceptions = null,
        string? skipReason = null,
        Type? skipType = null,
        string? skipUnless = null,
        string? skipWhen = null,
        Dictionary<string, HashSet<string>>? traits = null,
        object?[]? testMethodArguments = null,
        string? sourceFilePath = null,
        int? sourceLineNumber = null,
        int? timeout = null)
        : base(testMethod, testCaseDisplayName, uniqueID, @explicit, skipExceptions, skipReason, skipType, skipUnless, skipWhen, traits, testMethodArguments, sourceFilePath, sourceLineNumber, timeout)
    {
        this.settings = settings;
        settings.ApplyTraits(this);
        this.synchronizationContextType = synchronizationContextType;
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

#if MACOS
        /// <summary>
        /// Use a <see cref="SynchronizationContext"/> running on <see cref="Foundation.NSRunLoop.Main"/>, which is only available on macOS.
        /// </summary>
        Cocoa,
#endif

#if NETFRAMEWORK || WINDOWS
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

    /// <inheritdoc/>
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
            GetAdapter(this.synchronizationContextType),
            this.settings,
            explicitOption,
            messageBus,
            constructorArguments,
            aggregator,
            cancellationTokenSource);
    }

    internal static SyncContextAdapter GetAdapter(SyncContextType syncContextType)
    {
        switch (syncContextType)
        {
            case SyncContextType.None:
                return NullAdapter.Default;

            case SyncContextType.Portable:
                return UISynchronizationContext.Adapter.Default;
#if MACOS
            case SyncContextType.Cocoa:
                return CocoaSynchronizationContextAdapter.Default;
#endif
#if NETFRAMEWORK || WINDOWS
            case SyncContextType.WPF:
                return DispatcherSynchronizationContextAdapter.Default;

            case SyncContextType.WinForms:
                return WinFormsSynchronizationContextAdapter.Default;
#endif
            default:
                throw new NotSupportedException("Unsupported type of SynchronizationContext.");
        }
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
        this.synchronizationContextType = data.GetValue<SyncContextType>(nameof(this.synchronizationContextType));
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
