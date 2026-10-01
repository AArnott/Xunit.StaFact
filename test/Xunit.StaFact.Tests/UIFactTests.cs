// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using DesktopFactAttribute = Xunit.UIFactAttribute;

public partial class UIFactTests : IDisposable, IAsyncLifetime
{
    private static readonly ConcurrentDictionary<string, int> CultureRetryAttempts = new();
    private readonly SynchronizationContext? ctorSyncContext;
    private readonly int ctorThreadId;

    public UIFactTests()
    {
        this.ctorSyncContext = SynchronizationContext.Current;
        this.ctorThreadId = Environment.CurrentManagedThreadId;
        Assert.NotNull(this.ctorSyncContext);
    }

    public void Dispose()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    public async ValueTask InitializeAsync()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    public async ValueTask DisposeAsync()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [DesktopFact]
    public void CtorAndTestMethodInvokedInSameContext()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [DesktopFact]
    public async Task CtorAndTestMethodInvokedInSameContext_AcrossYields()
    {
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [DesktopFact]
    public async Task PassAfterYield()
    {
        // This will post to the SynchronizationContext before yielding.
        await Task.Yield();
    }

    [DesktopFact]
    public async Task PassAfterDelay()
    {
        // This won't post to the SynchronizationContext till after the delay.
        await Task.Delay(10);
    }

    [DesktopFact(Timeout = 30_000)]
    public async Task Timeout_NotExceeded()
    {
        Assert.Equal(30_000, Assert.IsAssignableFrom<Xunit.v3.IXunitTestCase>(TestContext.Current.TestCase).Timeout);
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
    }

#pragma warning disable xUnit1069 // Deliberately ignore the CancellationToken to simulate a blocked UI thread.
    [DesktopFact(Timeout = 100), Trait("TestCategory", "FailureExpected")]
    public void Timeout_Exceeded_Sync()
    {
        // Deliberately block the UI thread without honoring the CancellationToken.
        Thread.Sleep(2000);
        Assert.Fail("The test should have timed out.");
    }
#pragma warning restore xUnit1069

    [DesktopFact(Timeout = 100), Trait("TestCategory", "FailureExpected")]
    public async Task Timeout_Exceeded_Async()
    {
        await Task.Delay(2000, TestContext.Current.CancellationToken);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterYield()
    {
        await Task.Yield();
        Assert.False(true);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterDelay()
    {
        await Task.Delay(10);
        Assert.False(true);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterYield_Task()
    {
        await Task.Yield();
        Assert.False(true);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterDelay_Task()
    {
        await Task.Delay(10);
        Assert.False(true);
    }

    [DesktopFact]
    public async Task UIFact_OnSingleThreadedSyncContext()
    {
        int initialThread = Environment.CurrentManagedThreadId;
        SynchronizationContext? syncContext = SynchronizationContext.Current;
        await Task.Yield();
        Assert.Equal(initialThread, Environment.CurrentManagedThreadId);
        Assert.Same(syncContext, SynchronizationContext.Current);
    }

    [DesktopFact(Timeout = 30_000)]
    [UISettings(Cultures = new[] { "en-US", "fr-FR" })]
    public async Task ExecutesUnderEachConfiguredCulture()
    {
        TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
        string culture = CultureInfo.CurrentCulture.Name;
        Assert.Equal(culture, CultureInfo.CurrentUICulture.Name);
        Assert.Contains(culture, new[] { "en-US", "fr-FR" });

        await Task.Yield();

        Assert.Equal(culture, CultureInfo.CurrentCulture.Name);
        Assert.Equal(culture, CultureInfo.CurrentUICulture.Name);
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [DesktopFact]
    [UISettings(MaxAttempts = 2, Cultures = new[] { "en-US", "fr-FR" })]
    public void RetriesIndependentlyForEachCulture()
    {
        string culture = CultureInfo.CurrentCulture.Name;
        int attempt = CultureRetryAttempts.AddOrUpdate(culture, 1, (_, value) => value + 1);
        Assert.Equal(2, attempt);
    }

    [DesktopFact]
    public async Task SendBackFromOtherThread()
    {
        SynchronizationContext sc = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext");
        bool delegateComplete = false;
        await Task.Run(delegate
        {
            sc.Send(
                s =>
                {
                    Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
                    Assert.Equal(5, (int)s!);
                },
                5);
            delegateComplete = true;
        });
        Assert.True(delegateComplete);
    }

    [DesktopFact]
    public async Task SendBackFromOtherThread_Throws()
    {
        SynchronizationContext sc = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext");
        await Task.Run(delegate
        {
            Assert.Throws<System.IO.IOException>(() =>
                sc.Send(
                    s =>
                    {
                        throw new System.IO.IOException();
                    },
                    5));
        });
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public void JustFailVoid() => throw new InvalidOperationException("Expected failure.");

    [DesktopFact]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNeeded() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 2);

    [DesktopFact]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNotNeeded() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 1);

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    [UISettings(MaxAttempts = 2)]
    public void FailsAllRetries()
    {
        Assert.Fail("Failure expected.");
    }

    [DesktopFact(SkipExceptions = [typeof(SkipOnThisException)])]
    public void CanSkipOnSpecificExceptions()
    {
        throw new SkipOnThisException();
    }

    [UISettings(MaxAttempts = 2)]
    public class ClassWithDefaultRetryPolicy
    {
        [DesktopFact]
        public void AutomaticRetryNeeded() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 2);

        [DesktopFact, Trait("TestCategory", "FailureExpected")]
        public void FailsAllRetries()
        {
            Assert.Fail("Failure expected.");
        }

        [DesktopFact]
        [UISettings(MaxAttempts = 3)]
        public void SucceedOn3rdAttempt() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 3);
    }
}
