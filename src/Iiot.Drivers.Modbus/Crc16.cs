namespace Iiot.Drivers.Modbus;

/// <summary>
/// Modbus CRC16。
///
/// 参数:多项式 0xA001(0x8005 的位反转)、初值 0xFFFF、无输出异或。
/// **传输时低字节在前**(而数据域是大端)—— 这个不一致是规范的历史遗留,
/// 面试常问,记住即可,推不出来。
///
/// W2 任务:先写逐位版本(理解原理),再写查表版本(理解性能),
/// 两版都要通过 Crc16Tests。
/// </summary>
public static class Crc16
{
    private const ushort Polynomial = 0xA001;

    /// <summary>逐位计算。返回 CRC 的数值(不是传输字节序)。</summary>
    public static ushort Compute(ReadOnlySpan<byte> data)
    {
        // TODO(W2): crc = 0xFFFF;
        //   对每个字节:crc ^= b;
        //     重复 8 次:若最低位为 1 则 crc = (crc >> 1) ^ Polynomial,否则 crc >>= 1;
        _ = Polynomial;
        _ = data;
        throw new NotImplementedException("W2 任务:实现逐位 CRC16");
    }

    /// <summary>查表计算。结果必须与 <see cref="Compute"/> 完全一致。</summary>
    public static ushort ComputeFast(ReadOnlySpan<byte> data)
    {
        // TODO(W2): 预生成 256 项查表,静态构造或 static readonly ushort[]。
        _ = data;
        throw new NotImplementedException("W2 任务:实现查表法 CRC16");
    }

    /// <summary>把 CRC 按 Modbus 的传输顺序(低字节在前)写入目标缓冲。</summary>
    public static void WriteTo(ushort crc, Span<byte> destination)
    {
        destination[0] = (byte)(crc & 0xFF);        // 低字节在前
        destination[1] = (byte)((crc >> 8) & 0xFF);
    }
}
