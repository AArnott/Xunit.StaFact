// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

public class NonSerializableObject
{
    public static object[][] Data => new object[][] { new object[] { new NonSerializableObject() } };

    public int ProcessId { get; } = Process.GetCurrentProcess().Id;

    public int ThreadId { get; } = Environment.CurrentManagedThreadId;
}
