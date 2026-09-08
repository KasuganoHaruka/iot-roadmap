namespace Iiot.Abstractions;

/// <summary>点位数据类型。刻意保持精简 —— 现场 95% 的点位都在这几种里。</summary>
public enum TagDataType
{
    Bool,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Float32,
    Float64
}

/// <summary>
/// 一个点位的定义。**从配置文件读取,不写死在代码里** ——
/// 现场换一个品牌的仪表只应该改配置,不应该改代码重新发版。
/// </summary>
/// <param name="Name">点位名,在设备内唯一,例如 "Temperature"。</param>
/// <param name="Address">
/// 驱动自解释的地址字符串。
/// Modbus: "4x:0" / "3x:100";S7: "DB1.DBD0";OPC UA: "ns=2;s=Line1.Temp"。
/// </param>
/// <param name="DataType">数据类型。</param>
/// <param name="Scale">线性缩放系数(原始值 * Scale + Offset),默认 1。</param>
/// <param name="Offset">线性偏移,默认 0。</param>
/// <param name="Options">
/// 驱动私有选项。Modbus 用它承载 wordOrder(ABCD/CDAB/BADC/DCBA)。
/// 放在这里而不是提升为一等字段,是为了不让 Modbus 的概念污染通用抽象。
/// </param>
public sealed record TagDefinition(
    string Name,
    string Address,
    TagDataType DataType,
    double Scale = 1.0,
    double Offset = 0.0,
    IReadOnlyDictionary<string, string>? Options = null);
