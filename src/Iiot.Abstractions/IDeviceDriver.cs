namespace Iiot.Abstractions;

/// <summary>
/// 统一设备驱动抽象。Modbus / S7 / OPC UA 三个实现都落在这个接口下,
/// 上层的采集调度、缓冲、上报完全不知道下面接的是什么协议。
///
/// 这是整套系统最重要的一条接缝 —— 面试时被问"你的架构好在哪",
/// 答案就是这里:**加一种协议不需要改采集层和平台层**。
/// </summary>
public interface IDeviceDriver : IAsyncDisposable
{
    /// <summary>设备唯一标识,用于 MQTT 主题与时序库子表命名。</summary>
    string DeviceId { get; }

    /// <summary>协议名,用于诊断展示("modbus-rtu" / "s7" / "opcua")。</summary>
    string Protocol { get; }

    bool IsConnected { get; }

    DriverStatistics Statistics { get; }

    Task ConnectAsync(CancellationToken ct = default);

    Task DisconnectAsync(CancellationToken ct = default);

    /// <summary>
    /// 批量读取。**必须是批量接口**,因为几乎所有工业协议的
    /// 单次往返开销都远大于多读几个字节的开销 —— 一次读 20 个点位
    /// 和读 1 个点位的耗时几乎一样。逐点读会让轮询周期慢一个数量级。
    ///
    /// 实现方应在内部把离散地址合并成最少的请求(见 ReadPlanner)。
    /// 部分点位失败时,返回对应的 Bad 值,而不是整体抛异常。
    /// </summary>
    Task<IReadOnlyList<TagValue>> ReadAsync(
        IReadOnlyList<TagDefinition> tags,
        CancellationToken ct = default);

    /// <summary>写单个点位。写操作一律单点 + 显式确认,不做批量。</summary>
    Task WriteAsync(TagDefinition tag, object value, CancellationToken ct = default);
}
