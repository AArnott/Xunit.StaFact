// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Xunit.Sdk;

/// <summary>
/// Represents a test case that cannot run on the current platform.
/// </summary>
public class SkippedTestCase : XunitTestCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkippedTestCase"/> class
    /// for deserialization.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public SkippedTestCase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SkippedTestCase"/> class.
    /// </summary>
    /// <param name="testMethod">The test method this test case belongs to.</param>
    /// <param name="testCaseDisplayName">The display name for the test case.</param>
    /// <param name="uniqueID">The unique ID for the test case.</param>
    /// <param name="explicit">Indicates whether the test case was marked as explicit.</param>
    /// <param name="skipReason">The reason the test is skipped.</param>
    /// <param name="traits">The optional traits list.</param>
    /// <param name="testMethodArguments">The optional arguments for the test method.</param>
    /// <param name="sourceFilePath">The optional source file in where this test case originated.</param>
    /// <param name="sourceLineNumber">The optional source line number where this test case originated.</param>
    /// <param name="timeout">The optional timeout for the test case (in milliseconds).</param>
    internal SkippedTestCase(
        IXunitTestMethod testMethod,
        string testCaseDisplayName,
        string uniqueID,
        bool @explicit,
        string skipReason,
        Dictionary<string, HashSet<string>>? traits = null,
        object?[]? testMethodArguments = null,
        string? sourceFilePath = null,
        int? sourceLineNumber = null,
        int? timeout = null)
        : base(testMethod, testCaseDisplayName, uniqueID, @explicit, null, skipReason, null, null, null, traits, testMethodArguments, sourceFilePath, sourceLineNumber, timeout)
    {
    }
}
