// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit;

public record struct UISettingsKey(int MaxAttempts)
{
    public static UISettingsKey Default { get; } = new(MaxAttempts: 1);
}
