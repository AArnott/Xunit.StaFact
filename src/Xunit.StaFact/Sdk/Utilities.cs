// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using Xunit.Internal;

namespace Xunit.Sdk;

internal static class Utilities
{
    internal static IReadOnlyCollection<IXunitTestCase> CreateTestCasesForFact(
        UITestCase.SyncContextType synchronizationContextType,
        string? skipReason,
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        IFactAttribute factAttribute)
    {
        (string TestCaseDisplayName, bool Explicit, Type[]? SkipExceptions, string? SkipReason, Type? SkipType, string? SkipUnless, string? SkipWhen, string? SourceFilePath, int? SourceLineNumber, int Timeout, string UniqueID, IXunitTestMethod ResolvedTestMethod) details;
        Dictionary<string, HashSet<string>> traits;

        details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);
        traits = GetTraits(testMethod);

        return CreateTestCases(GetSettings(testMethod), culture => skipReason is not null
            ? new SkippedTestCase(
                details.ResolvedTestMethod,
                GetCultureDisplayName(details.TestCaseDisplayName, culture),
                GetCultureUniqueID(details.UniqueID, culture),
                details.Explicit,
                skipReason,
                GetCultureTraits(traits, culture),
                sourceFilePath: details.SourceFilePath,
                sourceLineNumber: details.SourceLineNumber)
            : new UITestCase(
                CreateSettings(GetSettings(testMethod), culture),
                synchronizationContextType,
                details.ResolvedTestMethod,
                GetCultureDisplayName(details.TestCaseDisplayName, culture),
                GetCultureUniqueID(details.UniqueID, culture),
                details.Explicit,
                details.SkipExceptions,
                details.SkipReason,
                details.SkipType,
                details.SkipUnless,
                details.SkipWhen,
                GetCultureTraits(traits, culture),
                sourceFilePath: details.SourceFilePath,
                sourceLineNumber: details.SourceLineNumber));
    }

    internal static IReadOnlyCollection<IXunitTestCase> CreateTestCasesForDataRow(
        UITestCase.SyncContextType synchronizationContextType,
        string? skipReason,
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute,
        ITheoryDataRow dataRow,
        object?[] testMethodArguments)
    {
        (string TestCaseDisplayName, bool Explicit, Type[]? SkipExceptions, string? SkipReason, Type? SkipType, string? SkipUnless, string? SkipWhen, string? SourceFilePath, int? SourceLineNumber, int Timeout, string UniqueID, IXunitTestMethod ResolvedTestMethod) details;
        Dictionary<string, HashSet<string>> traits;

        details = TestIntrospectionHelper.GetTestCaseDetailsForTheoryDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments);
        traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);

        return CreateTestCases(GetSettings(testMethod), culture => skipReason is not null
            ? new SkippedTestCase(
                details.ResolvedTestMethod,
                GetCultureDisplayName(details.TestCaseDisplayName, culture),
                GetCultureUniqueID(details.UniqueID, culture),
                details.Explicit,
                skipReason,
                GetCultureTraits(traits, culture),
                testMethodArguments,
                sourceFilePath: details.SourceFilePath,
                sourceLineNumber: details.SourceLineNumber)
            : new UITestCase(
                CreateSettings(GetSettings(testMethod), culture),
                synchronizationContextType,
                details.ResolvedTestMethod,
                GetCultureDisplayName(details.TestCaseDisplayName, culture),
                GetCultureUniqueID(details.UniqueID, culture),
                details.Explicit,
                details.SkipExceptions,
                details.SkipReason,
                details.SkipType,
                details.SkipUnless,
                details.SkipWhen,
                GetCultureTraits(traits, culture),
                testMethodArguments,
                sourceFilePath: details.SourceFilePath,
                sourceLineNumber: details.SourceLineNumber,
                timeout: details.Timeout));
    }

    internal static IReadOnlyCollection<IXunitTestCase> CreateTestCasesForTheory(
        UITestCase.SyncContextType synchronizationContextType,
        string? skipReason,
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute attribute)
    {
        (string TestCaseDisplayName, bool Explicit, Type[]? SkipExceptions, string? SkipReason, Type? SkipType, string? SkipUnless, string? SkipWhen, string? SourceFilePath, int? SourceLineNumber, int Timeout, string UniqueID, IXunitTestMethod ResolvedTestMethod) details;
        Dictionary<string, HashSet<string>> traits;

        details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, attribute);
        traits = GetTraits(testMethod);

        return CreateTestCases(GetSettings(testMethod), culture => skipReason is not null
            ? new SkippedTestCase(
                details.ResolvedTestMethod,
                GetCultureDisplayName(details.TestCaseDisplayName, culture),
                GetCultureUniqueID(details.UniqueID, culture),
                details.Explicit,
                skipReason,
                GetCultureTraits(traits, culture),
                sourceFilePath: details.SourceFilePath,
                sourceLineNumber: details.SourceLineNumber)
            : new UIDelayEnumeratedTestCase(
                CreateSettings(GetSettings(testMethod), culture),
                synchronizationContextType,
                details.ResolvedTestMethod,
                GetCultureDisplayName(details.TestCaseDisplayName, culture),
                GetCultureUniqueID(details.UniqueID, culture),
                details.Explicit,
                attribute.SkipTestWithoutData,
                details.SkipExceptions,
                details.SkipReason,
                details.SkipType,
                details.SkipUnless,
                details.SkipWhen,
                GetCultureTraits(traits, culture),
                sourceFilePath: details.SourceFilePath,
                sourceLineNumber: details.SourceLineNumber,
                timeout: details.Timeout));
    }

    internal static SyncContextAwaiter GetAwaiter(this SynchronizationContext synchronizationContext) => new SyncContextAwaiter(synchronizationContext);

    private static Dictionary<string, HashSet<string>> GetTraits(IXunitTestMethod testMethod)
    {
        return testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase);
    }

    private static UISettingsAttribute GetSettings(IXunitTestMethod testMethod)
    {
        // Initialize with defaults.
        UISettingsAttribute settings = UISettingsAttribute.Default;

        // Enumerate through each attribute (each progressively overriding the previous) and apply any explicitly set values to the attribute we'll return.
        foreach (UISettingsAttribute settingsAttribute in GetSettingsAttributes(testMethod))
        {
            if (settingsAttribute.MaxAttempts != 0)
            {
                settings.MaxAttempts = settingsAttribute.MaxAttempts;
            }

            if (settingsAttribute.Cultures is not null)
            {
                settings.Cultures = settingsAttribute.Cultures;
            }
        }

        return settings;
    }

    private static IReadOnlyCollection<IXunitTestCase> CreateTestCases(UISettingsAttribute settings, Func<string?, IXunitTestCase> createTestCase)
    {
        if (settings.Cultures is null)
        {
            return [createTestCase(null)];
        }

        string[] cultures = settings.Cultures;
        if (cultures.Length == 0 || cultures.Any(string.IsNullOrEmpty) || cultures.Distinct(StringComparer.OrdinalIgnoreCase).Count() != cultures.Length)
        {
            throw new ArgumentException("UISettingsAttribute.Cultures must contain one or more unique, non-empty culture names.");
        }

        foreach (string culture in cultures)
        {
            _ = new CultureInfo(culture, useUserOverride: false);
        }

        return cultures.Select(createTestCase).ToArray();
    }

    private static UISettingsAttribute CreateSettings(UISettingsAttribute settings, string? culture)
    {
        return new()
        {
            MaxAttempts = settings.MaxAttempts,
            Culture = culture,
        };
    }

    private static string GetCultureDisplayName(string displayName, string? culture) => culture is null ? displayName : $"{displayName}[{culture}]";

    private static string GetCultureUniqueID(string uniqueID, string? culture) => culture is null ? uniqueID : $"{uniqueID}[{culture}]";

    private static Dictionary<string, HashSet<string>> GetCultureTraits(Dictionary<string, HashSet<string>> traits, string? culture)
    {
        Dictionary<string, HashSet<string>> result = traits.ToDictionary(pair => pair.Key, pair => new HashSet<string>(pair.Value), StringComparer.OrdinalIgnoreCase);
        if (culture is not null)
        {
            result["Culture"] = new(StringComparer.OrdinalIgnoreCase) { culture };
        }

        return result;
    }

    private static IEnumerable<UISettingsAttribute> GetSettingsAttributes(IXunitTestMethod testMethod)
    {
        UISettingsAttribute? classLevel = null;
        UISettingsAttribute? methodLevel = null;

        try
        {
            var classAttributes = testMethod.TestClass.Class.GetCustomAttributes(typeof(UISettingsAttribute), true);
            classLevel = classAttributes.FirstOrDefault() as UISettingsAttribute;
        }
        catch (InvalidOperationException)
        {
            // Static classes may cause GetCustomAttributes to throw, so we swallow this exception
        }

        try
        {
            var methodAttributes = testMethod.Method.GetCustomAttributes(typeof(UISettingsAttribute), true);
            methodLevel = methodAttributes.FirstOrDefault() as UISettingsAttribute;
        }
        catch (InvalidOperationException)
        {
            // In case method attributes also have issues
        }

        if (classLevel is not null)
        {
            yield return classLevel;
        }

        if (methodLevel is not null)
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
