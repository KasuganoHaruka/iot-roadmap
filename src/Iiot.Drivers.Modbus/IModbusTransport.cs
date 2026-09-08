namespace Iiot.Drivers.Modbus;

/// <summary>
/// 传输层抽象。RTU 与 TCP 的区别只在**帧格式和校验**上,
/// 上面的功能码逻辑完全一样,所以在这里切开:
///   RTU:[从站地址][PDU][CRC 低][CRC 高]
///   TCP:[MBAP 7 字节][PDU],无 CRC(TCP 自己保证完整性)
/// </summary>
public interface IModbusTransport : IAsyncDisposable
{
    bool IsOpen { get; }

    Task OpenAsync(CancellationToken ct = default);

    Task CloseAsync(CancellationToken ct = default);

    /// <summary>
    /// 发送一个 PDU(功能码 + 数据),返回响应的 PDU。
    /// 实现方负责:加帧头/校验、分帧、超时、CRC 校验失败重同步。
    /// 设备返回异常码时应抛 <see cref="ModbusException"/>。
    /// </summary>
    Task<byte[]> SendAsync(byte unitId, ReadOnlyMemory<byte> pdu, CancellationToken ct = default);
}

/// <summary>设备返回异常响应(功能码 | 0x80)时抛出。</summary>
public sealed class ModbusException(byte functionCode, byte exceptionCode)
    : Exception($"Modbus 异常响应:功能码 0x{functionCode:X2},异常码 0x{exceptionCode:X2} ({Describe(exceptionCode)})")
{
    public byte FunctionCode { get; } = functionCode;
    public byte ExceptionCode { get; } = exceptionCode;

    private static string Describe(byte code) => code switch
    {
        0x01 => "非法功能码 —— 设备不支持这个功能码,查手册看它到底支持 03 还是 04",
        0x02 => "非法数据地址 —— 地址越界,最常见是 4xxxx 记法没减 1",
        0x03 => "非法数据值 —— 数量超上限,或写入值超范围",
        0x04 => "从站设备故障",
        0x05 => "确认(设备正在处理长耗时请求)",
        0x06 => "从站忙 —— 降低轮询频率再试",
        0x0B => "网关目标设备无响应 —— 网关通了但后面的设备没通",
        _ => "未知异常码"
    };
}
