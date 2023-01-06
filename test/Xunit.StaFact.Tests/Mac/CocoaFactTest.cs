using System.Threading.Tasks;
using AppKit;
using Xunit;
public class CocoaFactTests
{
    [CocoaFact]
    public void AssertMainThreadSync()
        => NSApplication.EnsureUIThread();
    [Fact]
    public void AssertMainThreadSyncAndFail()
        => Assert.Throws<AppKitThreadAccessException>(NSApplication.EnsureUIThread);
    [CocoaFact]
    public Task AssertMainThreadAsyncReturnCompletedTask()
    {
        NSApplication.EnsureUIThread();
        return Task.CompletedTask;
    }
    [CocoaFact]
    public async Task AssertMainThreadAsyncWithAwait()
    {
        await Task.Yield();
        NSApplication.EnsureUIThread();
        await Task.Delay(100);
    }

    [CocoaFact]
    public void AssertExceptionsAreCaptured()
    {
        NSApplication.EnsureUIThread();
        throw new System.Exception("Thrown from Main");
    }
}
