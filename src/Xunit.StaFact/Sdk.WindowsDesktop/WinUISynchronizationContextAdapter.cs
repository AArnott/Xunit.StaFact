// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

using System.Windows.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Xunit.Sdk;

internal class WinUISynchronizationContextAdapter : SyncContextAdapter
{
    internal static readonly SyncContextAdapter Default = new WinUISynchronizationContextAdapter();

    private WinUISynchronizationContextAdapter()
    {
    }

    internal override bool CanCompleteOperations => false;

    internal override SynchronizationContext Create(string name) => new DispatcherQueueSynchronizationContext(dispatcherQueue: new TestWindow().DispatcherQueue);

    internal override Task WaitForOperationCompletionAsync(SynchronizationContext syncContext)
    {
        throw new NotSupportedException("Async void test methods are not supported by the WinUI dispatcher. Use Async Task instead.");
    }

    internal override void PumpTill(SynchronizationContext synchronizationContext, Task task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(_ => frame.Continue = false, TaskScheduler.Default);
            Dispatcher.PushFrame(frame);
        }
    }

    internal override void Cleanup()
    {
        Dispatcher.CurrentDispatcher.InvokeShutdown();
        base.Cleanup();
    }

    private class TestWindow : Window
    {
    }
}

#endif
