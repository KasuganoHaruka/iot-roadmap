using Iiot.Abstractions;
using Iiot.Drivers.Modbus;
using Xunit;

namespace Iiot.Drivers.Modbus.Tests;

public class ReadPlannerTests
{
    private static TagDefinition Tag(string name, string address, TagDataType type = TagDataType.UInt16)
        => new(name, address, type);

    /// <summary>相邻且空洞小的点位应合并成一次请求。</summary>
    [Fact]
    public void MergesAdjacentTags()
    {
        List<TagDefinition> tags =
        [
            Tag("a", "4x:0"),
            Tag("b", "4x:1"),
            Tag("c", "4x:5")
        ];

        var plan = ReadPlanner.Plan(tags, maxGap: 8);

        var request = Assert.Single(plan);
        Assert.Equal(ModbusArea.HoldingRegister, request.Area);
        Assert.Equal(0, request.StartOffset);
        Assert.Equal(6, request.Count);   // 0..5
    }

    /// <summary>空洞超过 maxGap 就要拆成两次请求 —— 读一大片无用寄存器反而更慢。</summary>
    [Fact]
    public void SplitsOnLargeGap()
    {
        List<TagDefinition> tags =
        [
            Tag("a", "4x:0"),
            Tag("b", "4x:1"),
            Tag("c", "4x:100")
        ];

        var plan = ReadPlanner.Plan(tags, maxGap: 8);

        Assert.Equal(2, plan.Count);
        Assert.Equal(0, plan[0].StartOffset);
        Assert.Equal(2, plan[0].Count);
        Assert.Equal(100, plan[1].StartOffset);
        Assert.Equal(1, plan[1].Count);
    }

    /// <summary>不同存储区绝不能合并。</summary>
    [Fact]
    public void NeverMergesAcrossAreas()
    {
        List<TagDefinition> tags =
        [
            Tag("a", "4x:0"),
            Tag("b", "3x:0")
        ];

        var plan = ReadPlanner.Plan(tags, maxGap: 8);

        Assert.Equal(2, plan.Count);
        Assert.Contains(plan, r => r.Area == ModbusArea.HoldingRegister);
        Assert.Contains(plan, r => r.Area == ModbusArea.InputRegister);
    }

    /// <summary>单次请求不得超过 125 个寄存器(协议上限)。</summary>
    [Fact]
    public void RespectsMaxRegisterLimit()
    {
        var tags = Enumerable.Range(0, 200)
            .Select(i => Tag($"t{i}", $"4x:{i}"))
            .ToList();

        var plan = ReadPlanner.Plan(tags, maxGap: 8);

        Assert.All(plan, r => Assert.True(r.Count <= ReadPlanner.MaxRegistersPerRead));
        Assert.Equal(2, plan.Count);
    }

    /// <summary>32 位点位占两个寄存器,规划时必须把长度算进去。</summary>
    [Fact]
    public void AccountsForMultiRegisterTypes()
    {
        List<TagDefinition> tags = [Tag("f", "4x:10", TagDataType.Float32)];

        var request = Assert.Single(ReadPlanner.Plan(tags));

        Assert.Equal(10, request.StartOffset);
        Assert.Equal(2, request.Count);
    }

    /// <summary>输入乱序时结果必须稳定。</summary>
    [Fact]
    public void HandlesUnsortedInput()
    {
        List<TagDefinition> tags =
        [
            Tag("c", "4x:5"),
            Tag("a", "4x:0"),
            Tag("b", "4x:2")
        ];

        var request = Assert.Single(ReadPlanner.Plan(tags, maxGap: 8));

        Assert.Equal(0, request.StartOffset);
        Assert.Equal(6, request.Count);
        Assert.Equal(3, request.Tags.Count);
    }
}
