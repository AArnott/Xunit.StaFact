// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for the <see cref="UIFactAttribute"/>.
/// </summary>
public class UIFactDiscoverer : FactDiscoverer
{
    internal static UISettingsAttribute GetSettings(IXunitTestMethod testMethod)
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

    /// <inheritdoc/>
    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
    {
        UISettingsAttribute settings = GetSettings(testMethod);
        return new UITestCase(UITestCase.SyncContextType.Portable, discoveryOptions.MethodDisplayOrDefault(), testMethod, testMethodArguments: null, settings);
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
}
