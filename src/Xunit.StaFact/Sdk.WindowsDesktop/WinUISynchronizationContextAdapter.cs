// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Hosting;

namespace Xunit.Sdk;

internal class WinUISynchronizationContextAdapter : SyncContextAdapter
{
    internal static readonly SyncContextAdapter Default = new WinUISynchronizationContextAdapter();

    private static readonly SemaphoreSlim XamlManagerGate = new(1, 1);

    [ThreadStatic]
    private static DispatcherQueueController? dispatcherQueueController;

    [ThreadStatic]
    private static bool ownsXamlManagerGate;

    [ThreadStatic]
    private static WindowsXamlManager? windowsXamlManager;

    private WinUISynchronizationContextAdapter()
    {
    }

    internal override SynchronizationContext Create(string name)
    {
        XamlManagerGate.Wait();
        ownsXamlManagerGate = true;
        try
        {
            dispatcherQueueController = DispatcherQueueController.CreateOnCurrentThread();
            windowsXamlManager = WindowsXamlManager.InitializeForCurrentThread();
            return new DispatcherQueueSynchronizationContext(dispatcherQueueController.DispatcherQueue);
        }
        catch
        {
            windowsXamlManager?.Dispose();
            windowsXamlManager = null;
            dispatcherQueueController?.ShutdownQueue();
            dispatcherQueueController = null;
            ownsXamlManagerGate = false;
            XamlManagerGate.Release();
            throw;
        }
    }

    internal override void PumpTill(SynchronizationContext synchronizationContext, Task task)
    {
        DispatcherQueueController controller = dispatcherQueueController
            ?? throw new InvalidOperationException("The WinUI dispatcher queue has not been initialized.");

        _ = task.ContinueWith(
            _ => controller.DispatcherQueue.EnqueueEventLoopExit(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
        try
        {
            controller.DispatcherQueue.RunEventLoop();
        }
        finally
        {
            controller.ShutdownQueue();
            dispatcherQueueController = null;
            if (ownsXamlManagerGate)
            {
                ownsXamlManagerGate = false;
                XamlManagerGate.Release();
            }
        }
    }

    internal override void Cleanup(SynchronizationContext synchronizationContext)
    {
        if (SynchronizationContext.Current == synchronizationContext)
        {
            this.Cleanup();
            return;
        }

        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        synchronizationContext.Post(
            _ =>
            {
                try
                {
                    this.Cleanup();
                    completion.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            },
            null);
        completion.Task.GetAwaiter().GetResult();
    }

    internal override void Cleanup()
    {
        windowsXamlManager?.Dispose();
        windowsXamlManager = null;
    }
}

#endif
