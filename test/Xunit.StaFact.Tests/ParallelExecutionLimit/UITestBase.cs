// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

public class UITestBase
{
    protected static void TestMethod([CallerMemberName] string? name = null)
    {
        try
        {
            Thread.Sleep(200);
            Counter.Increment();
            Thread.Sleep(200);
        }
        finally
        {
            Counter.Decrement();
        }
    }
}
