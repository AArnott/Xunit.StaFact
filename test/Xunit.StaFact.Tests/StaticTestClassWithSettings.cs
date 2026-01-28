// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using UIFactAttribute = Xunit.UIFactAttribute;

/// <summary>
/// Tests that verify static test classes work with UISettings attribute.
/// </summary>
[UISettings(MaxAttempts = 2)]
public static class StaticTestClassWithSettings
{
    [UIFactAttribute]
    public static void StaticUIFact_WithClassSettings()
    {
        Assert.NotNull(SynchronizationContext.Current);
    }

    [UIFactAttribute]
    [UISettings(MaxAttempts = 3)]
    public static void StaticUIFact_WithMethodSettings()
    {
        Assert.NotNull(SynchronizationContext.Current);
    }
}
