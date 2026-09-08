namespace Iiot.Abstractions;

/// <summary>
/// 数据质量码。工业场景里"读到了但不可信"和"没读到"必须区分开,
/// 否则看板上会把通信故障期间的旧值当成真实值展示。
/// </summary>
public enum Quality
{
    /// <summary>本次采集成功,值可信。</summary>
    Good,

    /// <summary>通信失败 / 超时 / 设备返回异常码,值不可用。</summary>
    Bad,

    /// <summary>值是上一周期的缓存,或来自降级路径。展示时应标记为陈旧。</summary>
    Uncertain
}
