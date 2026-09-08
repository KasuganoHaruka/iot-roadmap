namespace Iiot.Abstractions;

/// <summary>
/// 通信质量统计。**面试加分项**:能一眼看出哪台设备总丢包,
/// 而不是等客户打电话来说"数据不对"。
/// </summary>
public sealed class DriverStatistics
{
    private long _requests;
    private long _failures;
    private long _timeouts;
    private long _totalLatencyMs;
    private long _maxLatencyMs;

    public long Requests => Interlocked.Read(ref _requests);
    public long Failures => Interlocked.Read(ref _failures);
    public long Timeouts => Interlocked.Read(ref _timeouts);
    public long ConsecutiveFailures { get; private set; }

    public double SuccessRate =>
        Requests == 0 ? 1.0 : 1.0 - (double)Failures / Requests;

    public double AverageLatencyMs =>
        Requests == 0 ? 0 : (double)Interlocked.Read(ref _totalLatencyMs) / Requests;

    public long MaxLatencyMs => Interlocked.Read(ref _maxLatencyMs);

    public void RecordSuccess(long latencyMs)
    {
        Interlocked.Increment(ref _requests);
        Interlocked.Add(ref _totalLatencyMs, latencyMs);
        InterlockedMax(ref _maxLatencyMs, latencyMs);
        ConsecutiveFailures = 0;
    }

    public void RecordFailure(bool timedOut)
    {
        Interlocked.Increment(ref _requests);
        Interlocked.Increment(ref _failures);
        if (timedOut) Interlocked.Increment(ref _timeouts);
        ConsecutiveFailures++;
    }

    private static void InterlockedMax(ref long target, long value)
    {
        long current;
        do
        {
            current = Interlocked.Read(ref target);
            if (value <= current) return;
        }
        while (Interlocked.CompareExchange(ref target, value, current) != current);
    }
}
