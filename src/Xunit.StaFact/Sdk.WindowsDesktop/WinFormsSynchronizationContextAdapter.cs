﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Xunit.Sdk;

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

    internal override SynchronizationContext Create(string name) => new WindowsFormsSynchronizationContext();

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
