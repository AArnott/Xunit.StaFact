// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Xunit.Sdk;

internal static class MaxAttemptsHelper
{
    private static readonly Dictionary<(Type, string), StrongBox<int>> AttemptNumber = new();

    /// <summary>
    /// Gets the current attempt number for a test involving automatic retries.
    /// </summary>
    /// <param name="testClass">The class defining the test.</param>
    /// <param name="testMethod">The unique name of the test method within <paramref name="testClass"/>.</param>
    /// <returns>The current (1-based) attempt number.</returns>
    internal static int GetAndIncrementAttemptNumber(Type testClass, string testMethod)
    {
        StrongBox<int>? attemptNumber;
        lock (AttemptNumber)
        {
            if (!AttemptNumber.TryGetValue((testClass, testMethod), out attemptNumber))
            {
                attemptNumber = new();
                AttemptNumber.Add((testClass, testMethod), attemptNumber);
            }
        }

        return Interlocked.Increment(ref attemptNumber.Value);
    }

    /// <summary>
    /// Throws an exception to fail a test if the current attempt number is not the one designated as the passing one.
    /// </summary>
    /// <param name="testClass">The test class type. Used for counting attempts.</param>
    /// <param name="testMethod">The name of the test method, including any theory arguments. Used for counting attempts.</param>
    /// <param name="passingAttemptNumber">The attempt number which should pass.</param>
    /// <exception cref="FailException">Thrown when the <paramref name="passingAttemptNumber"/> does not match the current attempt number.</exception>
    internal static void ThrowUnlessAttemptNumber(Type testClass, string testMethod, int passingAttemptNumber)
    {
        int attemptNumber = GetAndIncrementAttemptNumber(testClass, testMethod);
        if (attemptNumber != passingAttemptNumber)
        {
            Assert.Fail($"This test fails on attempt {attemptNumber} (but is expected to pass on attempt {passingAttemptNumber}).");
        }
    }
}
