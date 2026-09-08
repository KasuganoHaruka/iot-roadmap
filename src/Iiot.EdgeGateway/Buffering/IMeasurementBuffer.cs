using Iiot.Abstractions;

namespace Iiot.EdgeGateway.Buffering;

/// <summary>缓冲里的一条记录,带序号用于保证顺序和去重。</summary>
/// <param name="Sequence">本网关内单调递增的序号 —— 下游用它做幂等去重。</param>
public sealed record BufferedMeasurement(long Sequence, string DeviceId, TagValue Value);

/// <summary>
/// 持久化缓冲。**断网续传的核心** ——
/// 采集写进来,上报读出去,两者互不阻塞。
///
/// 面试常问"你怎么保证不丢数据":
/// 答案的边界是「事务提交之后不丢」。采到但还没提交时断电会丢一个采集周期,
/// 要消除就得每条同步落盘,代价是吞吐掉一个数量级。工业场景一般接受这个取舍——
/// **主动说出边界比声称"绝对不丢"可信得多**。
/// </summary>
public interface IMeasurementBuffer
{
    /// <summary>批量写入(单事务)。</summary>
    Task AppendAsync(IReadOnlyList<BufferedMeasurement> items, CancellationToken ct = default);

    /// <summary>按序号取未发送的数据。</summary>
    Task<IReadOnlyList<BufferedMeasurement>> TakePendingAsync(int max, CancellationToken ct = default);

    /// <summary>确认已发送(收到 PUBACK 之后才调用)。</summary>
    Task MarkSentAsync(long upToSequence, CancellationToken ct = default);

    /// <summary>清理已发送的旧数据,并在超过 MaxRows 时丢弃最旧的未发送数据。</summary>
    Task<int> PruneAsync(CancellationToken ct = default);

    /// <summary>积压条数。这是最重要的健康指标 —— 持续上涨就说明上报跟不上采集。</summary>
    Task<long> GetPendingCountAsync(CancellationToken ct = default);
}
