// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for the <see cref="UIFactAttribute"/>.
/// </summary>
public class UIFactDiscoverer : FactDiscoverer
{
    private readonly IMessageSink diagnosticMessageSink;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIFactDiscoverer"/> class.
    /// </summary>
    /// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
    public UIFactDiscoverer(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        this.diagnosticMessageSink = diagnosticMessageSink;
    }

    internal static UISettingsAttribute GetSettings(ITestMethod testMethod)
    {
        // Initialize with defaults.
        UISettingsAttribute settings = UISettingsAttribute.Default;

        // Enumerate through each attribute (each progressively overriding the previous) and apply any explicitly set values to the attribute we'll return.
        foreach (IAttributeInfo settingsAttribute in GetSettingsAttributes(testMethod))
        {
            if (settingsAttribute.GetNamedArgument<int?>(nameof(UISettingsAttribute.MaxAttempts)) is int maxAttempts)
            {
                settings.MaxAttempts = maxAttempts;
            }
        }

        return settings;
    }

    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        UISettingsAttribute settings = GetSettings(testMethod);
        return new UITestCase(UITestCase.SyncContextType.Portable, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, testMethodArguments: null, settings);
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
