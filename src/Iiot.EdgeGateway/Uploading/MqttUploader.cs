using Iiot.EdgeGateway.Buffering;
using Microsoft.Extensions.Options;

namespace Iiot.EdgeGateway.Uploading;

/// <summary>
/// MQTT 上报。W7 + W8 任务。
///
/// 主题规范(定下来就不要随便改,权限和订阅都依赖它):
///   iiot/{site}/{line}/{device}/telemetry    遥测数据
///   iiot/{site}/{line}/{device}/status       在线状态(retain + LWT)
///   iiot/{site}/{line}/{device}/cmd          下行指令
///   iiot/{site}/{line}/{device}/cmd_resp     指令响应(带 requestId 关联)
///
/// **点位名放 payload,设备放主题**。把测点名放进主题会导致主题爆炸,
/// 加一个点位就要改所有订阅方。
///
/// QoS 选择:遥测用 QoS1 + 下游去重,比 QoS2 划算得多(QoS2 吞吐能差一半以上)。
/// 下行控制指令才值得用 QoS2。
///
/// LWT:连接时注册遗嘱 {"online":false} 到 status 主题并 retain;
/// 上线后自己发 {"online":true}。注意**正常 DISCONNECT 不触发遗嘱**,
/// 所以优雅退出时要手动发离线消息。
///
/// 续传限速:断网恢复的瞬间会有大量积压数据,必须限速(令牌桶),
/// 否则会把 Broker 和下游数据库打爆 —— 这是真实事故的常见来源。
/// </summary>
public sealed class MqttUploader(
    IMeasurementBuffer buffer,
    IOptions<GatewayOptions> options,
    ILogger<MqttUploader> logger) : BackgroundService
{
    private readonly GatewayOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("上报服务启动,Broker {Host}:{Port}",
            _options.Mqtt.Host, _options.Mqtt.Port);

        // TODO(W7): 连接 Broker(带 LWT)+ 自动重连
        // TODO(W7): 循环 TakePendingAsync → 发布 QoS1 → MarkSentAsync
        // TODO(W8): Polly 重试与熔断、令牌桶限速、积压告警
        _ = buffer;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
