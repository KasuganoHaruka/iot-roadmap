using Iiot.Drivers.Modbus;
using Xunit;

namespace Iiot.Drivers.Modbus.Tests;

/// <summary>
/// 这些测试是**先给你的**,W2 的任务是让它们变绿。
/// 校验向量都经过验证,如果你的实现算出别的值,是实现错了,不是测试错了。
/// </summary>
public class Crc16Tests
{
    /// <summary>
    /// CRC-16/MODBUS 的标准校验值:对 ASCII "123456789" 计算结果为 0x4B37。
    /// 这是所有 CRC 实现的通用自检向量。
    /// </summary>
    [Fact]
    public void StandardCheckVector()
    {
        var data = "123456789"u8.ToArray();
        Assert.Equal(0x4B37, Crc16.Compute(data));
    }

    /// <summary>
    /// 真实报文:从站 1,FC03,起始地址 0,读 10 个寄存器。
    /// 完整帧为 01 03 00 00 00 0A C5 CD —— 注意 CRC 值是 0xCDC5,
    /// **传输时低字节 0xC5 在前**。
    /// </summary>
    [Fact]
    public void RealFc03Request()
    {
        byte[] frame = [0x01, 0x03, 0x00, 0x00, 0x00, 0x0A];
        Assert.Equal(0xCDC5, Crc16.Compute(frame));
    }

    [Fact]
    public void WriteTo_PutsLowByteFirst()
    {
        Span<byte> buffer = stackalloc byte[2];
        Crc16.WriteTo(0xCDC5, buffer);
        Assert.Equal(0xC5, buffer[0]);
        Assert.Equal(0xCD, buffer[1]);
    }

    /// <summary>查表法必须和逐位法结果完全一致。</summary>
    [Fact]
    public void TableAndBitwiseAgree()
    {
        var random = new Random(42);
        for (var i = 0; i < 200; i++)
        {
            var data = new byte[random.Next(1, 64)];
            random.NextBytes(data);
            Assert.Equal(Crc16.Compute(data), Crc16.ComputeFast(data));
        }
    }
}
