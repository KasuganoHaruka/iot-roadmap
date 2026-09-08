using Iiot.Abstractions;

namespace Iiot.Drivers.Modbus;

/// <summary>
/// 寄存器数组 ↔ 强类型值的转换。字序问题全部收敛在这里。
///
/// W3 任务:实现下面两个方法,通过 ValueCodecTests。
/// </summary>
public static class ValueCodec
{
    /// <summary>
    /// 从寄存器数组解码出一个值。
    /// </summary>
    /// <param name="registers">按 Modbus 响应顺序排列的寄存器(每个寄存器本身是大端 16 位)。</param>
    /// <param name="dataType">目标类型。</param>
    /// <param name="wordOrder">32/64 位类型的字序;16 位类型忽略此参数。</param>
    public static object Decode(
        ReadOnlySpan<ushort> registers,
        TagDataType dataType,
        WordOrder wordOrder)
    {
        // TODO(W3):
        //   1) 把寄存器摊平成字节数组(每个寄存器高字节在前)
        //   2) 按 wordOrder 重排成 ABCD 顺序
        //   3) BinaryPrimitives.ReadSingleBigEndian / ReadInt32BigEndian 等解出值
        _ = registers; _ = dataType; _ = wordOrder;
        throw new NotImplementedException("W3 任务:实现 Decode");
    }

    /// <summary>把值编码成寄存器数组,用于 FC06 / FC10 写入。</summary>
    public static ushort[] Encode(object value, TagDataType dataType, WordOrder wordOrder)
    {
        // TODO(W3): Decode 的逆过程。写测试时用 Decode(Encode(x)) == x 做往返验证。
        _ = value; _ = dataType; _ = wordOrder;
        throw new NotImplementedException("W3 任务:实现 Encode");
    }

    /// <summary>该类型占几个寄存器。</summary>
    public static int RegisterCount(TagDataType dataType) => dataType switch
    {
        TagDataType.Bool or TagDataType.Int16 or TagDataType.UInt16 => 1,
        TagDataType.Int32 or TagDataType.UInt32 or TagDataType.Float32 => 2,
        TagDataType.Float64 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(dataType))
    };
}
