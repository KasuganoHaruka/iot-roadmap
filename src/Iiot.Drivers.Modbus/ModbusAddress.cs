namespace Iiot.Drivers.Modbus;

/// <summary>Modbus 的四个存储区。</summary>
public enum ModbusArea
{
    /// <summary>0x 线圈,读写位。FC01 / 05 / 0F。</summary>
    Coil,

    /// <summary>1x 离散输入,只读位。FC02。</summary>
    DiscreteInput,

    /// <summary>3x 输入寄存器,只读字。FC04。</summary>
    InputRegister,

    /// <summary>4x 保持寄存器,读写字。FC03 / 06 / 10。</summary>
    HoldingRegister
}

/// <summary>
/// 解析点表里的地址字符串,例如 "4x:100"、"3x:0"、"0x:8"。
///
/// ⚠️ **协议地址是 0-based**,而传统文档的 4xxxx 记法是 1-based。
/// 文档写 40001 → 协议地址 0;文档写 40100 → 协议地址 99。
/// "地址老是差 1" 这个经典问题就出在这里,所以点表里统一用协议地址,
/// 并在文档里写清楚这个约定。
/// </summary>
public readonly record struct ModbusAddress(ModbusArea Area, ushort Offset)
{
    public static ModbusAddress Parse(string address)
    {
        // TODO(W3): 解析 "4x:100" 形式;非法输入抛 FormatException 并给出清晰消息。
        _ = address;
        throw new NotImplementedException("W3 任务:实现地址解析");
    }
}
