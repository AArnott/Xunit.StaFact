// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit;

public class AppKitFactTests
{
    [AppKitFact]
    public void AssertMainThreadSync()
        => NSApplication.EnsureUIThread();

    [Fact]
    public void AssertMainThreadSyncAndFail()
        => Assert.Throws<AppKitThreadAccessException>(NSApplication.EnsureUIThread);

    [AppKitFact]
    public Task AssertMainThreadAsyncReturnCompletedTask()
    {
        NSApplication.EnsureUIThread();
        return Task.CompletedTask;
    }

    [AppKitFact]
    public async Task AssertMainThreadAsyncWithAwait()
    {
        await Task.Yield();
        NSApplication.EnsureUIThread();
        await Task.Delay(100);
    }
}
