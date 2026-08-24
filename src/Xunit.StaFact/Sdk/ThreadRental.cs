// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Xunit.Sdk;

internal class ThreadRental : IDisposable
{
    private readonly TaskCompletionSource<object?> disposalTaskSource;
    private readonly SynchronizationContext syncContext;
    private int disposing;

    private ThreadRental(SyncContextAdapter syncContextAdapter, TaskCompletionSource<object?> disposalTaskSource, SynchronizationContext uiSyncContextSource)
    {
        this.SyncContextAdapter = syncContextAdapter;
        this.disposalTaskSource = disposalTaskSource;
        this.syncContext = uiSyncContextSource;
    }

    internal SyncContextAdapter SyncContextAdapter { get; }

    internal bool IsDisposed => this.disposalTaskSource.Task.IsCompleted;

    internal SynchronizationContext SynchronizationContext
    {
        get
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            return this.syncContext;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref this.disposing, 1) != 0)
        {
            return;
        }

        try
        {
            this.syncContext.Send(_ => this.SyncContextAdapter.Cleanup(), null);
        }
        finally
        {
            this.disposalTaskSource.TrySetResult(null);
        }
    }

    internal static async Task<ThreadRental> CreateAsync(SyncContextAdapter syncContextAdapter, ITestMethod testMethod)
        => await CreateAsync(syncContextAdapter, $"{testMethod.TestClass.TestClassName}.{testMethod.MethodName}");

    internal static async Task<ThreadRental> CreateAsync(SyncContextAdapter syncContextAdapter, string threadName)
    {
        var disposalTaskSource = new TaskCompletionSource<object?>();
        var syncContextSource = new TaskCompletionSource<SynchronizationContext>();
        var thread = new Thread(() =>
        {
            SynchronizationContext uiSyncContext;
            try
            {
                uiSyncContext = syncContextAdapter.Create(threadName);
                if (syncContextAdapter.ShouldSetAsCurrent)
                {
                    SynchronizationContext.SetSynchronizationContext(uiSyncContext);
                }

                syncContextAdapter.InitializeThread();
            }
            catch (Exception ex)
            {
                syncContextSource.TrySetException(ex);
                return;
            }

            syncContextSource.SetResult(uiSyncContext);
            syncContextAdapter.PumpTill(uiSyncContext, disposalTaskSource.Task);
        });

        thread.Name = threadName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            thread.SetApartmentState(ApartmentState.STA);
        }

        thread.Start();

        SynchronizationContext syncContext = await syncContextSource.Task.ConfigureAwait(false);

        var rental = new ThreadRental(
            syncContextAdapter,
            disposalTaskSource,
            syncContext);
        return rental;
    }
}
