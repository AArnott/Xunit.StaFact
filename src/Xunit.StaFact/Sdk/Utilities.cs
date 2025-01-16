// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Xunit.Internal;

namespace Xunit.Sdk;

internal static class Utilities
{
    internal static IXunitTestCase CreateTestCase(
        TestCaseKind kind,
        UITestCase.SyncContextType synchronizationContextType,
        string? skipReason,
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        IFactAttribute attribute,
        object?[]? testMethodArguments)
    {
        (string TestCaseDisplayName, bool Explicit, string? SkipReason, Type? SkipType, string? SkipUnless, string? SkipWhen, int Timeout, string UniqueID, IXunitTestMethod ResolvedTestMethod) details;
        Dictionary<string, HashSet<string>> traits;
        UISettingsAttribute settings;

        details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, attribute);
        settings = GetSettings(testMethod);
        traits = testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase);

        if (skipReason is not null)
        {
            return new SkippedTestCase(
               details.ResolvedTestMethod,
               details.TestCaseDisplayName,
               details.UniqueID,
               details.Explicit,
               skipReason,
               traits,
               testMethodArguments);
        }

        if (kind == TestCaseKind.DelayEnumerated)
        {
            return new UIDelayEnumeratedTestCase(
                settings,
                synchronizationContextType,
                details.ResolvedTestMethod,
                details.TestCaseDisplayName,
                details.UniqueID,
                details.Explicit,
                (attribute as ITheoryAttribute)?.SkipTestWithoutData ?? false,
                details.SkipReason,
                details.SkipType,
                details.SkipUnless,
                details.SkipWhen,
                traits,
                timeout: details.Timeout);
        }

        // Fact and DataRow test case kinds both result in a UITestCase.
        // The only difference is that DataRow should have test method
        // arguments, and Fact should not. The caller should pass null
        // for the test method arguments when the test case kind is Fact,
        // so we can use the same code path for both kinds.
        return new UITestCase(
            settings,
            synchronizationContextType,
            details.ResolvedTestMethod,
            details.TestCaseDisplayName,
            details.UniqueID,
            details.Explicit,
            details.SkipReason,
            details.SkipType,
            details.SkipUnless,
            details.SkipWhen,
            traits,
            testMethodArguments,
            timeout: details.Timeout);
    }

    internal static SyncContextAwaiter GetAwaiter(this SynchronizationContext synchronizationContext) => new SyncContextAwaiter(synchronizationContext);

    private static UISettingsAttribute GetSettings(IXunitTestMethod testMethod)
    {
        // Initialize with defaults.
        UISettingsAttribute settings = UISettingsAttribute.Default;

        // Enumerate through each attribute (each progressively overriding the previous) and apply any explicitly set values to the attribute we'll return.
        foreach (UISettingsAttribute settingsAttribute in GetSettingsAttributes(testMethod))
        {
            settings.MaxAttempts = settingsAttribute.MaxAttempts;
        }

        return settings;
    }

    private static IEnumerable<UISettingsAttribute> GetSettingsAttributes(IXunitTestMethod testMethod)
    {
        if (testMethod.TestClass.Class.GetCustomAttributes(typeof(UISettingsAttribute), true).SingleOrDefault() is UISettingsAttribute classLevel)
        {
            yield return classLevel;
        }

        if (testMethod.Method.GetCustomAttributes(typeof(UISettingsAttribute), true).SingleOrDefault() is UISettingsAttribute methodLevel)
        {
            yield return methodLevel;
        }
    }

    internal struct SyncContextAwaiter : ICriticalNotifyCompletion
    {
        private readonly SynchronizationContext synchronizationContext;

        internal SyncContextAwaiter(SynchronizationContext synchronizationContext)
        {
            this.synchronizationContext = synchronizationContext;
        }

        /// <summary>
        /// Gets a value indicating whether the caller is already running on the desired <see cref="SynchronizationContext"/>.
        /// </summary>
        public bool IsCompleted => (this.synchronizationContext is UISynchronizationContext uiSyncContext && uiSyncContext.IsInContext) || (SynchronizationContext.Current == this.synchronizationContext);

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            this.synchronizationContext.Post(s => ((Action)s!)(), continuation);
        }

        public void UnsafeOnCompleted(Action continuation) => this.OnCompleted(continuation);
    }
}
