using System.Diagnostics;
using Iiot.Abstractions;

namespace Iiot.Drivers.Modbus;

public sealed class ModbusDriverOptions
{
    public required string DeviceId { get; init; }

    /// <summary>485 从站地址;Modbus TCP 里是 Unit ID(网关后挂多台时有意义)。</summary>
    public byte UnitId { get; init; } = 1;

    /// <summary>
    /// 单次请求超时。**必须每设备可配** —— 老电表、无线透传可能要 1–2 秒,
    /// 一刀切会造成误判掉线和重试风暴。
    /// 下限参考:报文字节数 × 10 bit ÷ 波特率 + 设备处理时间。
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMilliseconds(300);

    public int MaxRetries { get; init; } = 2;

    /// <summary>批量读合并时允许跨越的最大空洞。</summary>
    public int MaxGap { get; init; } = 8;

    public WordOrder DefaultWordOrder { get; init; } = WordOrder.CDAB;
}

/// <summary>
/// Modbus 驱动。**先自己写,不要用 NModbus** ——
/// 这一层是你和"只会调库的人"的分水岭,面试会往字节级问。
/// </summary>
public sealed class ModbusDriver(IModbusTransport transport, ModbusDriverOptions options)
    : IDeviceDriver
{
    public string DeviceId => options.DeviceId;

    public string Protocol => "modbus";

    public bool IsConnected => transport.IsOpen;

    public DriverStatistics Statistics { get; } = new();

    public Task ConnectAsync(CancellationToken ct = default) => transport.OpenAsync(ct);

    public Task DisconnectAsync(CancellationToken ct = default) => transport.CloseAsync(ct);

    public async Task<IReadOnlyList<TagValue>> ReadAsync(
        IReadOnlyList<TagDefinition> tags,
        CancellationToken ct = default)
    {
        var results = new List<TagValue>(tags.Count);
        var plan = ReadPlanner.Plan(tags, options.MaxGap);

        foreach (var request in plan)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var responsePdu = await transport.SendAsync(options.UnitId, BuildReadPdu(request), ct);

                // TODO(W3): 拆响应 → 按每个 tag 在请求内的偏移和类型调 ValueCodec.Decode
                //   → 应用 Scale/Offset → 产出 Good 值加入 results。
                _ = responsePdu;
                throw new NotImplementedException("W3 任务:解析响应并生成 TagValue");
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                // 超时。**部分失败不整体抛异常** —— 其他请求的数据仍然有效,
                // 失败的点位返回 Bad,让上层决定怎么处理。
                Statistics.RecordFailure(timedOut: true);
                results.AddRange(request.Tags.Select(t => TagValue.Bad(t.Name, DateTimeOffset.UtcNow)));
            }
            catch (ModbusException)
            {
                Statistics.RecordFailure(timedOut: false);
                results.AddRange(request.Tags.Select(t => TagValue.Bad(t.Name, DateTimeOffset.UtcNow)));
            }
            finally
            {
                if (Statistics.ConsecutiveFailures == 0) Statistics.RecordSuccess(sw.ElapsedMilliseconds);
            }
        }

        return results;
    }

    /// <summary>
    /// 组装读请求的 PDU:[功能码][起始地址 高][起始地址 低][数量 高][数量 低]。
    /// 注意 PDU 里的地址和数量都是**大端**(和 CRC 的小端相反)。
    /// 功能码按存储区选:HoldingRegister→0x03,InputRegister→0x04,
    /// Coil→0x01,DiscreteInput→0x02。
    /// </summary>
    private static ReadOnlyMemory<byte> BuildReadPdu(ReadRequest request)
    {
        // TODO(W3): 实现 PDU 组装
        _ = request;
        throw new NotImplementedException("W3 任务:实现 BuildReadPdu");
    }

    public Task WriteAsync(TagDefinition tag, object value, CancellationToken ct = default)
    {
        // TODO(W3): 单寄存器用 FC06,多寄存器用 FC10。写完建议回读校验。
        _ = tag; _ = value; _ = ct;
        throw new NotImplementedException("W3 任务:实现 WriteAsync");
    }

    public ValueTask DisposeAsync() => transport.DisposeAsync();
}
