using Iiot.Abstractions;

namespace Iiot.Drivers.Modbus;

/// <summary>一次实际发出的读请求。</summary>
/// <param name="Area">存储区。</param>
/// <param name="StartOffset">起始协议地址。</param>
/// <param name="Count">寄存器/线圈数量。</param>
/// <param name="Tags">这一请求覆盖的点位。</param>
public sealed record ReadRequest(
    ModbusArea Area,
    ushort StartOffset,
    ushort Count,
    IReadOnlyList<TagDefinition> Tags);

/// <summary>
/// 把离散的点位地址合并成**最少的读请求**。
///
/// 这是轮询性能的关键,也是面试的加分点:
/// 100 个散布在 0–200 的点位,逐点读要 100 次往返(9600 波特下约 4 秒),
/// 合并后可能只要 2 次(约 80 毫秒)。
///
/// 约束:
///   - FC03/04 单次最多 **125** 个寄存器;FC01/02 单次最多 **2000** 个线圈
///   - 两个点位之间的空洞超过 maxGap 就不合并(读无用寄存器也要花时间,
///     而且有些设备的保留地址一读就返回异常码)
///   - 不同存储区绝不合并
///
/// W3 任务:实现 Plan,通过 ReadPlannerTests。
/// </summary>
public static class ReadPlanner
{
    public const int MaxRegistersPerRead = 125;
    public const int MaxCoilsPerRead = 2000;

    /// <param name="tags">待读点位(地址可乱序)。</param>
    /// <param name="maxGap">允许跨越的最大空洞寄存器数。经验值 8–16。</param>
    public static IReadOnlyList<ReadRequest> Plan(
        IReadOnlyList<TagDefinition> tags,
        int maxGap = 8)
    {
        // TODO(W3):
        //   1) 按 Area 分组
        //   2) 组内按 Offset 排序
        //   3) 贪心扫描:能并入当前请求(空洞 <= maxGap 且总长 <= 上限)就并入,否则开新请求
        _ = tags; _ = maxGap;
        throw new NotImplementedException("W3 任务:实现批量读规划");
    }
}
