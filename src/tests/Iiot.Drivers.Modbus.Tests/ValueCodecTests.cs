using Iiot.Abstractions;
using Iiot.Drivers.Modbus;
using Xunit;

namespace Iiot.Drivers.Modbus.Tests;

/// <summary>
/// 字序测试。25.5f 的 IEEE754 表示是 0x41CC0000,
/// 四个字节 A=0x41 B=0xCC C=0x00 D=0x00。
///
/// 现场排查"浮点数读出来是乱码"时用的就是这个办法:
/// 让设备输出一个已知值,把四种排列都算一遍,对上的那个就是设备的字序。
/// </summary>
public class ValueCodecTests
{
    [Theory]
    [InlineData(WordOrder.ABCD, 0x41CC, 0x0000)]
    [InlineData(WordOrder.CDAB, 0x0000, 0x41CC)]
    [InlineData(WordOrder.BADC, 0xCC41, 0x0000)]
    [InlineData(WordOrder.DCBA, 0x0000, 0xCC41)]
    public void Decode_Float32_AllWordOrders(WordOrder order, ushort reg0, ushort reg1)
    {
        ushort[] registers = [reg0, reg1];
        var value = ValueCodec.Decode(registers, TagDataType.Float32, order);
        Assert.Equal(25.5f, Assert.IsType<float>(value));
    }

    [Fact]
    public void Decode_UInt16_IgnoresWordOrder()
    {
        ushort[] registers = [0x1234];
        foreach (var order in Enum.GetValues<WordOrder>())
        {
            var value = ValueCodec.Decode(registers, TagDataType.UInt16, order);
            Assert.Equal((ushort)0x1234, Assert.IsType<ushort>(value));
        }
    }

    [Fact]
    public void Decode_Int32_Negative()
    {
        // -2 = 0xFFFFFFFE,ABCD 排列
        ushort[] registers = [0xFFFF, 0xFFFE];
        var value = ValueCodec.Decode(registers, TagDataType.Int32, WordOrder.ABCD);
        Assert.Equal(-2, Assert.IsType<int>(value));
    }

    /// <summary>往返验证:编码再解码必须回到原值。四种字序都要过。</summary>
    [Theory]
    [InlineData(WordOrder.ABCD)]
    [InlineData(WordOrder.CDAB)]
    [InlineData(WordOrder.BADC)]
    [InlineData(WordOrder.DCBA)]
    public void EncodeDecode_RoundTrip(WordOrder order)
    {
        const float original = -1234.5678f;
        var registers = ValueCodec.Encode(original, TagDataType.Float32, order);
        var decoded = ValueCodec.Decode(registers, TagDataType.Float32, order);
        Assert.Equal(original, Assert.IsType<float>(decoded));
    }

    [Theory]
    [InlineData(TagDataType.UInt16, 1)]
    [InlineData(TagDataType.Int32, 2)]
    [InlineData(TagDataType.Float32, 2)]
    [InlineData(TagDataType.Float64, 4)]
    public void RegisterCount_IsCorrect(TagDataType type, int expected)
        => Assert.Equal(expected, ValueCodec.RegisterCount(type));
}
