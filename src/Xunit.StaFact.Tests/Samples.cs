// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public partial class Samples
{
    [UIFact]
    public async Task UIFact_OnSTAThread()
    {
        int initialThread = Environment.CurrentManagedThreadId;
        await Task.Yield();
        Assert.Equal(initialThread, Environment.CurrentManagedThreadId);
    }
}
