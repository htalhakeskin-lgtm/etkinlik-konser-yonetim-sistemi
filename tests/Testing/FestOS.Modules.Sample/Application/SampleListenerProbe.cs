namespace FestOS.Modules.Sample.Application;

/// <summary>Counts calls to <see cref="NotifyOnSampleItemUsedHandler"/> and makes it fail on demand.</summary>
public sealed class SampleListenerProbe
{
    private int _calls;
    private int _failuresRemaining;

    /// <summary>How many times the listener ran.</summary>
    public int Calls => Volatile.Read(ref _calls);

    /// <summary>How many of the next runs throw.</summary>
    public int FailuresRemaining
    {
        get => Volatile.Read(ref _failuresRemaining);
        set => Volatile.Write(ref _failuresRemaining, value);
    }

    /// <summary>Forgets calls and planned failures.</summary>
    public void Reset()
    {
        Volatile.Write(ref _calls, 0);
        Volatile.Write(ref _failuresRemaining, 0);
    }

    internal void Run()
    {
        Interlocked.Increment(ref _calls);
        if (Interlocked.Decrement(ref _failuresRemaining) >= 0)
        {
            throw new InvalidOperationException("Sample listener failure.");
        }

        Interlocked.Exchange(ref _failuresRemaining, 0);
    }
}
