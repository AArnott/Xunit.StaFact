// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal class UISettingsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for a test.
    /// </summary>
    /// <value>
    /// <list type="bullet">
    /// <item><description><c>0</c> to inherit the value from an attribute applied to a containing type or member, or use the default value when no other value is specified (equivalent to <c>1</c>; tests will not be automatically retried on failure)</description></item>
    /// <item><description><c>1</c> to not retry the test on failure</description></item>
    /// <item><description>An explicit value greater than <c>1</c> to retry the test up to a total of this many attempts on failure</description></item>
    /// </list>
    /// </value>
    public int MaxAttempts { get; set; }
}
