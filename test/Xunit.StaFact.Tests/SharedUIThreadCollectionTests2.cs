// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

[Collection(nameof(SharedUIThreadCollection))]
public class SharedUIThreadCollectionTests2
{
    private readonly SharedUIThreadCollection.CollectionUIThreadFixture fixture;

    public SharedUIThreadCollectionTests2(SharedUIThreadCollection.CollectionUIThreadFixture fixture)
    {
        this.fixture = fixture;
    }

    [UIFact]
    public void UsesSameCollectionFixtureThread()
    {
        Assert.Equal(this.fixture.ThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.fixture.Context, SynchronizationContext.Current);
    }
}
