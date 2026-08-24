// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

[CollectionDefinition(nameof(DirectUIThreadCollection))]
public class DirectUIThreadCollection : ICollectionFixture<UIThreadFixture>
{
    private static int sharedThreadId;

    public static void AssertSharedThread()
    {
        int threadId = Environment.CurrentManagedThreadId;
        int previousThreadId = Interlocked.CompareExchange(ref sharedThreadId, threadId, 0);
        Assert.True(previousThreadId is 0 || previousThreadId == threadId);
    }
}
