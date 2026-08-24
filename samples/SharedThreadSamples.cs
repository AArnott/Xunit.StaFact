// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

/// <summary>
/// Demonstrates sharing one UI thread across several tests.
/// </summary>
public static class SharedThreadSamples
{
    #region ClassSharedThread
    /// <summary>
    /// Every UI fact or theory in this class uses one shared thread.
    /// </summary>
    public class ClassScopedTests : IClassFixture<UIThreadFixture>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassScopedTests"/> class.
        /// </summary>
        /// <param name="fixture">The shared thread fixture.</param>
        public ClassScopedTests(UIThreadFixture fixture)
        {
        }

        /// <summary>
        /// Runs on the shared thread.
        /// </summary>
        [UIFact]
        public void FirstTest()
        {
            Assert.NotNull(SynchronizationContext.Current);
        }

        /// <summary>
        /// Runs on the same thread as <see cref="FirstTest"/>.
        /// </summary>
        [UIFact]
        public void SecondTest()
        {
            Assert.NotNull(SynchronizationContext.Current);
        }
    }
    #endregion

    #region CollectionSharedThread
    /// <summary>
    /// Defines a collection whose test classes share one UI thread.
    /// </summary>
    [CollectionDefinition(nameof(SharedUIThreadCollection))]
    public class SharedUIThreadCollection : ICollectionFixture<UIThreadFixture>
    {
    }

    /// <summary>
    /// Tests in any class in this collection can use the same fixture thread.
    /// </summary>
    [Collection(nameof(SharedUIThreadCollection))]
    public class CollectionScopedTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionScopedTests"/> class.
        /// </summary>
        /// <param name="fixture">The collection's shared thread fixture.</param>
        public CollectionScopedTests(UIThreadFixture fixture)
        {
        }

        /// <summary>
        /// Runs on the collection fixture thread.
        /// </summary>
        [UIFact]
        public void UsesCollectionThread()
        {
            Assert.NotNull(SynchronizationContext.Current);
        }
    }
    #endregion

    #region SharedThreadFixture
    /// <summary>
    /// Owns thread-affine state and initializes and disposes it on the shared UI thread.
    /// </summary>
    public class ThreadAffineFixture : UIThreadFixture
    {
        /// <summary>
        /// Gets the thread that owns the fixture state.
        /// </summary>
        public int ThreadId { get; private set; }

        /// <inheritdoc/>
        protected override ValueTask InitializeOnUIThreadAsync()
        {
            this.ThreadId = Environment.CurrentManagedThreadId;
            return default;
        }

        /// <inheritdoc/>
        protected override ValueTask DisposeOnUIThreadAsync()
        {
            Assert.Equal(this.ThreadId, Environment.CurrentManagedThreadId);
            return default;
        }
    }
    #endregion
}
