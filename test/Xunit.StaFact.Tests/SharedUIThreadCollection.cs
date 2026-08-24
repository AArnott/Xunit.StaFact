// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

[CollectionDefinition(nameof(SharedUIThreadCollection))]
public class SharedUIThreadCollection : ICollectionFixture<SharedUIThreadCollection.CollectionUIThreadFixture>
{
    public class CollectionUIThreadFixture : UIThreadFixture
    {
        public SynchronizationContext? Context { get; private set; }

        public int ThreadId { get; private set; }

        protected override ValueTask InitializeOnUIThreadAsync()
        {
            this.Context = SynchronizationContext.Current;
            this.ThreadId = Environment.CurrentManagedThreadId;
            return default;
        }
    }
}
