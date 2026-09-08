using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Iiot.EdgeGateway.Buffering;

/// <summary>
/// SQLite 实现。W7 任务。
///
/// 建表建议:
///   CREATE TABLE IF NOT EXISTS measurements (
///     seq        INTEGER PRIMARY KEY AUTOINCREMENT,
///     device_id  TEXT    NOT NULL,
///     tag        TEXT    NOT NULL,
///     value      TEXT,              -- 用文本存,避免类型丢失
///     quality    INTEGER NOT NULL,
///     ts_utc     INTEGER NOT NULL,  -- Unix 毫秒
///     sent       INTEGER NOT NULL DEFAULT 0
///   );
///   CREATE INDEX IF NOT EXISTS ix_pending ON measurements(sent, seq);
///
/// 性能要点(树莓派的 SD 卡很慢,这几条能差一个数量级):
///   PRAGMA journal_mode = WAL;      -- 读写不互相阻塞
///   PRAGMA synchronous  = NORMAL;   -- FULL 太慢,NORMAL 在 WAL 下足够安全
///   批量写用**单事务**,不要一条一个事务
/// </summary>
public sealed class SqliteMeasurementBuffer(IOptions<GatewayOptions> options) : IMeasurementBuffer, IAsyncDisposable
{
    private readonly BufferOptions _options = options.Value.Buffer;

    public Task AppendAsync(IReadOnlyList<BufferedMeasurement> items, CancellationToken ct = default)
    {
        // TODO(W7): 单事务批量 INSERT
        _ = _options; _ = items; _ = ct;
        throw new NotImplementedException("W7 任务:实现 AppendAsync");
    }

    public Task<IReadOnlyList<BufferedMeasurement>> TakePendingAsync(int max, CancellationToken ct = default)
    {
        // TODO(W7): SELECT ... WHERE sent = 0 ORDER BY seq LIMIT @max
        _ = max; _ = ct;
        throw new NotImplementedException("W7 任务:实现 TakePendingAsync");
    }

    public Task MarkSentAsync(long upToSequence, CancellationToken ct = default)
    {
        // TODO(W7): UPDATE measurements SET sent = 1 WHERE seq <= @upTo
        _ = upToSequence; _ = ct;
        throw new NotImplementedException("W7 任务:实现 MarkSentAsync");
    }

    public Task<int> PruneAsync(CancellationToken ct = default)
    {
        // TODO(W7): 删除 sent=1 且超过 RetainSentFor 的行;
        //   总行数超过 MaxRows 时,再删最旧的未发送行(并记 WARN 日志)。
        _ = ct;
        throw new NotImplementedException("W7 任务:实现 PruneAsync");
    }

    public Task<long> GetPendingCountAsync(CancellationToken ct = default)
    {
        _ = ct;
        throw new NotImplementedException("W7 任务:实现 GetPendingCountAsync");
    }

    public ValueTask DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        return ValueTask.CompletedTask;
    }
}
