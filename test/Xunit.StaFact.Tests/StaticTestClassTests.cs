// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using StaFactAttribute = Xunit.StaFactAttribute;
using StaTheoryAttribute = Xunit.StaTheoryAttribute;
using UIFactAttribute = Xunit.UIFactAttribute;
using UITheoryAttribute = Xunit.UITheoryAttribute;

/// <summary>
/// Tests that verify static test classes work correctly with UIFact/UITheory attributes.
/// </summary>
public static class StaticTestClassTests
{
    [UIFactAttribute]
    public static void StaticUIFact_PassingTest()
    {
        Assert.NotNull(SynchronizationContext.Current);
    }

    [UIFactAttribute]
    public static async Task StaticUIFact_AsyncTest()
    {
        Assert.NotNull(SynchronizationContext.Current);
        await Task.Yield();
        Assert.NotNull(SynchronizationContext.Current);
    }

    [UITheoryAttribute]
    [InlineData(1)]
    [InlineData(2)]
    public static void StaticUITheory_PassingTest(int value)
    {
        Assert.NotNull(SynchronizationContext.Current);
        Assert.True(value > 0);
    }

    [UITheoryAttribute]
    [InlineData(10)]
    [InlineData(20)]
    public static async Task StaticUITheory_AsyncTest(int value)
    {
        Assert.NotNull(SynchronizationContext.Current);
        await Task.Yield();
        Assert.NotNull(SynchronizationContext.Current);
        Assert.True(value > 0);
    }

    [StaFactAttribute]
    public static void StaticStaFact_PassingTest()
    {
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
    }

    [StaTheoryAttribute]
    [InlineData(1)]
    [InlineData(2)]
    public static void StaticStaTheory_PassingTest(int value)
    {
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        Assert.True(value > 0);
    }
}

