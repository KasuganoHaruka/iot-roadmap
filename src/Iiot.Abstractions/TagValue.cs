namespace Iiot.Abstractions;

/// <summary>
/// 一次采集的结果。
/// </summary>
/// <param name="Name">点位名。</param>
/// <param name="Value">缩放后的值;Quality 不为 Good 时为 null。</param>
/// <param name="Quality">质量码。</param>
/// <param name="TimestampUtc">
/// **边缘采集时刻**,不是平台接收时刻。补传的数据也用这个时间入库,
/// 所以看板必须按时间排序渲染,而不是按到达顺序追加。
/// </param>
public readonly record struct TagValue(
    string Name,
    object? Value,
    Quality Quality,
    DateTimeOffset TimestampUtc)
{
    public static TagValue Good(string name, object value, DateTimeOffset ts) =>
        new(name, value, Quality.Good, ts);

    public static TagValue Bad(string name, DateTimeOffset ts) =>
        new(name, null, Quality.Bad, ts);
}
