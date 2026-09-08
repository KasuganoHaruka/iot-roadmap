using Iiot.EdgeGateway;
using Iiot.EdgeGateway.Buffering;
using Iiot.EdgeGateway.Polling;
using Iiot.EdgeGateway.Uploading;
using Serilog;

// 边缘网关。三个后台服务组成一条流水线:
//   PollingService  采集 → 写入 IMeasurementBuffer
//   MqttUploader    从 buffer 取未发送 → MQTT 发布 → 收到 ACK 后标记已发送
//   MaintenanceService 定期清理已发送数据、上报自身健康
//
// 关键设计:采集和上报**完全解耦**,中间只通过持久化缓冲连接。
// 这样断网时采集照常进行,恢复后自动续传 —— 这就是 M2 的核心成果。

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        fileSizeLimitBytes: 50 * 1024 * 1024)
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((services, config) => config
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.Configure<GatewayOptions>(builder.Configuration.GetSection("Gateway"));

    builder.Services.AddSingleton<IMeasurementBuffer, SqliteMeasurementBuffer>();

    // TODO(W7): 从 tags.json 加载点表,为每台设备构造 IDeviceDriver 并注册进采集调度
    builder.Services.AddHostedService<PollingService>();
    builder.Services.AddHostedService<MqttUploader>();

    var host = builder.Build();
    await host.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "网关启动失败");
    return 1;
}
finally
{
    // 优雅退出:确保日志刷盘。systemd 重启时这一步能保住最后的错误信息。
    await Log.CloseAndFlushAsync();
}
