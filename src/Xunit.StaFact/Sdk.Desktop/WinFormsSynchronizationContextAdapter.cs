// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    internal class WinFormsSynchronizationContextAdapter : SyncContextAdapter
    {
        internal static readonly SyncContextAdapter Default = new WinFormsSynchronizationContextAdapter();

#pragma warning disable SA1310 // Field names should not contain underscore
        private const uint MWMO_INPUTAVAILABLE = 0x0004;
        private const uint QS_ALLINPUT = 0x04FF;
        private const uint WAIT_FAILED = 0xFFFFFFFF;
#pragma warning restore SA1310 // Field names should not contain underscore

        private WinFormsSynchronizationContextAdapter()
        {
        }

        internal override bool CanCompleteOperations => false;

        internal override SynchronizationContext Create(string name) => new WindowsFormsSynchronizationContext();

        internal override Task WaitForOperationCompletionAsync(SynchronizationContext syncContext)
        {
            throw new NotSupportedException("Async void test methods are not supported by the WinForms dispatcher. Use Async Task instead.");
        }

        internal override void PumpTill(SynchronizationContext synchronizationContext, Task task)
        {
            while (!task.IsCompleted)
            {
                Application.DoEvents();
                if (MsgWaitForMultipleObjectsEx(0, IntPtr.Zero, 250, QS_ALLINPUT, MWMO_INPUTAVAILABLE) == WAIT_FAILED)
                {
                    throw new Win32Exception();
                }
            }
        }

        internal override void InitializeThread() => Application.OleRequired();

        [DllImport("user32", SetLastError = true)]
        private static extern uint MsgWaitForMultipleObjectsEx(
            uint nCount,
            IntPtr pHandles,
            uint dwMilliseconds,
            uint dwWakeMask,
            uint dwFlags);
    }
}
