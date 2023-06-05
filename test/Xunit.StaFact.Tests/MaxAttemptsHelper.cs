// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

internal static class MaxAttemptsHelper
{
    private static readonly Dictionary<(Type, string), StrongBox<int>> AttemptNumber = new();

    /// <summary>
    /// Gets the current attempt number for a test involving automatic retries.
    /// </summary>
    /// <param name="testClass">The class defining the test.</param>
    /// <param name="testMethod">The unique name of the test method within <paramref name="testClass"/>.</param>
    /// <returns>The 0-based attempt number.</returns>
    public static int GetAndIncrementAttemptNumber(Type testClass, string testMethod)
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

        return Interlocked.Increment(ref attemptNumber.Value) - 1;
    }
}
