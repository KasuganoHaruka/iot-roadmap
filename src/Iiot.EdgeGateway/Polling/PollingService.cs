using Iiot.Abstractions;
using Iiot.EdgeGateway.Buffering;

namespace Iiot.EdgeGateway.Polling;

/// <summary>
/// 采集调度。W4 + W7 任务。
///
/// 要点:
///   - **每设备独立的采集周期和优先级**,不能一个循环打天下
///   - 慢设备/离线设备隔离到"探活队列",不拖垮整条总线的周期
///   - 连续失败 N 次(建议 3)才判离线,连续成功 M 次才判恢复 —— 去抖,避免误报
///   - 离线后指数退避重连:1s → 2s → 4s → 上限 30s
///   - 同一条 485 总线上的设备**必须串行**(半双工共享介质),
///     不同总线之间才能并行。这一点写多线程时极易搞错。
/// </summary>
public sealed class PollingService(
    IMeasurementBuffer buffer,
    ILogger<PollingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("采集服务启动");

        // TODO(W7):
        //   1) 从点表加载设备与点位
        //   2) 每条总线一个 Task,总线内的设备串行轮询
        //   3) 每轮:driver.ReadAsync(tags) → 转 BufferedMeasurement → buffer.AppendAsync
        //   4) 更新 DriverStatistics,离线判定与退避重连
        _ = buffer;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
