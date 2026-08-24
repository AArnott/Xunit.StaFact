// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

public class DirectUIThreadFixtureTests : IClassFixture<UIThreadFixture>
{
    private static int sharedThreadId;

    public DirectUIThreadFixtureTests(UIThreadFixture fixture)
    {
    }

    [UIFact]
    public void FactUsesSharedThread()
    {
        AssertSharedThread();
    }

    [UITheory]
    [InlineData(1)]
    [InlineData(2)]
    public void TheoryUsesSharedThread(int value)
    {
        Assert.True(value > 0);
        AssertSharedThread();
    }

    private static void AssertSharedThread()
    {
        int threadId = Environment.CurrentManagedThreadId;
        int previousThreadId = Interlocked.CompareExchange(ref sharedThreadId, threadId, 0);
        Assert.True(previousThreadId is 0 || previousThreadId == threadId);
    }
}
