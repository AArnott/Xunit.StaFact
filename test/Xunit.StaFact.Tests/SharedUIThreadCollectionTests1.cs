// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

[Collection(nameof(SharedUIThreadCollection))]
public class SharedUIThreadCollectionTests1
{
    private readonly SharedUIThreadCollection.CollectionUIThreadFixture fixture;

    public SharedUIThreadCollectionTests1(SharedUIThreadCollection.CollectionUIThreadFixture fixture)
    {
        this.fixture = fixture;
    }

    [UIFact]
    public void UsesCollectionFixtureThread()
    {
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.fixture.Context, SynchronizationContext.Current);
    }
}
