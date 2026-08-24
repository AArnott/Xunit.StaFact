// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

[Collection(nameof(DirectUIThreadCollection))]
public class DirectUIThreadCollectionTests2
{
    public DirectUIThreadCollectionTests2(UIThreadFixture fixture)
    {
    }

    [UIFact]
    public void UsesSameCollectionThread()
    {
        DirectUIThreadCollection.AssertSharedThread();
    }
}
