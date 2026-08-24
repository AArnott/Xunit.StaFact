// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a shared UI thread for tests in an xUnit class or collection fixture.
/// </summary>
/// <remarks>
/// Derive from one of the concrete fixture types rather than deriving from this type directly.
/// </remarks>
public abstract class UIThreadFixtureBase : IAsyncLifetime
{
    private readonly string threadName;
    private ThreadRental? threadRental;

    private protected UIThreadFixtureBase(UITestCase.SyncContextType synchronizationContextType)
    {
        this.SyncContextAdapter = UITestCase.GetAdapter(synchronizationContextType);
        this.threadName = this.GetType().FullName ?? this.GetType().Name;
    }

    internal SyncContextAdapter SyncContextAdapter { get; }

    internal ThreadRental ThreadRental => this.threadRental ?? throw new ObjectDisposedException(this.GetType().FullName);

    /// <summary>
    /// Gets the synchronization context for the shared UI thread.
    /// </summary>
    protected SynchronizationContext SynchronizationContext => this.ThreadRental.SynchronizationContext;

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        if (this.threadRental is not null)
        {
            throw new InvalidOperationException("The shared UI thread fixture has already been initialized.");
        }

        ThreadRental rental = await ThreadRental.CreateAsync(this.SyncContextAdapter, this.threadName);
        if (Interlocked.CompareExchange(ref this.threadRental, rental, null) is not null)
        {
            rental.Dispose();
            throw new InvalidOperationException("The shared UI thread fixture has already been initialized.");
        }

        try
        {
            await RunOnUIThreadAsync(rental, this.InitializeOnUIThreadAsync);
        }
        catch
        {
            this.threadRental = null;
            rental.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        ThreadRental? rental = Interlocked.Exchange(ref this.threadRental, null);
        if (rental is null)
        {
            return;
        }

        try
        {
            await RunOnUIThreadAsync(rental, this.DisposeOnUIThreadAsync);
        }
        finally
        {
            rental.Dispose();
        }
    }

    /// <summary>
    /// Initializes the fixture on its shared UI thread.
    /// </summary>
    /// <remarks>
    /// <see cref="StaThreadFixture"/> does not install a synchronization context, so code after a yielding
    /// <see langword="await"/> may resume on a thread-pool thread.
    /// </remarks>
    /// <returns>A task that completes when initialization has finished.</returns>
    protected virtual ValueTask InitializeOnUIThreadAsync() => default;

    /// <summary>
    /// Disposes the fixture on its shared UI thread.
    /// </summary>
    /// <remarks>
    /// <see cref="StaThreadFixture"/> does not install a synchronization context, so code after a yielding
    /// <see langword="await"/> may resume on a thread-pool thread.
    /// </remarks>
    /// <returns>A task that completes when disposal has finished.</returns>
    protected virtual ValueTask DisposeOnUIThreadAsync() => default;

    /// <summary>
    /// Runs a callback on the shared UI thread.
    /// </summary>
    /// <param name="callback">The callback to run.</param>
    /// <returns>A task that completes when the callback has finished.</returns>
    protected ValueTask RunOnUIThreadAsync(Func<ValueTask> callback)
    {
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return RunOnUIThreadAsync(this.ThreadRental, callback);
    }

    private static async ValueTask RunOnUIThreadAsync(ThreadRental rental, Func<ValueTask> callback)
    {
        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        rental.SynchronizationContext.Post(
            async _ =>
            {
                try
                {
                    await callback();
                    completion.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            },
            null);
        await completion.Task.ConfigureAwait(false);
    }
}
