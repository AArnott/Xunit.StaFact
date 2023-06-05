// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

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

    internal static UISettingsKey GetSettings(ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        var maxAttempts = GetMaxAttempts(testMethod, factAttribute);
        return new UISettingsKey(maxAttempts);
    }

    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        var maxAttempts = GetMaxAttempts(testMethod, factAttribute);
        var settings = new UISettingsKey(maxAttempts);
        return new UITestCase(UITestCase.SyncContextType.Portable, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, testMethodArguments: null, settings);
    }

    private static int GetMaxAttempts(ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        return GetMaxAttempts(factAttribute, GetSettingsAttributes(testMethod).ToArray());
    }

    private static IEnumerable<IAttributeInfo> GetSettingsAttributes(ITestMethod testMethod)
    {
        foreach (IAttributeInfo attributeData in testMethod.Method.GetCustomAttributes(typeof(UISettingsAttribute)))
        {
            yield return attributeData;
        }

        foreach (IAttributeInfo attributeData in testMethod.TestClass.Class.GetCustomAttributes(typeof(UISettingsAttribute)))
        {
            yield return attributeData;
        }
    }

    private static int GetMaxAttempts(IAttributeInfo factAttribute, IAttributeInfo[] settingsAttributes)
    {
        return GetNamedArgument(
            factAttribute,
            settingsAttributes,
            nameof(UISettingsAttribute.MaxAttempts),
            static value => value > 0,
            defaultValue: 1);
    }

    private static TValue GetNamedArgument<TValue>(IAttributeInfo factAttribute, IAttributeInfo[] settingsAttributes, string argumentName, Func<TValue, bool> isValidValue, TValue defaultValue)
    {
        return GetNamedArgument(
            factAttribute,
            settingsAttributes,
            argumentName,
            isValidValue,
            merge: null,
            defaultValue);
    }

    private static TValue GetNamedArgument<TValue>(IAttributeInfo factAttribute, IAttributeInfo[] settingsAttributes, string argumentName, Func<TValue, bool> isValidValue, Func<TValue, TValue, TValue>? merge, TValue defaultValue)
    {
        StrongBox<TValue>? result = null;
        if (TryGetNamedArgument(factAttribute, argumentName, isValidValue, out TValue? value))
        {
            if (merge is null)
            {
                return value;
            }

            result = new StrongBox<TValue>(value);
        }

        foreach (IAttributeInfo attribute in settingsAttributes)
        {
            if (TryGetNamedArgument(attribute, argumentName, isValidValue, out value))
            {
                if (merge is null)
                {
                    return value;
                }
                else if (result is null)
                {
                    result = new StrongBox<TValue>(value);
                }
                else
                {
                    result.Value = merge(value, result.Value!);
                }

                return value;
            }
        }

        if (result is not null)
        {
            return result.Value!;
        }

        return defaultValue;

        static bool TryGetNamedArgument(IAttributeInfo attribute, string argumentName, Func<TValue, bool> isValidValue, out TValue value)
        {
            value = attribute.GetNamedArgument<TValue>(argumentName);
            return isValidValue(value);
        }
    }
}
