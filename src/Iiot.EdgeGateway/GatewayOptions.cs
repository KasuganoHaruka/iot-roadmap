namespace Iiot.EdgeGateway;

public sealed class GatewayOptions
{
    /// <summary>站点标识,用于 MQTT 主题第一层:iiot/{Site}/...</summary>
    public string Site { get; set; } = "default";

    /// <summary>点表文件路径。支持热加载 —— 改点表不需要重启服务。</summary>
    public string TagFile { get; set; } = "tags.json";

    public MqttOptions Mqtt { get; set; } = new();

    public BufferOptions Buffer { get; set; } = new();
}

public sealed class MqttOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string ClientId { get; set; } = "edge-gateway";
    public string? Username { get; set; }
    public string? Password { get; set; }

    /// <summary>Keep Alive。离线检测延迟约等于这个值的 1.5 倍。</summary>
    public int KeepAliveSeconds { get; set; } = 30;

    /// <summary>一次发布多少条。批量能显著提高弱网下的吞吐。</summary>
    public int BatchSize { get; set; } = 100;
}

public sealed class BufferOptions
{
    public string DatabasePath { get; set; } = "data/buffer.db";

    /// <summary>
    /// 最大保留条数。**磁盘也有上限** —— 超了丢最旧的。
    /// 工业场景通常接受丢历史数据,不接受网关因为磁盘满而挂掉。
    /// </summary>
    public long MaxRows { get; set; } = 5_000_000;

    /// <summary>已发送数据保留多久后清理。</summary>
    public TimeSpan RetainSentFor { get; set; } = TimeSpan.FromHours(1);
}
