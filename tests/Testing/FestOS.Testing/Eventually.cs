using Xunit;

namespace FestOS.Testing;

/// <summary>
/// Waits for work that finishes in the background, such as event delivery, by checking a condition
/// until it holds or a time limit passes (testing §6). Never a fixed sleep.
/// </summary>
public static class Eventually
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>Checks the condition until it is true; fails the test after <paramref name="timeout"/>.</summary>
    public static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout, string because)
    {
        ArgumentNullException.ThrowIfNull(condition);
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        limit.CancelAfter(timeout);

        while (!await condition())
        {
            try
            {
                await Task.Delay(CheckInterval, limit.Token);
            }
            catch (OperationCanceledException) when (!TestContext.Current.CancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Waited {timeout} for: {because}");
            }
        }
    }
}
