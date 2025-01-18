// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class UISettingsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for a test.
    /// </summary>
    /// <value>
    /// <list type="bullet">
    /// <item><description>Leave unset to inherit the value from an attribute applied to a containing type (the ultimate default being <c>1</c>)</description></item>
    /// <item><description><c>1</c> to not retry the test on failure</description></item>
    /// <item><description>An explicit value greater than <c>1</c> to retry the test up to a total of this many attempts on failure</description></item>
    /// </list>
    /// </value>
    public int MaxAttempts { get; set; }

    internal static UISettingsAttribute Default => new() { MaxAttempts = 1 };

    /// <summary>
    /// Applies traits to a test case based on the settings in this attribute.
    /// </summary>
    /// <param name="testCase">The test case to add traits to.</param>
    internal void ApplyTraits(XunitTestCase testCase)
    {
        if (this.MaxAttempts > 1)
        {
            if (!testCase.Traits.ContainsKey("MaxAttempts"))
            {
                testCase.Traits.Add("MaxAttempts", new() { this.MaxAttempts.ToString() });
            }
        }
    }
}
