namespace Iiot.Drivers.Modbus;

/// <summary>
/// 32 位数据跨两个寄存器时的字节排列。
///
/// **必须做成点表里的可配置项**,不能写死 —— 现场换一个品牌的仪表
/// 排列方式就可能不同,而这类问题的表现是"读出来是乱码",极难靠猜解决。
///
/// 以 float 25.5f(IEEE754 = 0x41CC0000,字节 A=0x41 B=0xCC C=0x00 D=0x00)为例:
///   ABCD 大端        → 寄存器 [0x41CC, 0x0000]
///   CDAB 字交换(最常见)→ 寄存器 [0x0000, 0x41CC]
///   BADC 字节交换     → 寄存器 [0xCC41, 0x0000]
///   DCBA 全小端       → 寄存器 [0x0000, 0xCC41]
/// </summary>
public enum WordOrder
{
    ABCD,
    CDAB,
    BADC,
    DCBA
}
